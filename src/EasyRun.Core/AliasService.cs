using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EasyRun.Core
{
    /// <summary>
    /// The alias engine. Creates, lists, edits and removes aliases across HKCU/HKLM App Paths, builds the
    /// backing .lnk shortcuts, and wires elevated aliases to the elevation service. All UI and the CLI go
    /// through here, so the behaviour is identical and testable.
    /// </summary>
    public sealed class AliasService
    {
        private readonly IAppPathsStore _store;
        private readonly IShortcutWriter _shortcuts;
        private readonly IElevationService _elevation;
        private readonly IEasyRunPaths _paths;
        private readonly string _selfLauncherPath;

        public AliasService(
            IAppPathsStore store,
            IShortcutWriter shortcuts,
            IElevationService elevation,
            IEasyRunPaths paths,
            string selfLauncherPath)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _shortcuts = shortcuts ?? throw new ArgumentNullException(nameof(shortcuts));
            _elevation = elevation ?? throw new ArgumentNullException(nameof(elevation));
            _paths = paths ?? throw new ArgumentNullException(nameof(paths));
            _selfLauncherPath = selfLauncherPath;
        }

        public const string LaunchElevatedSwitch = "--launch-elevated";

        // ---------------------------------------------------------------- reads

        public IReadOnlyList<AliasRecord> List(AliasScope scope)
        {
            var result = new List<AliasRecord>();
            foreach (string key in _store.GetKeyNames(scope))
            {
                IDictionary<string, string> values = _store.Read(scope, key);
                if (values != null) result.Add(AliasRecordMapper.FromValues(scope, key, values));
            }
            return result.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }

        public IReadOnlyList<AliasRecord> ListAll() =>
            List(AliasScope.User).Concat(List(AliasScope.Machine)).ToList();

        public AliasRecord Get(AliasScope scope, string name)
        {
            IDictionary<string, string> values = _store.Read(scope, AliasName.ToKey(name));
            return values == null ? null : AliasRecordMapper.FromValues(scope, AliasName.ToKey(name), values);
        }

        public bool Exists(AliasScope scope, string name) => _store.Exists(scope, AliasName.ToKey(name));

        public AliasRecord Find(string name)
        {
            return Get(AliasScope.User, name) ?? Get(AliasScope.Machine, name);
        }

        // ---------------------------------------------------------------- create / update

        public AliasChangeResult Create(CreateAliasRequest request, AdminCredential credential = null)
        {
            string nameError = AliasName.Validate(request.Name);
            if (nameError != null) throw new EasyRunException(EasyRunError.InvalidArgument, nameError);
            ValidateTarget(request);

            string name = AliasName.StripExe(request.Name);
            string key = AliasName.ToKey(name);

            if (_store.Exists(request.Scope, key) && !request.Overwrite)
            {
                throw new EasyRunException(EasyRunError.Conflict,
                    "An alias named '" + name + "' already exists for " + request.Scope.DisplayName() + ".");
            }

            // If replacing an EasyRun alias, clean up its old backing files/task first.
            AliasRecord previous = Get(request.Scope, name);
            if (previous != null && previous.ManagedByEasyRun) CleanupBacking(previous);

            _paths.EnsureCreated();
            AliasRecord record = Build(name, request, credential);
            _store.Write(request.Scope, key, AliasRecordMapper.ToValues(record));

            string verb = previous == null ? "Created" : "Updated";
            return new AliasChangeResult(true,
                verb + " alias '" + name + "'. Press Win+R and type \"" + name + "\" to run it.", record);
        }

        public AliasChangeResult Remove(AliasScope scope, string name)
        {
            AliasRecord record = Get(scope, name);
            if (record == null)
            {
                return new AliasChangeResult(false, "No alias named '" + AliasName.Display(name) + "' for " + scope.DisplayName() + ".");
            }
            if (record.ManagedByEasyRun) CleanupBacking(record);
            _store.Delete(scope, AliasName.ToKey(name));
            return new AliasChangeResult(true, "Removed alias '" + record.Name + "'.", record);
        }

        /// <summary>Runs an elevated run-as alias by name (called by the alias's shortcut via the EasyRun launcher).</summary>
        public void LaunchElevated(AliasScope scope, string name, ICredentialPrompt prompt)
        {
            AliasRecord record = Get(scope, name) ?? Find(name);
            if (record == null) throw new EasyRunException(EasyRunError.NotFound, "No such alias: " + name);
            if (record.Kind != AliasKind.ElevatedRunAs)
            {
                throw new EasyRunException(EasyRunError.InvalidArgument, "Alias '" + name + "' is not a run-as elevated alias.");
            }

            using (AdminCredential cred = prompt.Prompt(
                "Enter the password for " + record.RunAsAccount + " to run '" + record.Name + "' elevated.",
                record.RunAsAccount))
            {
                if (cred == null) throw new EasyRunException(EasyRunError.Cancelled, "Cancelled.");
                _elevation.LaunchAs(record.Target, record.Arguments, record.WorkingDirectory, cred);
            }
        }

        // ---------------------------------------------------------------- building

        private AliasRecord Build(string name, CreateAliasRequest request, AdminCredential credential)
        {
            var record = new AliasRecord
            {
                Name = name,
                Scope = request.Scope,
                Kind = request.Kind,
                Target = NormalizeTarget(request),
                Arguments = request.Arguments ?? string.Empty,
                WorkingDirectory = request.WorkingDirectory ?? string.Empty,
                Description = request.Description ?? string.Empty,
                RunAsAccount = request.RunAsAccount,
                ManagedByEasyRun = true,
            };

            switch (request.Kind)
            {
                case AliasKind.Executable:
                    record.LaunchValue = record.Target;
                    break;

                case AliasKind.Folder:
                    record.ShortcutFile = ShortcutPath(name);
                    _shortcuts.Create(new ShortcutSpec
                    {
                        Path = record.ShortcutFile,
                        TargetPath = ExplorerPath(),
                        Arguments = Quote(record.Target),
                        Description = "EasyRun folder alias: " + name,
                    });
                    record.LaunchValue = record.ShortcutFile;
                    break;

                case AliasKind.Command:
                    record.ShortcutFile = ShortcutPath(name);
                    _shortcuts.Create(new ShortcutSpec
                    {
                        Path = record.ShortcutFile,
                        TargetPath = record.Target,
                        Arguments = record.Arguments,
                        WorkingDirectory = record.WorkingDirectory,
                        Description = "EasyRun alias: " + name,
                        IconLocation = record.Target,
                    });
                    record.LaunchValue = record.ShortcutFile;
                    break;

                case AliasKind.ElevatedTask:
                    BuildElevatedTask(name, record, credential);
                    break;

                case AliasKind.ElevatedRunAs:
                    BuildElevatedRunAs(name, record);
                    break;

                default:
                    throw new EasyRunException(EasyRunError.InvalidArgument, "Unknown alias kind.");
            }

            return record;
        }

        private void BuildElevatedTask(string name, AliasRecord record, AdminCredential credential)
        {
            if (credential == null)
            {
                throw new EasyRunException(EasyRunError.InvalidArgument, "Admin credentials are required to create an elevated (task) alias.");
            }
            record.RunAsAccount = credential.UserName;
            record.TaskName = TaskNameFor(name);
            _elevation.RegisterElevatedTask(new ElevatedTaskSpec
            {
                TaskName = record.TaskName,
                Target = record.Target,
                Arguments = record.Arguments,
                WorkingDirectory = record.WorkingDirectory,
                Description = "EasyRun elevated alias '" + name + "'",
            }, credential);

            record.ShortcutFile = ShortcutPath(name);
            _shortcuts.Create(new ShortcutSpec
            {
                Path = record.ShortcutFile,
                TargetPath = SchTasksPath(),
                Arguments = "/run /tn " + Quote(record.TaskName),
                Description = "EasyRun elevated alias: " + name,
                IconLocation = record.Target,
                Hidden = true,
            });
            record.LaunchValue = record.ShortcutFile;
        }

        private void BuildElevatedRunAs(string name, AliasRecord record)
        {
            if (string.IsNullOrEmpty(_selfLauncherPath))
            {
                throw new EasyRunException(EasyRunError.Failed, "The EasyRun launcher path is not configured.");
            }
            if (string.IsNullOrEmpty(record.RunAsAccount))
            {
                throw new EasyRunException(EasyRunError.InvalidArgument, "An admin account is required for a run-as alias.");
            }
            record.ShortcutFile = ShortcutPath(name);
            _shortcuts.Create(new ShortcutSpec
            {
                Path = record.ShortcutFile,
                TargetPath = _selfLauncherPath,
                Arguments = LaunchElevatedSwitch + " " + Quote(name) + " --scope " + record.Scope,
                Description = "EasyRun elevated (run as) alias: " + name,
                IconLocation = record.Target,
            });
            record.LaunchValue = record.ShortcutFile;
        }

        private void CleanupBacking(AliasRecord record)
        {
            if (!string.IsNullOrEmpty(record.ShortcutFile))
            {
                try
                {
                    _shortcuts.Delete(record.ShortcutFile);
                }
                catch (IOException)
                {
                    // Best effort; a leftover .lnk is harmless.
                }
            }
            if (record.Kind == AliasKind.ElevatedTask && !string.IsNullOrEmpty(record.TaskName) && _elevation.TaskExists(record.TaskName))
            {
                _elevation.RemoveTask(record.TaskName);
            }
        }

        // ---------------------------------------------------------------- validation / helpers

        private static void ValidateTarget(CreateAliasRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Target))
            {
                throw new EasyRunException(EasyRunError.InvalidArgument, "The target is empty.");
            }
            switch (request.Kind)
            {
                case AliasKind.Folder:
                    if (!LooksLikeFullPath(request.Target))
                    {
                        throw new EasyRunException(EasyRunError.InvalidArgument, "The folder must be a full path such as C:\\Tools.");
                    }
                    break;
                case AliasKind.Executable:
                case AliasKind.Command:
                case AliasKind.ElevatedTask:
                case AliasKind.ElevatedRunAs:
                    if (!LooksLikeFullPath(request.Target))
                    {
                        throw new EasyRunException(EasyRunError.InvalidArgument, "The program must be a full path such as C:\\Tools\\app.exe.");
                    }
                    break;
            }
        }

        private static string NormalizeTarget(CreateAliasRequest request)
        {
            string t = request.Target.Trim().Trim('"');
            return request.Kind == AliasKind.Folder ? t.TrimEnd('\\') : t;
        }

        public static bool LooksLikeFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            string p = path.Trim().Trim('"');
            if (p.Length >= 3 && char.IsLetter(p[0]) && p[1] == ':' && (p[2] == '\\' || p[2] == '/')) return true;
            return p.StartsWith(@"\\", StringComparison.Ordinal);
        }

        private string ShortcutPath(string name) => Path.Combine(_paths.AliasesDirectory, AliasName.ToShortcutFileName(name));

        internal static string TaskNameFor(string name) => "EasyRun_" + AliasName.StripExe(name);

        private static string ExplorerPath() =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");

        private static string SchTasksPath() =>
            Path.Combine(Environment.SystemDirectory, "schtasks.exe");

        private static string Quote(string s) => "\"" + s + "\"";
    }
}

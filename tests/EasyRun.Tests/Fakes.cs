using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyRun.Core;

namespace EasyRun.Tests
{
    internal sealed class FakeAppPathsStore : IAppPathsStore
    {
        private readonly Dictionary<AliasScope, Dictionary<string, Dictionary<string, string>>> _data =
            new Dictionary<AliasScope, Dictionary<string, Dictionary<string, string>>>
            {
                [AliasScope.User] = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase),
                [AliasScope.Machine] = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase),
            };

        public int Writes { get; private set; }
        public int Deletes { get; private set; }

        public IList<string> GetKeyNames(AliasScope scope) => _data[scope].Keys.ToList();

        public IDictionary<string, string> Read(AliasScope scope, string key) =>
            _data[scope].TryGetValue(key, out Dictionary<string, string> v)
                ? new Dictionary<string, string>(v, StringComparer.OrdinalIgnoreCase)
                : null;

        public void Write(AliasScope scope, string key, IDictionary<string, string> values)
        {
            Writes++;
            _data[scope][key] = new Dictionary<string, string>(values, StringComparer.OrdinalIgnoreCase);
        }

        public void Delete(AliasScope scope, string key)
        {
            Deletes++;
            _data[scope].Remove(key);
        }

        public bool Exists(AliasScope scope, string key) => _data[scope].ContainsKey(key);
    }

    internal sealed class FakeShortcutWriter : IShortcutWriter
    {
        public List<ShortcutSpec> Created { get; } = new List<ShortcutSpec>();
        public List<string> Deleted { get; } = new List<string>();

        public void Create(ShortcutSpec spec) => Created.Add(spec);

        public void Delete(string path) => Deleted.Add(path);

        public ShortcutSpec Last => Created.LastOrDefault();
    }

    internal sealed class FakeElevationService : IElevationService
    {
        public List<ElevatedTaskSpec> Registered { get; } = new List<ElevatedTaskSpec>();
        public List<string> Removed { get; } = new List<string>();
        public List<string> Launched { get; } = new List<string>();
        public string LastPassword { get; private set; }
        private readonly HashSet<string> _tasks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public string CurrentUserName => "TESTDOMAIN\\tester";
        public bool IsCurrentProcessElevated => false;

        public void RegisterElevatedTask(ElevatedTaskSpec spec, AdminCredential credential)
        {
            Registered.Add(spec);
            LastPassword = credential.Password;
            _tasks.Add(spec.TaskName);
        }

        public bool TaskExists(string taskName) => _tasks.Contains(taskName);

        public void RemoveTask(string taskName)
        {
            Removed.Add(taskName);
            _tasks.Remove(taskName);
        }

        public void RunTask(string taskName) => Launched.Add("task:" + taskName);

        public void LaunchAs(string target, string arguments, string workingDirectory, AdminCredential credential) =>
            Launched.Add("as:" + credential.UserName + ":" + target + " " + arguments);
    }

    internal sealed class FakeCredentialPrompt : ICredentialPrompt
    {
        private readonly string _user;
        private readonly string _password;

        public FakeCredentialPrompt(string user, string password)
        {
            _user = user;
            _password = password;
        }

        public bool Cancelled { get; set; }

        public AdminCredential Prompt(string message, string defaultUserName) =>
            Cancelled ? null : new AdminCredential(_user ?? defaultUserName, (_password ?? "pw").ToCharArray());
    }

    internal sealed class TestEnv : IDisposable
    {
        public TestEnv()
        {
            Root = Path.Combine(Path.GetTempPath(), "EasyRunTests-" + Guid.NewGuid().ToString("N"));
            Paths = new EasyRunPaths(Root);
            Service = new AliasService(Store, Shortcuts, Elevation, Paths, @"C:\Program Files\EasyRun\EasyRun.exe");
        }

        public string Root { get; }
        public FakeAppPathsStore Store { get; } = new FakeAppPathsStore();
        public FakeShortcutWriter Shortcuts { get; } = new FakeShortcutWriter();
        public FakeElevationService Elevation { get; } = new FakeElevationService();
        public EasyRunPaths Paths { get; }
        public AliasService Service { get; }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
            }
            catch (IOException)
            {
            }
        }
    }
}

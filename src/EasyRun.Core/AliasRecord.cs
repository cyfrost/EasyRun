
namespace EasyRun.Core
{
    /// <summary>
    /// Everything EasyRun knows about one alias. The launch target (the App Paths default value) plus the
    /// EasyRun-specific metadata that lets the manager show and clean up the alias accurately instead of guessing.
    /// </summary>
    public sealed class AliasRecord
    {
        /// <summary>The name the user types in the Run box (no ".exe").</summary>
        public string Name { get; set; }

        public AliasScope Scope { get; set; }

        public AliasKind Kind { get; set; }

        /// <summary>What App Paths launches: the target .exe, or the generated .lnk.</summary>
        public string LaunchValue { get; set; }

        /// <summary>The real thing the user chose (program, folder, or command target).</summary>
        public string Target { get; set; }

        /// <summary>Arguments for a Command/elevated alias (may be empty).</summary>
        public string Arguments { get; set; }

        /// <summary>Working directory (may be empty).</summary>
        public string WorkingDirectory { get; set; }

        /// <summary>Full path of the generated .lnk, when <see cref="Kind"/> uses one.</summary>
        public string ShortcutFile { get; set; }

        /// <summary>Task Scheduler task name, for <see cref="AliasKind.ElevatedTask"/>.</summary>
        public string TaskName { get; set; }

        /// <summary>Admin account an elevated alias runs as (e.g. DOMAIN\\admin). Never stores a password.</summary>
        public string RunAsAccount { get; set; }

        public string Description { get; set; }

        /// <summary>True when EasyRun created this alias (it carries EasyRun metadata values).</summary>
        public bool ManagedByEasyRun { get; set; }

        public string RegistryPath =>
            Scope.RegistryRoot() + @"\Software\Microsoft\Windows\CurrentVersion\App Paths\" + AliasName.ToKey(Name);

        public AliasRecord Clone() => (AliasRecord)MemberwiseClone();
    }
}

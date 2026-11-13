namespace EasyRun.Core
{
    /// <summary>How an alias launches its target. Determines what is written to App Paths.</summary>
    public enum AliasKind
    {
        /// <summary>An executable launched directly (App Paths points straight at the .exe).</summary>
        Executable,

        /// <summary>A folder opened in Explorer (via a generated .lnk).</summary>
        Folder,

        /// <summary>A file or program with arguments / a custom working directory (via a generated .lnk).</summary>
        Command,

        /// <summary>Runs elevated as a stored admin account through a Task Scheduler task.</summary>
        ElevatedTask,

        /// <summary>Runs elevated as an admin account via secondary logon, visible in the current session.</summary>
        ElevatedRunAs,
    }

    public static class AliasKindExtensions
    {
        public static bool IsElevated(this AliasKind kind) =>
            kind == AliasKind.ElevatedTask || kind == AliasKind.ElevatedRunAs;

        /// <summary>True when the alias is backed by a generated .lnk in the aliases folder.</summary>
        public static bool UsesShortcutFile(this AliasKind kind) =>
            kind == AliasKind.Folder || kind == AliasKind.Command || kind == AliasKind.ElevatedTask;

        public static string DisplayName(this AliasKind kind)
        {
            switch (kind)
            {
                case AliasKind.Executable: return "Program";
                case AliasKind.Folder: return "Folder";
                case AliasKind.Command: return "Command";
                case AliasKind.ElevatedTask: return "Elevated (task)";
                case AliasKind.ElevatedRunAs: return "Elevated (run as)";
                default: return kind.ToString();
            }
        }
    }
}

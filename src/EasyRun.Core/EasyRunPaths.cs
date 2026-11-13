using System;
using System.IO;

namespace EasyRun.Core
{
    /// <summary>Per-user locations EasyRun writes to. All under %LOCALAPPDATA%\EasyRun (no admin needed).</summary>
    public interface IEasyRunPaths
    {
        /// <summary>Folder holding generated .lnk shortcuts for folder/command/elevated aliases.</summary>
        string AliasesDirectory { get; }

        /// <summary>Folder holding backups of overwritten aliases.</summary>
        string BackupsDirectory { get; }

        void EnsureCreated();
    }

    public sealed class EasyRunPaths : IEasyRunPaths
    {
        private readonly string _root;

        public EasyRunPaths(string root = null)
        {
            _root = root ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EasyRun");
        }

        public string AliasesDirectory => Path.Combine(_root, "aliases");

        public string BackupsDirectory => Path.Combine(_root, "backups");

        public void EnsureCreated()
        {
            Directory.CreateDirectory(AliasesDirectory);
            Directory.CreateDirectory(BackupsDirectory);
        }
    }
}

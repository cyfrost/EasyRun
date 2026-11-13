namespace EasyRun.Core
{
    /// <summary>Describes a Windows shortcut (.lnk) to create.</summary>
    public sealed class ShortcutSpec
    {
        public string Path { get; set; }
        public string TargetPath { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public string Description { get; set; }
        public string IconLocation { get; set; }

        /// <summary>True to start the target hidden (used for the schtasks trigger, which needs no window).</summary>
        public bool Hidden { get; set; }
    }

    /// <summary>Creates and deletes .lnk shortcut files (or fakes them, in tests).</summary>
    public interface IShortcutWriter
    {
        void Create(ShortcutSpec spec);

        void Delete(string path);
    }
}

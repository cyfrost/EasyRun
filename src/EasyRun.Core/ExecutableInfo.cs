using System;
using System.IO;

namespace EasyRun.Core
{
    /// <summary>Reads the Windows PE subsystem so the UI can tell a GUI program from a console one.</summary>
    public static class ExecutableInfo
    {
        private const ushort ImageSubsystemWindowsGui = 2;
        private const ushort ImageSubsystemWindowsCui = 3;

        /// <summary>
        /// True when the target is best launched visibly in the user's session (a GUI program), so an
        /// elevated shortcut should default to secondary logon rather than a background scheduled task.
        /// Scripts and console programs return false.
        /// </summary>
        public static bool IsGuiProgram(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string ext = Path.GetExtension(path);
            if (ext.Equals(".cmd", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".bat", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".ps1", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".vbs", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            ushort? subsystem = TryReadSubsystem(path);
            // Unknown (not a PE, e.g. a .msc or document): treat as GUI so its window is visible.
            return subsystem != ImageSubsystemWindowsCui;
        }

        /// <summary>Returns the PE subsystem value, or null if the file can't be read as a PE image.</summary>
        public static ushort? TryReadSubsystem(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = new BinaryReader(stream))
                {
                    if (stream.Length < 0x40) return null;
                    stream.Position = 0x3C;
                    int peOffset = reader.ReadInt32();
                    if (peOffset <= 0 || peOffset + 0x5E + 2 > stream.Length) return null;
                    stream.Position = peOffset;
                    if (reader.ReadUInt32() != 0x00004550) return null; // "PE\0\0"

                    stream.Position = peOffset + 24; // skip COFF header to the optional header magic
                    ushort magic = reader.ReadUInt16();
                    // Subsystem sits at optional-header offset 68 for both PE32 (0x10B) and PE32+ (0x20B).
                    if (magic != 0x10B && magic != 0x20B) return null;
                    stream.Position = peOffset + 24 + 68;
                    return reader.ReadUInt16();
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
            {
                return null;
            }
        }
    }
}

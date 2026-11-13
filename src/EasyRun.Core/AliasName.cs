using System;
using System.IO;
using System.Linq;

namespace EasyRun.Core
{
    /// <summary>
    /// Rules for alias names. An alias is typed in the Run box, so it must be a single token with no
    /// spaces, and it is stored under App Paths as "&lt;name&gt;.exe" (the ".exe" suffix is what makes
    /// the Run box resolve the bare name).
    /// </summary>
    public static class AliasName
    {
        public const int MaxLength = 200;

        private static readonly char[] Invalid = "\\/:*?\"<>|; %".ToCharArray();

        /// <summary>Returns an error message, or null when the name is valid.</summary>
        public static string Validate(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "The alias is empty.";
            if (name != name.Trim()) return "The alias must not start or end with spaces.";
            if (name.IndexOf(' ') >= 0) return "The alias must not contain spaces (you type it in the Run box).";
            if (name.IndexOfAny(Invalid) >= 0) return "The alias must not contain any of \\ / : * ? \" < > | ; or %.";
            if (name.Length > MaxLength) return "The alias is longer than " + MaxLength + " characters.";
            if (StripExe(name).Length == 0) return "The alias is empty.";
            return null;
        }

        public static bool IsValid(string name) => Validate(name) == null;

        /// <summary>Removes a single trailing ".exe" (case-insensitive) if present.</summary>
        public static string StripExe(string name)
        {
            if (name == null) return null;
            string trimmed = name.Trim();
            return trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                ? trimmed.Substring(0, trimmed.Length - 4)
                : trimmed;
        }

        /// <summary>The App Paths key name for an alias: always "&lt;name&gt;.exe".</summary>
        public static string ToKey(string name) => StripExe(name) + ".exe";

        /// <summary>The name a user types (no ".exe").</summary>
        public static string Display(string keyOrName) => StripExe(keyOrName);

        /// <summary>A safe file name for a generated .lnk, derived from the alias.</summary>
        public static string ToShortcutFileName(string name)
        {
            string stem = StripExe(name);
            char[] bad = Path.GetInvalidFileNameChars();
            string clean = new string(stem.Select(c => bad.Contains(c) ? '_' : c).ToArray());
            return clean + ".lnk";
        }
    }
}

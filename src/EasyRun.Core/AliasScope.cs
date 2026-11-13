using System;

namespace EasyRun.Core
{
    /// <summary>Where an alias is registered.</summary>
    public enum AliasScope
    {
        /// <summary>HKCU\...\App Paths — the current user only. Never needs administrator rights.</summary>
        User,

        /// <summary>HKLM\...\App Paths — all users. Needs administrator rights to write.</summary>
        Machine,
    }

    public static class AliasScopeExtensions
    {
        public static string DisplayName(this AliasScope scope) => scope == AliasScope.Machine ? "all users" : "just me";

        public static string RegistryRoot(this AliasScope scope) =>
            scope == AliasScope.Machine ? "HKEY_LOCAL_MACHINE" : "HKEY_CURRENT_USER";

        public static AliasScope Parse(string text)
        {
            if (string.Equals(text, "User", StringComparison.OrdinalIgnoreCase)) return AliasScope.User;
            if (string.Equals(text, "Machine", StringComparison.OrdinalIgnoreCase)) return AliasScope.Machine;
            throw new FormatException("Unknown scope '" + text + "'.");
        }
    }
}

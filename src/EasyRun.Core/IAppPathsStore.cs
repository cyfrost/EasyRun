using System.Collections.Generic;

namespace EasyRun.Core
{
    /// <summary>
    /// Low-level access to the App Paths registry area (or a fake, in tests). EasyRun stores the launch target
    /// as the key's default value and its own metadata as extra named values under the same key.
    /// </summary>
    public interface IAppPathsStore
    {
        /// <summary>App Paths key names (e.g. "notepad.exe") present in the given scope.</summary>
        IList<string> GetKeyNames(AliasScope scope);

        /// <summary>
        /// Reads one key's values, or null if the key does not exist. The default value is under the key "".
        /// </summary>
        IDictionary<string, string> Read(AliasScope scope, string key);

        /// <summary>Creates or replaces the key with exactly these values (default value under "").</summary>
        void Write(AliasScope scope, string key, IDictionary<string, string> values);

        void Delete(AliasScope scope, string key);

        bool Exists(AliasScope scope, string key);
    }
}

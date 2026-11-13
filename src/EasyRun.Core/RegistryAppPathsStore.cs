using System;
using System.Collections.Generic;
using System.Security;
using Microsoft.Win32;

namespace EasyRun.Core
{
    /// <summary>The real App Paths store, backed by the Windows registry (64-bit view).</summary>
    public sealed class RegistryAppPathsStore : IAppPathsStore
    {
        internal const string BasePath = @"Software\Microsoft\Windows\CurrentVersion\App Paths";

        public IList<string> GetKeyNames(AliasScope scope)
        {
            return Guard(scope, () =>
            {
                using (RegistryKey baseKey = OpenBase(scope, writable: false))
                {
                    return baseKey == null ? (IList<string>)Array.Empty<string>() : baseKey.GetSubKeyNames();
                }
            });
        }

        public IDictionary<string, string> Read(AliasScope scope, string key)
        {
            return Guard(scope, () =>
            {
                using (RegistryKey baseKey = OpenBase(scope, writable: false))
                using (RegistryKey sub = baseKey?.OpenSubKey(key))
                {
                    if (sub == null) return null;
                    var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string name in sub.GetValueNames())
                    {
                        object v = sub.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
                        values[name] = v?.ToString() ?? string.Empty;
                    }
                    return (IDictionary<string, string>)values;
                }
            });
        }

        public void Write(AliasScope scope, string key, IDictionary<string, string> values)
        {
            Guard<object>(scope, () =>
            {
                using (RegistryKey baseKey = OpenBase(scope, writable: true))
                using (RegistryKey sub = baseKey.CreateSubKey(key))
                {
                    // Remove values that are no longer present so edits don't leave stale metadata.
                    foreach (string existing in sub.GetValueNames())
                    {
                        if (!values.ContainsKey(existing)) sub.DeleteValue(existing, throwOnMissingValue: false);
                    }
                    foreach (KeyValuePair<string, string> pair in values)
                    {
                        sub.SetValue(pair.Key, pair.Value ?? string.Empty, RegistryValueKind.String);
                    }
                }
                return null;
            });
        }

        public void Delete(AliasScope scope, string key)
        {
            Guard<object>(scope, () =>
            {
                using (RegistryKey baseKey = OpenBase(scope, writable: true))
                {
                    baseKey?.DeleteSubKeyTree(key, throwOnMissingSubKey: false);
                }
                return null;
            });
        }

        public bool Exists(AliasScope scope, string key)
        {
            return Guard(scope, () =>
            {
                using (RegistryKey baseKey = OpenBase(scope, writable: false))
                using (RegistryKey sub = baseKey?.OpenSubKey(key))
                {
                    return sub != null;
                }
            });
        }

        private static RegistryKey OpenBase(AliasScope scope, bool writable)
        {
            RegistryHive hive = scope == AliasScope.Machine ? RegistryHive.LocalMachine : RegistryHive.CurrentUser;
            using (RegistryKey root = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64))
            {
                RegistryKey key = root.OpenSubKey(BasePath, writable);
                if (key == null && writable) key = root.CreateSubKey(BasePath);
                return key;
            }
        }

        private static T Guard<T>(AliasScope scope, Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is SecurityException)
            {
                throw new EasyRunException(
                    EasyRunError.NotPermitted,
                    scope == AliasScope.Machine
                        ? "Administrator rights are required to change all-users aliases."
                        : "Access to the user registry was denied.",
                    ex);
            }
        }
    }
}

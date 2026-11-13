using System;
using Microsoft.Win32;

namespace EasyRun.App
{
    /// <summary>
    /// Registers per-user Explorer right-click verbs (HKCU\Software\Classes) that add the clicked file or
    /// folder as an EasyRun alias. No admin, no COM DLL loaded into Explorer.
    /// </summary>
    public static class ShellIntegration
    {
        private const string VerbName = "EasyRun.AddAlias";
        private const string Label = "Add as EasyRun alias";
        private static readonly string[] FileTargets = { @"*\shell", @"Directory\shell" };

        public static bool IsRegistered()
        {
            using (RegistryKey k = OpenClasses(false))
            {
                return k?.OpenSubKey(@"*\shell\" + VerbName) != null;
            }
        }

        public static void Register(string exePath)
        {
            Unregister();
            using (RegistryKey classes = OpenClasses(true))
            {
                WriteVerb(classes, @"*\shell", exePath, "--add-file");
                WriteVerb(classes, @"Directory\shell", exePath, "--add-dir");
            }
            NativeMethods.SHChangeNotify(NativeMethods.SHCNE_ASSOCCHANGED, NativeMethods.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        public static void Unregister()
        {
            using (RegistryKey classes = OpenClasses(true))
            {
                foreach (string target in FileTargets)
                {
                    classes?.DeleteSubKeyTree(target + "\\" + VerbName, throwOnMissingSubKey: false);
                }
            }
            NativeMethods.SHChangeNotify(NativeMethods.SHCNE_ASSOCCHANGED, NativeMethods.SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

        private static void WriteVerb(RegistryKey classes, string target, string exePath, string switchArg)
        {
            using (RegistryKey verb = classes.CreateSubKey(target + "\\" + VerbName))
            {
                verb.SetValue(string.Empty, Label);
                verb.SetValue("Icon", "\"" + exePath + "\",0");
                using (RegistryKey command = verb.CreateSubKey("command"))
                {
                    command.SetValue(string.Empty, "\"" + exePath + "\" " + switchArg + " \"%1\"");
                }
            }
        }

        private static RegistryKey OpenClasses(bool writable)
        {
            using (RegistryKey root = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
            {
                return writable ? root.CreateSubKey(@"Software\Classes") : root.OpenSubKey(@"Software\Classes");
            }
        }
    }
}

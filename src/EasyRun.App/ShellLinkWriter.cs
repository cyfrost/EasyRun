using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using EasyRun.Core;

namespace EasyRun.App
{
    /// <summary>Writes and deletes .lnk shortcut files via the Windows IShellLink COM interface (no extra dependency).</summary>
    internal sealed class ShellLinkWriter : IShortcutWriter
    {
        private const int SwShowNormal = 1;
        private const int SwShowMinNoActive = 7;

        public void Create(ShortcutSpec spec)
        {
            if (spec == null) throw new ArgumentNullException(nameof(spec));
            Directory.CreateDirectory(Path.GetDirectoryName(spec.Path));

            var link = (IShellLinkW)new CShellLink();
            try
            {
                link.SetPath(spec.TargetPath);
                if (!string.IsNullOrEmpty(spec.Arguments)) link.SetArguments(spec.Arguments);
                if (!string.IsNullOrEmpty(spec.WorkingDirectory)) link.SetWorkingDirectory(spec.WorkingDirectory);
                if (!string.IsNullOrEmpty(spec.Description)) link.SetDescription(spec.Description);
                if (!string.IsNullOrEmpty(spec.IconLocation) && File.Exists(spec.IconLocation)) link.SetIconLocation(spec.IconLocation, 0);
                link.SetShowCmd(spec.Hidden ? SwShowMinNoActive : SwShowNormal);
                ((IPersistFile)link).Save(spec.Path, true);
            }
            finally
            {
                Marshal.ReleaseComObject(link);
            }
        }

        public void Delete(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class CShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszFile, int cch, IntPtr pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszName, int cch);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszDir, int cch);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszArgs, int cch);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pszIconPath, int cch, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }
    }
}

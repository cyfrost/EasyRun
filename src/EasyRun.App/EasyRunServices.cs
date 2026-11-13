using System.IO;
using System.Reflection;
using EasyRun.Core;

namespace EasyRun.App
{
    /// <summary>Wires up the real Windows implementations for the GUI and CLI.</summary>
    public static class EasyRunServices
    {
        public static AliasService CreateAliasService()
        {
            var paths = new EasyRunPaths();
            return new AliasService(
                new RegistryAppPathsStore(),
                new ShellLinkWriter(),
                new WindowsElevationService(),
                paths,
                SelfPath(),
                new FileAliasBackupStore(paths));
        }

        /// <summary>Path to EasyRun.exe (the launcher run-as aliases invoke), resolved next to the running assembly.</summary>
        public static string SelfPath()
        {
            string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string easyRun = Path.Combine(dir, "EasyRun.exe");
            if (File.Exists(easyRun)) return easyRun;
            return Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly().Location;
        }
    }
}

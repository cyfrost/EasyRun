using System;
using System.Linq;
using System.Windows.Forms;
using EasyRun.Core;

namespace EasyRun.App
{
    internal static class Program
    {
        internal const string Usage =
            "EasyRun.exe                      Open EasyRun\n" +
            "EasyRun.exe --add-file <path>    Add a program as an alias\n" +
            "EasyRun.exe --add-dir <path>     Add a folder as an alias\n" +
            "EasyRun.exe --launch-elevated <alias> [--scope User|Machine]\n" +
            "EasyRun.exe --register-shell | --unregister-shell\n";

        [STAThread]
        private static int Main(string[] args)
        {
            // Installer hooks (no UI).
            if (args.Length > 0 && (args[0] == "--register-shell" || args[0] == "--unregister-shell"))
            {
                return RunShellRegistration(args[0]);
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => Ui.Error(null, "Unexpected error: " + e.Exception.Message);

            try
            {
                return Dispatch(args);
            }
            catch (EasyRunException ex)
            {
                if (ex.Error != EasyRunError.Cancelled) Ui.Error(null, ex.Message);
                return (int)ex.Error;
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException))
            {
                Ui.Error(null, "Unexpected error: " + ex.Message);
                return 1;
            }
        }

        private static int Dispatch(string[] args)
        {
            AliasService service = EasyRunServices.CreateAliasService();

            if (args.Length == 0)
            {
                using (var main = new MainForm(service))
                {
                    Application.Run(main);
                }
                return 0;
            }

            string command = args[0].ToLowerInvariant();
            switch (command)
            {
                case "--add-file":
                case "--add-dir":
                    {
                        string path = PathArg(args);
                        if (path == null) { Ui.Error(null, "Expected a path.\n\n" + Usage); return 2; }
                        using (var dialog = new AddAliasDialog(service, path, presetIsFolder: command == "--add-dir"))
                        {
                            Application.Run(dialog);
                        }
                        return 0;
                    }

                case "--launch-elevated":
                    {
                        string alias = PathArg(args);
                        if (alias == null) { Ui.Error(null, "Expected an alias name."); return 2; }
                        AliasScope scope = ParseScope(args);
                        using (var prompt = new CredentialDialog())
                        {
                            service.LaunchElevated(scope, alias, prompt);
                        }
                        return 0;
                    }

                case "--help":
                case "-h":
                case "/?":
                    Ui.Info(null, Usage);
                    return 0;

                default:
                    Ui.Error(null, "Unknown option '" + args[0] + "'.\n\n" + Usage);
                    return 2;
            }
        }

        private static string PathArg(string[] args)
        {
            string value = args.Skip(1).FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim().Trim('"');
        }

        private static AliasScope ParseScope(string[] args)
        {
            int i = Array.FindIndex(args, a => string.Equals(a, "--scope", StringComparison.OrdinalIgnoreCase));
            if (i >= 0 && i + 1 < args.Length && Enum.TryParse(args[i + 1], true, out AliasScope scope)) return scope;
            return AliasScope.User;
        }

        private static int RunShellRegistration(string command)
        {
            try
            {
                if (command == "--register-shell") ShellIntegration.Register(EasyRunServices.SelfPath());
                else ShellIntegration.Unregister();
                return 0;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is System.Security.SecurityException)
            {
                return (int)EasyRunError.NotPermitted;
            }
            catch (Exception)
            {
                return 1;
            }
        }
    }
}

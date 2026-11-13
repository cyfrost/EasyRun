using System;
using EasyRun.App;
using EasyRun.Core;

namespace EasyRun.Cli
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                // integrate/unintegrate are handled here so the CLI can toggle Explorer verbs too.
                if (args.Length == 1 && (args[0] == "integrate" || args[0] == "unintegrate"))
                {
                    if (args[0] == "integrate") ShellIntegration.Register(EasyRunServices.SelfPath());
                    else ShellIntegration.Unregister();
                    Console.Out.WriteLine("Explorer integration " + (args[0] == "integrate" ? "enabled." : "disabled."));
                    return 0;
                }

                AliasService service = EasyRunServices.CreateAliasService();
                return new CliApp(service, Console.Out, Console.Error).Run(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("erun: unexpected error: " + ex.Message);
                return (int)EasyRunError.Failed;
            }
        }
    }
}

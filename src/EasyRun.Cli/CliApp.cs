using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using EasyRun.Core;

namespace EasyRun.Cli
{
    /// <summary>The erun command-line tool. Returns process exit codes (see <see cref="EasyRunError"/>).</summary>
    internal sealed class CliApp
    {
        internal const int ExitOk = 0;
        internal const int ExitUsage = 2;

        private readonly AliasService _service;
        private readonly TextWriter _out;
        private readonly TextWriter _err;

        public CliApp(AliasService service, TextWriter output, TextWriter error)
        {
            _service = service;
            _out = output;
            _err = error;
        }

        public int Run(string[] args)
        {
            try
            {
                if (args.Length == 0) { _out.WriteLine(Usage); return ExitUsage; }
                string command = args[0].ToLowerInvariant();
                List<string> rest = args.Skip(1).ToList();

                switch (command)
                {
                    case "add": return Add(rest);
                    case "list":
                    case "ls": return List(rest);
                    case "remove":
                    case "rm": return Remove(rest);
                    case "run": return RunAlias(rest);
                    case "help":
                    case "--help":
                    case "-h":
                    case "/?": _out.WriteLine(Usage); return ExitOk;
                    case "version":
                    case "--version": _out.WriteLine("erun " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3)); return ExitOk;
                    default: _err.WriteLine("erun: unknown command '" + command + "'. Run 'erun help'."); return ExitUsage;
                }
            }
            catch (EasyRunException ex)
            {
                _err.WriteLine("erun: " + ex.Message);
                return (int)ex.Error;
            }
        }

        private int Add(List<string> args)
        {
            bool dir = Take(args, "--dir");
            bool machine = Take(args, "--machine");
            bool force = Take(args, "--force");
            string name = TakeValue(args, "--name");
            string aliasArgs = TakeValue(args, "--args");
            string target = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));
            if (string.IsNullOrWhiteSpace(target)) { _err.WriteLine("erun: add needs a target path."); return ExitUsage; }
            target = target.Trim('"');

            if (name == null)
            {
                name = dir ? new DirectoryInfo(target.TrimEnd('\\')).Name : Path.GetFileNameWithoutExtension(target);
            }

            AliasKind kind = dir ? AliasKind.Folder : string.IsNullOrEmpty(aliasArgs) ? AliasKind.Executable : AliasKind.Command;
            var request = new CreateAliasRequest
            {
                Name = name,
                Scope = machine ? AliasScope.Machine : AliasScope.User,
                Kind = kind,
                Target = target,
                Arguments = aliasArgs,
                Overwrite = force,
            };
            AliasChangeResult result = _service.Create(request);
            _out.WriteLine(result.Message);
            return ExitOk;
        }

        private int List(List<string> args)
        {
            bool user = Take(args, "--user");
            bool machine = Take(args, "--machine");
            IEnumerable<AliasRecord> records =
                user && !machine ? _service.List(AliasScope.User) :
                machine && !user ? _service.List(AliasScope.Machine) :
                _service.ListAll();

            int n = 0;
            foreach (AliasRecord r in records)
            {
                n++;
                string runs = r.Kind.IsElevated() ? "  [as " + r.RunAsAccount + "]" : string.Empty;
                _out.WriteLine(string.Format(System.Globalization.CultureInfo.CurrentCulture,
                    "{0,-20} {1,-16} {2}{3}", r.Name, r.Kind.DisplayName(), r.Target ?? r.LaunchValue, runs));
            }
            if (n == 0) _out.WriteLine("No aliases.");
            return ExitOk;
        }

        private int Remove(List<string> args)
        {
            bool machine = Take(args, "--machine");
            string name = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));
            if (string.IsNullOrWhiteSpace(name)) { _err.WriteLine("erun: remove needs an alias name."); return ExitUsage; }
            AliasScope scope = machine ? AliasScope.Machine : AliasScope.User;
            AliasChangeResult result = _service.Remove(scope, name);
            _out.WriteLine(result.Message);
            return result.Changed ? ExitOk : (int)EasyRunError.NotFound;
        }

        private int RunAlias(List<string> args)
        {
            string name = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal));
            if (string.IsNullOrWhiteSpace(name)) { _err.WriteLine("erun: run needs an alias name."); return ExitUsage; }
            AliasRecord r = _service.Find(name);
            if (r == null) { _err.WriteLine("erun: no such alias: " + name); return (int)EasyRunError.NotFound; }
            using (Process.Start(new ProcessStartInfo(r.LaunchValue) { UseShellExecute = true }))
            {
            }
            return ExitOk;
        }

        private static bool Take(List<string> args, string flag)
        {
            int i = args.FindIndex(a => string.Equals(a, flag, StringComparison.OrdinalIgnoreCase));
            if (i < 0) return false;
            args.RemoveAt(i);
            return true;
        }

        private static string TakeValue(List<string> args, string flag)
        {
            int i = args.FindIndex(a => string.Equals(a, flag, StringComparison.OrdinalIgnoreCase));
            if (i < 0 || i + 1 >= args.Count) return null;
            string value = args[i + 1];
            args.RemoveAt(i + 1);
            args.RemoveAt(i);
            return value.Trim('"');
        }

        internal const string Usage =
@"EasyRun - run things from the Run box.

Usage: erun <command> [arguments]

  add <path>            Add an alias for a program
      --dir               the path is a folder (opens in Explorer)
      --args ""<args>""      make it a command with arguments
      --name <alias>      alias to type in Run (default: the file/folder name)
      --machine           all users (needs admin);  --force  overwrite
  list [--user|--machine] List aliases
  remove <alias>        Remove an alias            (--machine)
  run <alias>           Run an alias now
  version | help

Elevated shortcuts are created from the EasyRun window (they need admin or a credential prompt).";
    }
}

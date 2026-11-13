using System.IO;
using EasyRun.Cli;
using EasyRun.Core;
using Xunit;

namespace EasyRun.Tests
{
    public class CliTests
    {
        private static (int code, string output, string error) Run(TestEnv env, params string[] args)
        {
            var output = new StringWriter();
            var error = new StringWriter();
            int code = new CliApp(env.Service, output, error).Run(args);
            return (code, output.ToString(), error.ToString());
        }

        [Fact]
        public void Add_defaults_name_from_file()
        {
            using (var env = new TestEnv())
            {
                var (code, _, _) = Run(env, "add", @"C:\Tools\devenv.exe");
                Assert.Equal(0, code);
                Assert.NotNull(env.Service.Get(AliasScope.User, "devenv"));
            }
        }

        [Fact]
        public void Add_dir_and_args()
        {
            using (var env = new TestEnv())
            {
                Run(env, "add", @"C:\work", "--dir", "--name", "w");
                Assert.Equal(AliasKind.Folder, env.Service.Get(AliasScope.User, "w").Kind);

                Run(env, "add", @"C:\tools\py.exe", "--name", "serve", "--args", "-m http.server");
                AliasRecord r = env.Service.Get(AliasScope.User, "serve");
                Assert.Equal(AliasKind.Command, r.Kind);
                Assert.Equal("-m http.server", r.Arguments);
            }
        }

        [Fact]
        public void Add_without_target_is_usage_error()
        {
            using (var env = new TestEnv())
            {
                Assert.Equal(CliApp.ExitUsage, Run(env, "add").code);
            }
        }

        [Fact]
        public void List_and_remove()
        {
            using (var env = new TestEnv())
            {
                Run(env, "add", @"C:\a\x.exe", "--name", "x");
                var (code, output, _) = Run(env, "list");
                Assert.Equal(0, code);
                Assert.Contains("x", output);

                var (rc, _, _) = Run(env, "remove", "x");
                Assert.Equal(0, rc);
                Assert.Null(env.Service.Get(AliasScope.User, "x"));
            }
        }

        [Fact]
        public void Remove_missing_returns_not_found()
        {
            using (var env = new TestEnv())
            {
                Assert.Equal((int)EasyRunError.NotFound, Run(env, "remove", "ghost").code);
            }
        }

        [Fact]
        public void Unknown_command_is_usage_error()
        {
            using (var env = new TestEnv())
            {
                Assert.Equal(CliApp.ExitUsage, Run(env, "frobnicate").code);
                Assert.Equal(CliApp.ExitUsage, Run(env).code);
            }
        }

        [Fact]
        public void Help_and_version_succeed()
        {
            using (var env = new TestEnv())
            {
                Assert.Equal(0, Run(env, "help").code);
                Assert.Equal(0, Run(env, "version").code);
            }
        }
    }
}

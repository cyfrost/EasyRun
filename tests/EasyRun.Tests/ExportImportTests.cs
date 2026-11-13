using System.IO;
using EasyRun.Core;
using Xunit;

namespace EasyRun.Tests
{
    public class ExportImportTests
    {
        [Fact]
        public void Export_includes_only_managed_aliases_without_secrets()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "vs", Target = @"C:\a\devenv.exe" });
                env.Service.Create(new CreateAliasRequest { Name = "serve", Kind = AliasKind.Command, Target = @"C:\p\py.exe", Arguments = "-m http.server" });
                // A foreign (unmanaged) alias should not be exported.
                env.Store.Write(AliasScope.Machine, "notepad.exe",
                    new System.Collections.Generic.Dictionary<string, string> { [""] = @"C:\Windows\notepad.exe" });

                var items = env.Service.GetExportItems();
                Assert.Equal(2, items.Count);
                Assert.DoesNotContain(items, i => i.Name == "notepad");
                Assert.Contains(items, i => i.Name == "serve" && i.Arguments == "-m http.server");
            }
        }

        [Fact]
        public void Save_then_Load_round_trips_through_a_file()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "vs", Target = @"C:\a\devenv.exe", Description = "editor" });
                string file = Path.Combine(env.Root, "aliases.json");
                Directory.CreateDirectory(env.Root);
                AliasExport.Save(file, env.Service.GetExportItems());

                var loaded = AliasExport.Load(file);
                Assert.Single(loaded);
                Assert.Equal("vs", loaded[0].Name);
                Assert.Equal(@"C:\a\devenv.exe", loaded[0].Target);
                Assert.Equal("editor", loaded[0].Description);
            }
        }

        [Fact]
        public void Import_recreates_normal_aliases_and_skips_elevated_tasks()
        {
            using (var env = new TestEnv())
            {
                var items = new[]
                {
                    new AliasExportItem { Name = "vs", Scope = "User", Kind = "Executable", Target = @"C:\a\devenv.exe" },
                    new AliasExportItem { Name = "admincmd", Scope = "User", Kind = "ElevatedRunAs", Target = @"C:\Windows\System32\mmc.exe", RunAsAccount = @"DOM\admin" },
                    new AliasExportItem { Name = "fixit", Scope = "User", Kind = "ElevatedTask", Target = @"C:\m.cmd", RunAsAccount = @"DOM\admin" },
                };

                ImportSummary summary = env.Service.Import(items, overwrite: false);

                Assert.Equal(2, summary.Created); // executable + run-as
                Assert.Single(summary.Skipped);   // elevated task needs a password
                Assert.Contains(summary.Skipped, s => s.StartsWith("fixit", System.StringComparison.Ordinal));
                Assert.NotNull(env.Service.Get(AliasScope.User, "vs"));
                Assert.NotNull(env.Service.Get(AliasScope.User, "admincmd"));
                Assert.Null(env.Service.Get(AliasScope.User, "fixit"));
            }
        }

        [Fact]
        public void Import_without_overwrite_skips_existing()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "vs", Target = @"C:\old\devenv.exe" });
                var items = new[] { new AliasExportItem { Name = "vs", Scope = "User", Kind = "Executable", Target = @"C:\new\devenv.exe" } };

                ImportSummary summary = env.Service.Import(items, overwrite: false);
                Assert.Equal(0, summary.Created);
                Assert.Single(summary.Skipped);
                Assert.Equal(@"C:\old\devenv.exe", env.Service.Get(AliasScope.User, "vs").Target);

                ImportSummary forced = env.Service.Import(items, overwrite: true);
                Assert.Equal(1, forced.Created);
                Assert.Equal(@"C:\new\devenv.exe", env.Service.Get(AliasScope.User, "vs").Target);
            }
        }

        [Fact]
        public void Load_rejects_a_non_easyrun_file()
        {
            using (var env = new TestEnv())
            {
                Directory.CreateDirectory(env.Root);
                string file = Path.Combine(env.Root, "bad.json");
                File.WriteAllText(file, "this is not json at all {{{");
                Assert.Throws<EasyRunException>(() => AliasExport.Load(file));
            }
        }
    }
}

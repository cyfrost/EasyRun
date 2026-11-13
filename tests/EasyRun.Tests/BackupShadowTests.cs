using EasyRun.Core;
using Xunit;

namespace EasyRun.Tests
{
    public class BackupTests
    {
        [Fact]
        public void Overwriting_a_managed_alias_backs_up_the_previous_value()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "vs", Target = @"C:\old\devenv.exe" });
                env.Service.Create(new CreateAliasRequest { Name = "vs", Target = @"C:\new\devenv.exe", Overwrite = true });

                Assert.Equal(1, env.BackupStore.Count);
                AliasBackup b = env.BackupStore.List()[0];
                Assert.Equal("vs", b.Item.Name);
                Assert.Equal(@"C:\old\devenv.exe", b.Item.Target);
            }
        }

        [Fact]
        public void Removing_a_managed_alias_backs_it_up()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "vs", Target = @"C:\a\devenv.exe" });
                env.Service.Remove(AliasScope.User, "vs");
                Assert.Equal(1, env.BackupStore.Count);
            }
        }

        [Fact]
        public void First_create_does_not_back_up()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "vs", Target = @"C:\a\devenv.exe" });
                Assert.Equal(0, env.BackupStore.Count);
            }
        }

        [Fact]
        public void Restore_recreates_the_alias()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "vs", Target = @"C:\a\devenv.exe" });
                env.Service.Remove(AliasScope.User, "vs");
                Assert.Null(env.Service.Get(AliasScope.User, "vs"));

                AliasBackup b = env.BackupStore.List()[0];
                AliasChangeResult r = env.Service.RestoreBackup(b, overwrite: true);
                Assert.True(r.Changed);
                Assert.Equal(@"C:\a\devenv.exe", env.Service.Get(AliasScope.User, "vs").Target);
            }
        }

        [Fact]
        public void File_backup_store_round_trips_and_prunes()
        {
            using (var env = new TestEnv())
            {
                var store = new FileAliasBackupStore(env.Paths, keep: 3);
                for (int i = 0; i < 5; i++)
                {
                    store.Save(new AliasExportItem { Name = "a" + i, Scope = "User", Kind = "Executable", Target = @"C:\x\a" + i + ".exe" });
                }
                var list = store.List();
                Assert.Equal(3, list.Count);
                AliasBackup newest = list[0];
                AliasBackup loaded = store.Load(newest.Id);
                Assert.Equal(newest.Item.Name, loaded.Item.Name);
            }
        }
    }

    public class ShadowTests
    {
        [Fact]
        public void Detects_names_present_in_both_scopes()
        {
            using (var env = new TestEnv())
            {
                env.Store.Write(AliasScope.User, "git.exe", Dict(@"C:\user\git.exe"));
                env.Store.Write(AliasScope.Machine, "git.exe", Dict(@"C:\machine\git.exe"));
                env.Store.Write(AliasScope.Machine, "node.exe", Dict(@"C:\machine\node.exe"));

                var shadowed = env.Service.GetShadowedNames();
                Assert.Contains("git", shadowed);
                Assert.DoesNotContain("node", shadowed);
            }
        }

        private static System.Collections.Generic.Dictionary<string, string> Dict(string target) =>
            new System.Collections.Generic.Dictionary<string, string> { [""] = target };
    }

    public class TestRunTests
    {
        [Fact]
        public void TestLaunchAs_delegates_to_elevation()
        {
            using (var env = new TestEnv())
            using (var cred = new AdminCredential(@"DOM\admin", "pw".ToCharArray()))
            {
                env.Service.TestLaunchAs(@"C:\Windows\System32\mmc.exe", "compmgmt.msc", null, cred);
                Assert.Contains(env.Elevation.Launched, l => l.Contains("mmc.exe") && l.Contains("DOM\\admin"));
            }
        }
    }
}

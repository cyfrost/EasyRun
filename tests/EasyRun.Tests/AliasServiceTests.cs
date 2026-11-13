using System.Linq;
using EasyRun.Core;
using Xunit;

namespace EasyRun.Tests
{
    public class AliasServiceTests
    {
        [Fact]
        public void Create_executable_points_app_paths_straight_at_the_exe()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest
                {
                    Name = "vs",
                    Scope = AliasScope.User,
                    Kind = AliasKind.Executable,
                    Target = @"C:\Tools\devenv.exe",
                });

                AliasRecord r = env.Service.Get(AliasScope.User, "vs");
                Assert.NotNull(r);
                Assert.Equal(@"C:\Tools\devenv.exe", r.LaunchValue);
                Assert.True(r.ManagedByEasyRun);
                Assert.Empty(env.Shortcuts.Created); // no .lnk for a plain exe
                Assert.True(env.Store.Exists(AliasScope.User, "vs.exe"));
            }
        }

        [Fact]
        public void Create_folder_builds_an_explorer_shortcut()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest
                {
                    Name = "docs",
                    Kind = AliasKind.Folder,
                    Target = @"C:\Users\me\Documents\",
                });

                AliasRecord r = env.Service.Get(AliasScope.User, "docs");
                Assert.Equal(AliasKind.Folder, r.Kind);
                Assert.Equal(@"C:\Users\me\Documents", r.Target); // trailing slash trimmed
                Assert.Single(env.Shortcuts.Created);
                ShortcutSpec lnk = env.Shortcuts.Last;
                Assert.EndsWith("explorer.exe", lnk.TargetPath, System.StringComparison.OrdinalIgnoreCase);
                Assert.Contains("Documents", lnk.Arguments);
                Assert.Equal(lnk.Path, r.LaunchValue);
            }
        }

        [Fact]
        public void Create_command_stores_arguments_in_the_shortcut()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest
                {
                    Name = "serve",
                    Kind = AliasKind.Command,
                    Target = @"C:\tools\python.exe",
                    Arguments = "-m http.server 8080",
                    WorkingDirectory = @"C:\www",
                });

                ShortcutSpec lnk = env.Shortcuts.Last;
                Assert.Equal(@"C:\tools\python.exe", lnk.TargetPath);
                Assert.Equal("-m http.server 8080", lnk.Arguments);
                Assert.Equal(@"C:\www", lnk.WorkingDirectory);
                AliasRecord r = env.Service.Get(AliasScope.User, "serve");
                Assert.Equal("-m http.server 8080", r.Arguments);
            }
        }

        [Fact]
        public void Duplicate_without_overwrite_is_a_conflict()
        {
            using (var env = new TestEnv())
            {
                var req = new CreateAliasRequest { Name = "vs", Target = @"C:\a\b.exe" };
                env.Service.Create(req);
                EasyRunException ex = Assert.Throws<EasyRunException>(() => env.Service.Create(req));
                Assert.Equal(EasyRunError.Conflict, ex.Error);
            }
        }

        [Fact]
        public void Overwrite_cleans_up_the_previous_shortcut()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "x", Kind = AliasKind.Folder, Target = @"C:\one" });
                string firstLnk = env.Shortcuts.Last.Path;
                env.Service.Create(new CreateAliasRequest { Name = "x", Kind = AliasKind.Executable, Target = @"C:\two\app.exe", Overwrite = true });

                Assert.Contains(firstLnk, env.Shortcuts.Deleted);
                Assert.Equal(@"C:\two\app.exe", env.Service.Get(AliasScope.User, "x").LaunchValue);
            }
        }

        [Fact]
        public void Remove_deletes_key_and_shortcut()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "docs", Kind = AliasKind.Folder, Target = @"C:\d" });
                string lnk = env.Shortcuts.Last.Path;

                AliasChangeResult result = env.Service.Remove(AliasScope.User, "docs");
                Assert.True(result.Changed);
                Assert.False(env.Store.Exists(AliasScope.User, "docs.exe"));
                Assert.Contains(lnk, env.Shortcuts.Deleted);
            }
        }

        [Fact]
        public void Remove_missing_alias_is_a_no_op()
        {
            using (var env = new TestEnv())
            {
                Assert.False(env.Service.Remove(AliasScope.User, "nope").Changed);
            }
        }

        [Fact]
        public void Elevated_task_registers_a_task_and_a_schtasks_shortcut()
        {
            using (var env = new TestEnv())
            using (var cred = new AdminCredential(@"DOM\admin", "secret".ToCharArray()))
            {
                env.Service.Create(new CreateAliasRequest
                {
                    Name = "fixit",
                    Kind = AliasKind.ElevatedTask,
                    Target = @"C:\tools\maint.cmd",
                    RunAsAccount = @"DOM\admin",
                }, cred);

                Assert.Single(env.Elevation.Registered);
                ElevatedTaskSpec task = env.Elevation.Registered[0];
                Assert.Equal("EasyRun_fixit", task.TaskName);
                Assert.Equal(@"C:\tools\maint.cmd", task.Target);
                Assert.Equal("secret", env.Elevation.LastPassword);

                ShortcutSpec lnk = env.Shortcuts.Last;
                Assert.EndsWith("schtasks.exe", lnk.TargetPath, System.StringComparison.OrdinalIgnoreCase);
                Assert.Contains("/run /tn \"EasyRun_fixit\"", lnk.Arguments);
                Assert.True(lnk.Hidden);

                AliasRecord r = env.Service.Get(AliasScope.User, "fixit");
                Assert.Equal(@"DOM\admin", r.RunAsAccount);
                Assert.Equal("EasyRun_fixit", r.TaskName);
            }
        }

        [Fact]
        public void Elevated_task_without_credentials_is_rejected()
        {
            using (var env = new TestEnv())
            {
                EasyRunException ex = Assert.Throws<EasyRunException>(() => env.Service.Create(new CreateAliasRequest
                {
                    Name = "fixit",
                    Kind = AliasKind.ElevatedTask,
                    Target = @"C:\tools\maint.cmd",
                    RunAsAccount = @"DOM\admin",
                }));
                Assert.Equal(EasyRunError.InvalidArgument, ex.Error);
            }
        }

        [Fact]
        public void Removing_an_elevated_task_alias_also_removes_the_task()
        {
            using (var env = new TestEnv())
            using (var cred = new AdminCredential(@"DOM\admin", "secret".ToCharArray()))
            {
                env.Service.Create(new CreateAliasRequest { Name = "fixit", Kind = AliasKind.ElevatedTask, Target = @"C:\m.cmd", RunAsAccount = @"DOM\admin" }, cred);
                env.Service.Remove(AliasScope.User, "fixit");
                Assert.Contains("EasyRun_fixit", env.Elevation.Removed);
            }
        }

        [Fact]
        public void Elevated_runas_builds_a_launcher_shortcut_and_needs_no_credential()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest
                {
                    Name = "admincmd",
                    Kind = AliasKind.ElevatedRunAs,
                    Target = @"C:\Windows\System32\mmc.exe",
                    RunAsAccount = @"DOM\admin",
                });

                Assert.Empty(env.Elevation.Registered); // no task
                ShortcutSpec lnk = env.Shortcuts.Last;
                Assert.EndsWith("EasyRun.exe", lnk.TargetPath, System.StringComparison.OrdinalIgnoreCase);
                Assert.Contains(AliasService.LaunchElevatedSwitch, lnk.Arguments);
                Assert.Contains("admincmd", lnk.Arguments);
            }
        }

        [Fact]
        public void LaunchElevated_prompts_and_calls_the_elevation_service()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest
                {
                    Name = "admincmd",
                    Kind = AliasKind.ElevatedRunAs,
                    Target = @"C:\Windows\System32\mmc.exe",
                    Arguments = "compmgmt.msc",
                    RunAsAccount = @"DOM\admin",
                });

                env.Service.LaunchElevated(AliasScope.User, "admincmd", new FakeCredentialPrompt(@"DOM\admin", "pw"));
                Assert.Contains(env.Elevation.Launched, l => l.StartsWith("as:DOM\\admin:", System.StringComparison.Ordinal) && l.Contains("mmc.exe"));
            }
        }

        [Fact]
        public void LaunchElevated_cancel_throws_cancelled()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "a", Kind = AliasKind.ElevatedRunAs, Target = @"C:\x.exe", RunAsAccount = @"DOM\admin" });
                var prompt = new FakeCredentialPrompt(@"DOM\admin", "pw") { Cancelled = true };
                EasyRunException ex = Assert.Throws<EasyRunException>(() => env.Service.LaunchElevated(AliasScope.User, "a", prompt));
                Assert.Equal(EasyRunError.Cancelled, ex.Error);
            }
        }

        [Fact]
        public void List_is_sorted_and_find_searches_both_scopes()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest { Name = "zeta", Target = @"C:\z.exe" });
                env.Service.Create(new CreateAliasRequest { Name = "alpha", Target = @"C:\a.exe" });
                var names = env.Service.List(AliasScope.User).Select(r => r.Name).ToList();
                Assert.Equal(new[] { "alpha", "zeta" }, names);
                Assert.NotNull(env.Service.Find("alpha"));
                Assert.Null(env.Service.Find("missing"));
            }
        }

        [Fact]
        public void Rejects_invalid_name_and_relative_target()
        {
            using (var env = new TestEnv())
            {
                Assert.Throws<EasyRunException>(() => env.Service.Create(new CreateAliasRequest { Name = "bad name", Target = @"C:\a.exe" }));
                Assert.Throws<EasyRunException>(() => env.Service.Create(new CreateAliasRequest { Name = "ok", Target = "relative.exe" }));
            }
        }
    }

    public class AliasRecordMapperTests
    {
        [Fact]
        public void Round_trips_through_app_paths_values()
        {
            using (var env = new TestEnv())
            {
                env.Service.Create(new CreateAliasRequest
                {
                    Name = "serve",
                    Kind = AliasKind.Command,
                    Target = @"C:\tools\python.exe",
                    Arguments = "-m http.server",
                    WorkingDirectory = @"C:\www",
                    Description = "dev server",
                });

                AliasRecord r = env.Service.Get(AliasScope.User, "serve");
                Assert.Equal(AliasKind.Command, r.Kind);
                Assert.Equal(@"C:\tools\python.exe", r.Target);
                Assert.Equal("-m http.server", r.Arguments);
                Assert.Equal(@"C:\www", r.WorkingDirectory);
                Assert.Equal("dev server", r.Description);
            }
        }

        [Fact]
        public void Foreign_alias_without_metadata_is_flagged_unmanaged()
        {
            using (var env = new TestEnv())
            {
                env.Store.Write(AliasScope.Machine, "notepad.exe",
                    new System.Collections.Generic.Dictionary<string, string> { [""] = @"C:\Windows\notepad.exe" });
                AliasRecord r = env.Service.Get(AliasScope.Machine, "notepad");
                Assert.False(r.ManagedByEasyRun);
                Assert.Equal(AliasKind.Executable, r.Kind);
                Assert.Equal(@"C:\Windows\notepad.exe", r.Target);
            }
        }
    }
}

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using EasyRun.Core;
using Microsoft.Win32.TaskScheduler;

namespace EasyRun.App
{
    /// <summary>
    /// The Windows elevation implementation: registers stored-password scheduled tasks (for elevated-task
    /// aliases) and launches programs via secondary logon (for run-as aliases).
    /// </summary>
    internal sealed class WindowsElevationService : IElevationService
    {
        // File-style rights: read + execute (lets the interactive user query and run the task).
        private const int TaskReadExecute = 0x1200A9;

        public string CurrentUserName => Environment.UserDomainName + "\\" + Environment.UserName;

        public bool IsCurrentProcessElevated
        {
            get
            {
                using (WindowsIdentity id = WindowsIdentity.GetCurrent())
                {
                    return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
        }

        public void RegisterElevatedTask(ElevatedTaskSpec spec, AdminCredential credential)
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));
            try
            {
                using (var ts = new TaskService())
                {
                    TaskDefinition td = ts.NewTask();
                    td.RegistrationInfo.Author = "EasyRun";
                    td.RegistrationInfo.Description = spec.Description ?? ("EasyRun elevated task: " + spec.TaskName);
                    td.Principal.RunLevel = TaskRunLevel.Highest;
                    td.Principal.LogonType = TaskLogonType.Password;
                    td.Principal.UserId = credential.UserName;
                    td.Settings.AllowDemandStart = true;
                    td.Settings.Enabled = true;
                    td.Settings.DisallowStartIfOnBatteries = false;
                    td.Settings.StopIfGoingOnBatteries = false;
                    td.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;
                    td.Settings.ExecutionTimeLimit = TimeSpan.Zero; // no time limit
                    td.Actions.Add(new ExecAction(
                        spec.Target,
                        string.IsNullOrEmpty(spec.Arguments) ? null : spec.Arguments,
                        string.IsNullOrEmpty(spec.WorkingDirectory) ? null : spec.WorkingDirectory));

                    Task task = ts.RootFolder.RegisterTaskDefinition(
                        spec.TaskName, td, TaskCreation.CreateOrUpdate,
                        credential.UserName, credential.Password, TaskLogonType.Password);

                    GrantInteractiveUserRun(task);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new EasyRunException(EasyRunError.NotPermitted,
                    "Creating an elevated task requires administrator rights. Start EasyRun as the admin account, or use a run-as alias instead.", ex);
            }
            catch (Exception ex) when (!(ex is EasyRunException))
            {
                throw new EasyRunException(EasyRunError.Failed, "Could not register the task: " + ex.Message, ex);
            }
        }

        public bool TaskExists(string taskName)
        {
            using (var ts = new TaskService())
            {
                return ts.GetTask(taskName) != null;
            }
        }

        public System.Collections.Generic.IReadOnlyList<ElevatedTaskInfo> ListManagedTasks()
        {
            var result = new System.Collections.Generic.List<ElevatedTaskInfo>();
            try
            {
                using (var ts = new TaskService())
                {
                    foreach (Task task in ts.RootFolder.Tasks)
                    {
                        if (task.Name == null || !task.Name.StartsWith(AliasService.TaskPrefix, StringComparison.Ordinal)) continue;
                        string action = null;
                        try
                        {
                            if (task.Definition.Actions.Count > 0 && task.Definition.Actions[0] is ExecAction exec)
                            {
                                action = exec.Path + (string.IsNullOrEmpty(exec.Arguments) ? string.Empty : " " + exec.Arguments);
                            }
                        }
                        catch (Exception)
                        {
                            // Can't read this task's action (access/permission); leave it blank.
                        }
                        result.Add(new ElevatedTaskInfo
                        {
                            Name = task.Name,
                            UserId = SafeUserId(task),
                            State = task.State.ToString(),
                            Action = action,
                        });
                    }
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException)
            {
                // No access to enumerate; return what we have.
            }
            return result;
        }

        private static string SafeUserId(Task task)
        {
            try
            {
                return task.Definition.Principal.UserId;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void RemoveTask(string taskName)
        {
            try
            {
                using (var ts = new TaskService())
                {
                    ts.RootFolder.DeleteTask(taskName, exceptionOnNotExists: false);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new EasyRunException(EasyRunError.NotPermitted, "Removing the task requires administrator rights.", ex);
            }
        }

        public void RunTask(string taskName)
        {
            using (var ts = new TaskService())
            {
                Task task = ts.GetTask(taskName)
                    ?? throw new EasyRunException(EasyRunError.NotFound, "Task not found: " + taskName);
                task.Run();
            }
        }

        public void LaunchAs(string target, string arguments, string workingDirectory, AdminCredential credential)
        {
            SplitAccount(credential.UserName, out string domain, out string user);
            string commandLine = BuildCommandLine(target, arguments);
            string workDir = string.IsNullOrEmpty(workingDirectory) ? Path.GetDirectoryName(target) : workingDirectory;

            var si = new Win32.STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = @"winsta0\default"; // the interactive desktop, so the window is visible
            si.wShowWindow = 1; // SW_SHOWNORMAL
            si.dwFlags = 0x00000001; // STARTF_USESHOWWINDOW

            bool ok = Win32.CreateProcessWithLogonW(
                user, domain, credential.Password, Win32.LOGON_WITH_PROFILE,
                null, commandLine, Win32.CREATE_UNICODE_ENVIRONMENT, IntPtr.Zero, workDir,
                ref si, out Win32.PROCESS_INFORMATION pi);

            if (!ok)
            {
                int err = Marshal.GetLastWin32Error();
                throw new EasyRunException(EasyRunError.Failed,
                    "Could not start '" + target + "' as " + credential.UserName + ": " + new Win32Exception(err).Message);
            }
            Win32.CloseHandle(pi.hThread);
            Win32.CloseHandle(pi.hProcess);
        }

        /// <summary>Grants the interactive user read+run on the task so they can trigger it without admin.</summary>
        private static void GrantInteractiveUserRun(Task task)
        {
            try
            {
                string sddl = task.GetSecurityDescriptorSddlForm(System.Security.AccessControl.SecurityInfos.DiscretionaryAcl);
                string userSid;
                using (WindowsIdentity id = WindowsIdentity.GetCurrent())
                {
                    userSid = id.User.Value;
                }
                string ace = "(A;;" + "0x" + TaskReadExecute.ToString("X", System.Globalization.CultureInfo.InvariantCulture) + ";;;" + userSid + ")";
                if (sddl.IndexOf(userSid, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    // Insert our ACE after the DACL header "D:...(" preserving existing entries.
                    int dacl = sddl.IndexOf("D:", StringComparison.Ordinal);
                    if (dacl >= 0)
                    {
                        int firstAce = sddl.IndexOf('(', dacl);
                        sddl = firstAce >= 0 ? sddl.Insert(firstAce, ace) : sddl + ace;
                        task.SetSecurityDescriptorSddlForm(sddl);
                    }
                }
            }
            catch (Exception)
            {
                // Non-fatal: the task still works for an admin; document the manual grant if needed.
            }
        }

        private static void SplitAccount(string account, out string domain, out string user)
        {
            int slash = account.IndexOf('\\');
            if (slash > 0)
            {
                domain = account.Substring(0, slash);
                user = account.Substring(slash + 1);
            }
            else
            {
                domain = ".";
                user = account;
            }
        }

        private static string BuildCommandLine(string target, string arguments)
        {
            string quoted = "\"" + target + "\"";
            return string.IsNullOrEmpty(arguments) ? quoted : quoted + " " + arguments;
        }
    }
}

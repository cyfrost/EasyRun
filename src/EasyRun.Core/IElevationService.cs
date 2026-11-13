namespace EasyRun.Core
{
    /// <summary>Details for an elevated task that runs the target as an admin account.</summary>
    public sealed class ElevatedTaskSpec
    {
        public string TaskName { get; set; }
        public string Target { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Creates and runs the Task Scheduler tasks behind elevated aliases, and launches programs via
    /// secondary logon. The Windows implementation needs admin rights to register a task; the interface
    /// is faked in tests.
    /// </summary>
    public interface IElevationService
    {
        /// <summary>
        /// Registers a task that runs <paramref name="spec"/> as <paramref name="credential"/> with the
        /// highest privileges and a stored password, granting the current interactive user permission to run it.
        /// </summary>
        void RegisterElevatedTask(ElevatedTaskSpec spec, AdminCredential credential);

        bool TaskExists(string taskName);

        void RemoveTask(string taskName);

        /// <summary>Triggers an already-registered task (no prompt).</summary>
        void RunTask(string taskName);

        /// <summary>
        /// Starts a program as <paramref name="credential"/> in the current interactive session (visible),
        /// via secondary logon. Used by run-as aliases for GUI programs.
        /// </summary>
        void LaunchAs(string target, string arguments, string workingDirectory, AdminCredential credential);

        /// <summary>The account the current interactive user would grant run rights to (its SID as SDDL), for display.</summary>
        string CurrentUserName { get; }

        bool IsCurrentProcessElevated { get; }
    }
}

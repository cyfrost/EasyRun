namespace EasyRun.Core
{
    /// <summary>A snapshot of one EasyRun-created scheduled task, for the managed-tasks view.</summary>
    public sealed class ElevatedTaskInfo
    {
        public string Name { get; set; }

        /// <summary>The account the task runs as (e.g. DOMAIN\\admin).</summary>
        public string UserId { get; set; }

        /// <summary>Ready / Running / Disabled / Unknown.</summary>
        public string State { get; set; }

        /// <summary>The program the task launches.</summary>
        public string Action { get; set; }
    }
}

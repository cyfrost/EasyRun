namespace EasyRun.Core
{
    /// <summary>A request to create or update an alias.</summary>
    public sealed class CreateAliasRequest
    {
        public string Name { get; set; }
        public AliasScope Scope { get; set; } = AliasScope.User;
        public AliasKind Kind { get; set; } = AliasKind.Executable;

        /// <summary>The program, folder or command target the user chose.</summary>
        public string Target { get; set; }
        public string Arguments { get; set; }
        public string WorkingDirectory { get; set; }
        public string Description { get; set; }

        /// <summary>For elevated aliases: the admin account to run as (e.g. DOMAIN\\admin).</summary>
        public string RunAsAccount { get; set; }

        /// <summary>Allow replacing an existing alias of the same name and scope.</summary>
        public bool Overwrite { get; set; }
    }

    /// <summary>The outcome of a change.</summary>
    public sealed class AliasChangeResult
    {
        public AliasChangeResult(bool changed, string message, AliasRecord record = null)
        {
            Changed = changed;
            Message = message;
            Record = record;
        }

        public bool Changed { get; }
        public string Message { get; }
        public AliasRecord Record { get; }
    }
}

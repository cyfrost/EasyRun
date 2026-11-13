using System;

namespace EasyRun.Core
{
    /// <summary>Categories of failure, mapped to CLI exit codes and UI messages.</summary>
    public enum EasyRunError
    {
        None = 0,
        Failed = 1,
        InvalidArgument = 2,
        NotFound = 3,
        Cancelled = 4,
        NotPermitted = 5,
        Conflict = 6,
    }

    /// <summary>An expected, user-explainable failure.</summary>
    [Serializable]
    public sealed class EasyRunException : Exception
    {
        public EasyRunException(EasyRunError error, string message) : base(message)
        {
            Error = error;
        }

        public EasyRunException(EasyRunError error, string message, Exception inner) : base(message, inner)
        {
            Error = error;
        }

        public EasyRunError Error { get; }
    }
}

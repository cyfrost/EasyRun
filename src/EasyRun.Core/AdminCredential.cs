using System;

namespace EasyRun.Core
{
    /// <summary>
    /// A transient admin credential used to register a task or start a secondary-logon process.
    /// EasyRun never persists the password: Task Scheduler stores it in LSA, and the run-as path prompts
    /// for it each session. Callers should <see cref="Dispose"/> it as soon as the call returns.
    /// </summary>
    public sealed class AdminCredential : IDisposable
    {
        private char[] _password;

        public AdminCredential(string userName, char[] password)
        {
            UserName = userName;
            _password = password ?? Array.Empty<char>();
        }

        /// <summary>Account the elevated alias runs as, e.g. DOMAIN\\admin or .\\admin.</summary>
        public string UserName { get; }

        public string Password => new string(_password);

        public void Dispose()
        {
            if (_password != null)
            {
                Array.Clear(_password, 0, _password.Length);
                _password = null;
            }
        }
    }

    /// <summary>Asks the user for admin credentials (or supplies them, in tests).</summary>
    public interface ICredentialPrompt
    {
        /// <summary>Returns the credential, or null if the user cancels.</summary>
        AdminCredential Prompt(string message, string defaultUserName);
    }
}

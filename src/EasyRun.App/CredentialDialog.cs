using System;
using System.Windows.Forms;
using EasyRun.Core;

namespace EasyRun.App
{
    /// <summary>Prompts for an admin account + password. Caches only the username (never the password).</summary>
    internal sealed class CredentialDialog : Form, ICredentialPrompt
    {
        private static string _lastUserName;

        private readonly TextBox _user;
        private readonly TextBox _password;
        private readonly Button _ok;

        public CredentialDialog()
        {
            SuspendLayout();
            Ui.InitDialog(this, "Administrator credentials");
            Ui.MakeChild(this);

            TableLayoutPanel root = Ui.Column();
            _messageLabel = Ui.Label(string.Empty);
            Ui.AddRow(root, _messageLabel);

            Ui.AddRow(root, Ui.Label("&Account (e.g. DOMAIN\\admin or .\\admin):"));
            _user = Ui.InputBox(string.Empty);
            Ui.AddRow(root, _user);
            Ui.AddRow(root, Ui.Label("&Password:"));
            _password = Ui.InputBox(string.Empty);
            _password.UseSystemPasswordChar = true;
            Ui.AddRow(root, _password);
            Ui.AddRow(root, Ui.Hint("EasyRun does not store the password. A task keeps it in Windows (LSA); run-as asks each time."));

            _ok = Ui.Button("OK", OnOk);
            _ok.DialogResult = DialogResult.None;
            Ui.AddRow(root, Ui.ButtonRow(_ok, Ui.CancelButton(this)));

            Controls.Add(root);
            AcceptButton = _ok;
            ResumeLayout(false);
            PerformLayout();
        }

        private readonly Label _messageLabel;
        private AdminCredential _result;

        public AdminCredential Prompt(string message, string defaultUserName)
        {
            _result = null;
            _messageLabel.Text = message ?? "Enter administrator credentials.";
            _user.Text = defaultUserName ?? _lastUserName ?? string.Empty;
            BeginInvokeFocus();
            return ShowDialog() == DialogResult.OK ? _result : null;
        }

        private void BeginInvokeFocus()
        {
            Shown += (s, e) =>
            {
                if (_user.TextLength > 0) _password.Focus();
                else _user.Focus();
            };
        }

        private void OnOk(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_user.Text))
            {
                Ui.Warn(this, "Enter the admin account name.");
                return;
            }
            if (_password.TextLength == 0)
            {
                Ui.Warn(this, "Enter the password.");
                return;
            }
            _lastUserName = _user.Text.Trim();
            _result = new AdminCredential(_lastUserName, _password.Text.ToCharArray());
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

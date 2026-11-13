using System;
using System.Windows.Forms;
using EasyRun.Core;

namespace EasyRun.App
{
    /// <summary>Create an elevated shortcut that runs a program as a stored admin account.</summary>
    internal sealed class AddElevatedDialog : Form
    {
        private readonly AliasService _service;
        private readonly TextBox _target;
        private readonly TextBox _args;
        private readonly TextBox _name;
        private readonly TextBox _account;
        private readonly RadioButton _auto;
        private readonly RadioButton _task;
        private readonly RadioButton _runas;
        private readonly Label _note;
        private readonly Button _ok;
        private readonly Button _test;

        public AddElevatedDialog(AliasService service)
        {
            _service = service;

            SuspendLayout();
            Ui.InitDialog(this, "New elevated shortcut");
            Ui.MakeChild(this);

            TableLayoutPanel root = Ui.Column();
            Ui.AddRow(root, Ui.Heading("Run a program as an admin account, launched by a Run-box alias:"));

            Ui.AddRow(root, Ui.Label("&Program:"));
            var line = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0) };
            _target = Ui.InputBox(string.Empty, Ui.ContentWidth - Ui.ButtonMinWidth - 6);
            _target.Margin = new Padding(3, 3, 0, 8);
            line.Controls.Add(_target);
            line.Controls.Add(Ui.Button("&Browse…", OnBrowse));
            Ui.AddRow(root, line);

            Ui.AddRow(root, Ui.Label("A&rguments (optional):"));
            _args = Ui.InputBox(string.Empty);
            Ui.AddRow(root, _args);

            Ui.AddRow(root, Ui.Label("Admin acco&unt to run as (e.g. DOMAIN\\admin):"));
            _account = Ui.InputBox(string.Empty);
            Ui.AddRow(root, _account);

            Ui.AddRow(root, Ui.Label("&Alias (what you type in Run):"));
            _name = Ui.InputBox(string.Empty);
            Ui.AddRow(root, _name);

            _auto = new RadioButton { Text = "Automatic (recommended): visible run-as for apps, background task for tools", AutoSize = true, Checked = true };
            _task = new RadioButton { Text = "Scheduled task: password stored by Windows, no prompt later (may be hidden for GUI apps)", AutoSize = true };
            _runas = new RadioButton { Text = "Run as (secondary logon): always visible, asks for the password each time", AutoSize = true };
            Ui.AddRow(root, Ui.Group("How to run it", _auto, _task, _runas));

            _note = Ui.Note(Ui.ContentWidth);
            Ui.AddRow(root, _note);

            _ok = Ui.Button("Create", OnCreate);
            _test = Ui.Button("&Test run", OnTestRun, 110);
            Ui.AddRow(root, Ui.ButtonRow(_ok, _test, Ui.CancelButton(this)));

            Controls.Add(root);
            AcceptButton = _ok;
            ResumeLayout(false);
            PerformLayout();

            _target.TextChanged += (s, e) => UpdateState();
            _account.TextChanged += (s, e) => UpdateState();
            _name.TextChanged += (s, e) => UpdateState();
            _auto.CheckedChanged += (s, e) => UpdateState();
            _task.CheckedChanged += (s, e) => UpdateState();
            _runas.CheckedChanged += (s, e) => UpdateState();
            UpdateState();
        }

        private AliasKind ResolveKind()
        {
            if (_task.Checked) return AliasKind.ElevatedTask;
            if (_runas.Checked) return AliasKind.ElevatedRunAs;
            return ExecutableInfo.IsGuiProgram(_target.Text.Trim().Trim('"'))
                ? AliasKind.ElevatedRunAs
                : AliasKind.ElevatedTask;
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Title = "Choose a program", CheckFileExists = true, Filter = "Programs|*.exe;*.bat;*.cmd;*.ps1;*.msc|All files|*.*" })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    _target.Text = ofd.FileName;
                    if (_name.TextLength == 0)
                    {
                        _name.Text = System.IO.Path.GetFileNameWithoutExtension(ofd.FileName).ToLowerInvariant();
                    }
                }
            }
        }

        private void UpdateState()
        {
            string err = _name.TextLength == 0 ? null : AliasName.Validate(_name.Text);
            if (err == null && _target.TextLength > 0 && !AliasService.LooksLikeFullPath(_target.Text))
            {
                err = "The program must be a full path such as C:\\Tools\\app.exe.";
            }
            bool taskMode = ResolveKind() == AliasKind.ElevatedTask;
            Ui.SetNote(_note, err ?? (taskMode
                ? "You'll be asked for the admin password once; Windows stores it for this task."
                : "You'll be asked for the admin password each time this alias runs."), err == null ? Ui.Tone.Info : Ui.Tone.Error);
            bool ready = _name.TextLength > 0 && _target.TextLength > 0 && _account.TextLength > 0 && err == null;
            _ok.Enabled = ready;
            _test.Enabled = _target.TextLength > 0 && _account.TextLength > 0 && (err == null || _name.TextLength == 0);
            Ui.SetShield(_ok, taskMode); // registering a task needs admin
        }

        private void OnTestRun(object sender, EventArgs e)
        {
            string account = _account.Text.Trim();
            if (!AliasService.LooksLikeFullPath(_target.Text))
            {
                Ui.Warn(this, "Enter the program's full path before testing.");
                return;
            }
            using (var prompt = new CredentialDialog())
            using (AdminCredential cred = prompt.Prompt("Enter the password for " + account + " to test-run this program now.", account))
            {
                if (cred == null) return;
                if (Ui.Guard(this, () => _service.TestLaunchAs(_target.Text.Trim().Trim('"'), _args.Text, null, cred)))
                {
                    Ui.Info(this, "Started '" + _target.Text + "' as " + account + ".\n\nIf it launched correctly, the credentials and path are valid.");
                }
            }
        }

        private void OnCreate(object sender, EventArgs e)
        {
            AliasKind kind = ResolveKind();
            var request = new CreateAliasRequest
            {
                Name = _name.Text,
                Scope = AliasScope.User,
                Kind = kind,
                Target = _target.Text,
                Arguments = _args.Text,
                RunAsAccount = _account.Text.Trim(),
                Overwrite = true,
            };

            if (_service.Exists(AliasScope.User, request.Name) &&
                !Ui.Confirm(this, "An alias named '" + AliasName.StripExe(request.Name) + "' already exists. Replace it?"))
            {
                return;
            }
            if (_service.GetShadowedNames().Contains(AliasName.StripExe(request.Name)) &&
                !Ui.Confirm(this, "An all-users alias named '" + AliasName.StripExe(request.Name) + "' also exists. Your new user alias will run instead of it.\n\nContinue?"))
            {
                return;
            }

            AliasChangeResult result = null;
            bool done;
            if (kind == AliasKind.ElevatedTask)
            {
                using (var prompt = new CredentialDialog())
                using (AdminCredential cred = prompt.Prompt(
                    "Enter the password for " + request.RunAsAccount + " to create this elevated task.", request.RunAsAccount))
                {
                    if (cred == null) return;
                    done = Ui.Guard(this, () => result = _service.Create(request, cred));
                }
            }
            else
            {
                done = Ui.Guard(this, () => result = _service.Create(request));
            }

            if (!done) return;
            Ui.Info(this, result.Message + "\n\nRuns as: " + request.RunAsAccount);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

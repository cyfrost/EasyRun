using System;
using System.IO;
using System.Windows.Forms;
using EasyRun.Core;

namespace EasyRun.App
{
    /// <summary>Create a normal alias: a program, a folder, or a program with arguments.</summary>
    internal sealed class AddAliasDialog : Form
    {
        private readonly AliasService _service;
        private readonly RadioButton _program;
        private readonly RadioButton _folder;
        private readonly RadioButton _command;
        private readonly TextBox _name;
        private readonly TextBox _target;
        private readonly Button _browse;
        private readonly Label _argsLabel;
        private readonly TextBox _args;
        private readonly RadioButton _machine;
        private readonly Label _note;
        private readonly Button _ok;

        public AddAliasDialog(AliasService service, string presetTarget = null, bool presetIsFolder = false)
        {
            _service = service;

            SuspendLayout();
            Ui.InitDialog(this, "New alias");
            Ui.MakeChild(this);

            TableLayoutPanel root = Ui.Column();
            Ui.AddRow(root, Ui.Heading("Create a shortcut you can run from the Run box (Win+R):"));

            _program = new RadioButton { Text = "A program", AutoSize = true, Checked = true };
            _folder = new RadioButton { Text = "A folder (opens in Explorer)", AutoSize = true };
            _command = new RadioButton { Text = "A program with arguments", AutoSize = true };
            Ui.AddRow(root, Ui.Group("Alias points to", _program, _folder, _command));

            Ui.AddRow(root, Ui.Label("&Target:"));
            var line = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0) };
            _target = Ui.InputBox(string.Empty, Ui.ContentWidth - Ui.ButtonMinWidth - 6);
            _target.Margin = new Padding(3, 3, 0, 8);
            _browse = Ui.Button("&Browse…", OnBrowse);
            line.Controls.Add(_target);
            line.Controls.Add(_browse);
            Ui.AddRow(root, line);

            _argsLabel = Ui.Label("A&rguments:");
            _args = Ui.InputBox(string.Empty);
            Ui.AddRow(root, _argsLabel);
            Ui.AddRow(root, _args);

            Ui.AddRow(root, Ui.Label("&Alias (what you type in Run):"));
            _name = Ui.InputBox(string.Empty);
            Ui.AddRow(root, _name);

            var user = new RadioButton { Text = Ui.ScopeChoice(AliasScope.User), AutoSize = true, Checked = true };
            _machine = new RadioButton { Text = Ui.ScopeChoice(AliasScope.Machine), AutoSize = true };
            Ui.AddRow(root, Ui.Group("Create it for", user, _machine));

            _note = Ui.Note();
            Ui.AddRow(root, _note);

            _ok = Ui.Button("Create", OnCreate);
            Ui.AddRow(root, Ui.ButtonRow(_ok, Ui.CancelButton(this)));

            Controls.Add(root);
            AcceptButton = _ok;
            ResumeLayout(false);
            PerformLayout();

            _program.CheckedChanged += (s, e) => UpdateMode();
            _folder.CheckedChanged += (s, e) => UpdateMode();
            _command.CheckedChanged += (s, e) => UpdateMode();
            _machine.CheckedChanged += (s, e) => UpdateState();
            _target.TextChanged += (s, e) => UpdateState();
            _name.TextChanged += (s, e) => UpdateState();

            if (!string.IsNullOrEmpty(presetTarget))
            {
                if (presetIsFolder) _folder.Checked = true;
                _target.Text = presetTarget;
                SuggestName();
            }
            UpdateMode();
        }

        private AliasScope Scope => _machine.Checked ? AliasScope.Machine : AliasScope.User;

        private AliasKind Kind =>
            _folder.Checked ? AliasKind.Folder : _command.Checked ? AliasKind.Command : AliasKind.Executable;

        private void UpdateMode()
        {
            bool cmd = _command.Checked;
            _argsLabel.Visible = cmd;
            _args.Visible = cmd;
            UpdateState();
        }

        private void OnBrowse(object sender, EventArgs e)
        {
            if (_folder.Checked)
            {
                string picked = FolderPicker.Pick(this, "Choose a folder", _target.Text);
                if (picked != null) _target.Text = picked;
            }
            else
            {
                using (var ofd = new OpenFileDialog { Title = "Choose a program", CheckFileExists = true, Filter = "Programs|*.exe;*.bat;*.cmd;*.ps1;*.msc|All files|*.*" })
                {
                    if (ofd.ShowDialog(this) == DialogResult.OK) _target.Text = ofd.FileName;
                }
            }
            SuggestName();
        }

        private void SuggestName()
        {
            if (_name.TextLength > 0 || string.IsNullOrWhiteSpace(_target.Text)) return;
            try
            {
                string leaf = _folder.Checked
                    ? new DirectoryInfo(_target.Text.TrimEnd('\\')).Name
                    : Path.GetFileNameWithoutExtension(_target.Text);
                _name.Text = new string((leaf ?? string.Empty).ToLowerInvariant().ToCharArray());
            }
            catch (ArgumentException)
            {
                // Ignore malformed target.
            }
        }

        private void UpdateState()
        {
            string err = _name.TextLength == 0 ? null : AliasName.Validate(_name.Text);
            Ui.SetNote(_note, err, Ui.Tone.Error);
            _ok.Enabled = _name.TextLength > 0 && _target.TextLength > 0 && err == null;
            Ui.SetShield(_ok, Scope == AliasScope.Machine);
            _browse.Text = _folder.Checked ? "&Browse…" : "&Browse…";
        }

        private void OnCreate(object sender, EventArgs e)
        {
            var request = new CreateAliasRequest
            {
                Name = _name.Text,
                Scope = Scope,
                Kind = Kind,
                Target = _target.Text,
                Arguments = _command.Checked ? _args.Text : null,
            };
            if (_service.Exists(Scope, request.Name) &&
                !Ui.Confirm(this, "An alias named '" + AliasName.StripExe(request.Name) + "' already exists for " +
                                  Scope.DisplayName() + ". Replace it?"))
            {
                return;
            }
            request.Overwrite = true;

            AliasChangeResult result = null;
            if (!Ui.Guard(this, () => result = _service.Create(request))) return;
            Ui.Info(this, result.Message);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

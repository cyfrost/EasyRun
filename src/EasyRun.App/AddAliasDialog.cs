using System;
using System.IO;
using System.Windows.Forms;
using EasyRun.Core;

namespace EasyRun.App
{
    /// <summary>Create or edit a normal alias: a program, a folder, or a program with arguments.</summary>
    internal sealed class AddAliasDialog : Form
    {
        private readonly AliasService _service;
        private readonly AliasRecord _editing;
        private readonly RadioButton _program;
        private readonly RadioButton _folder;
        private readonly RadioButton _command;
        private readonly TextBox _name;
        private readonly TextBox _target;
        private readonly Label _argsLabel;
        private readonly TextBox _args;
        private readonly RadioButton _user;
        private readonly RadioButton _machine;
        private readonly Label _note;
        private readonly Button _ok;

        public AddAliasDialog(AliasService service, string presetTarget = null, bool presetIsFolder = false)
            : this(service, null, presetTarget, presetIsFolder)
        {
        }

        public AddAliasDialog(AliasService service, AliasRecord editing, string presetTarget = null, bool presetIsFolder = false)
        {
            _service = service;
            _editing = editing;
            bool edit = editing != null;

            SuspendLayout();
            Ui.InitDialog(this, edit ? "Edit alias" : "New alias");
            Ui.MakeChild(this);

            TableLayoutPanel root = Ui.Column();
            Ui.AddRow(root, Ui.Heading(edit
                ? "Edit this alias:"
                : "Create a shortcut you can run from the Run box (Win+R):"));

            _program = new RadioButton { Text = "A &program", AutoSize = true, Checked = true };
            _folder = new RadioButton { Text = "A f&older (opens in Explorer)", AutoSize = true };
            _command = new RadioButton { Text = "A program with ar&guments", AutoSize = true };
            Ui.AddRow(root, Ui.Group("Alias points to", _program, _folder, _command));

            Ui.AddRow(root, Ui.Label("&Target:"));
            var line = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0) };
            _target = Ui.InputBox(string.Empty, Ui.ContentWidth - Ui.ButtonMinWidth - 6);
            _target.Margin = new Padding(3, 3, 0, 8);
            line.Controls.Add(_target);
            line.Controls.Add(Ui.Button("&Browse…", OnBrowse));
            Ui.AddRow(root, line);

            _argsLabel = Ui.Label("Ar&guments:");
            _args = Ui.InputBox(string.Empty);
            Ui.AddRow(root, _argsLabel);
            Ui.AddRow(root, _args);

            Ui.AddRow(root, Ui.Label("&Alias (what you type in Run):"));
            _name = Ui.InputBox(string.Empty);
            Ui.AddRow(root, _name);

            _user = new RadioButton { Text = Ui.ScopeChoice(AliasScope.User), AutoSize = true, Checked = true };
            _machine = new RadioButton { Text = Ui.ScopeChoice(AliasScope.Machine), AutoSize = true };
            Ui.AddRow(root, Ui.Group("Create it for", _user, _machine));

            _note = Ui.Note();
            Ui.AddRow(root, _note);

            _ok = Ui.Button(edit ? "Save" : "Create", OnSave);
            Ui.AddRow(root, Ui.ButtonRow(_ok, Ui.CancelButton(this)));

            Controls.Add(root);
            AcceptButton = _ok;
            ResumeLayout(false);
            PerformLayout();

            _program.CheckedChanged += (s, e) => UpdateMode();
            _folder.CheckedChanged += (s, e) => UpdateMode();
            _command.CheckedChanged += (s, e) => UpdateMode();
            _user.CheckedChanged += (s, e) => UpdateState();
            _machine.CheckedChanged += (s, e) => UpdateState();
            _target.TextChanged += (s, e) => UpdateState();
            _name.TextChanged += (s, e) => UpdateState();

            if (edit)
            {
                Prefill(editing);
            }
            else if (!string.IsNullOrEmpty(presetTarget))
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

        private void Prefill(AliasRecord r)
        {
            if (r.Kind == AliasKind.Folder) _folder.Checked = true;
            else if (r.Kind == AliasKind.Command) _command.Checked = true;
            else _program.Checked = true;
            _machine.Checked = r.Scope == AliasScope.Machine;
            _user.Checked = r.Scope != AliasScope.Machine;
            _target.Text = r.Target ?? r.LaunchValue;
            _args.Text = r.Arguments ?? string.Empty;
            _name.Text = r.Name;
        }

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
                _name.Text = (leaf ?? string.Empty).ToLowerInvariant();
            }
            catch (ArgumentException)
            {
                // Ignore a malformed target.
            }
        }

        private void UpdateState()
        {
            string err = _name.TextLength == 0 ? null : AliasName.Validate(_name.Text);
            if (err == null && _target.TextLength > 0 && Missing(_target.Text.Trim().Trim('"')))
            {
                Ui.SetNote(_note, "That " + (_folder.Checked ? "folder" : "target") + " doesn't exist yet — you can still create the alias.", Ui.Tone.Warning);
            }
            else
            {
                Ui.SetNote(_note, err, Ui.Tone.Error);
            }
            _ok.Enabled = _name.TextLength > 0 && _target.TextLength > 0 && err == null;
            Ui.SetShield(_ok, Scope == AliasScope.Machine);
        }

        private bool Missing(string path)
        {
            if (!AliasService.LooksLikeFullPath(path)) return false;
            try
            {
                return _folder.Checked ? !Directory.Exists(path) : !File.Exists(path);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void OnSave(object sender, EventArgs e)
        {
            var request = new CreateAliasRequest
            {
                Name = _name.Text,
                Scope = Scope,
                Kind = Kind,
                Target = _target.Text,
                Arguments = _command.Checked ? _args.Text : null,
            };

            bool renamed = _editing != null &&
                (!string.Equals(AliasName.StripExe(_editing.Name), AliasName.StripExe(request.Name), StringComparison.OrdinalIgnoreCase) || _editing.Scope != Scope);

            if ((renamed || _editing == null) && _service.Exists(Scope, request.Name) &&
                !Ui.Confirm(this, "An alias named '" + AliasName.StripExe(request.Name) + "' already exists for " +
                                  Scope.DisplayName() + ". Replace it?"))
            {
                return;
            }
            request.Overwrite = true;

            AliasChangeResult result = null;
            if (!Ui.Guard(this, () =>
            {
                result = _service.Create(request);
                if (renamed) _service.Remove(_editing.Scope, _editing.Name);
            }))
            {
                return;
            }
            Ui.Info(this, result.Message);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}

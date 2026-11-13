using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EasyRun.Core;

namespace EasyRun.App
{
    /// <summary>Lists every alias (user + machine) and lets you run, open, edit or remove them.</summary>
    internal sealed class AliasManagerForm : Form
    {
        private readonly AliasService _service;
        private readonly TextBox _search;
        private readonly ListView _list;
        private readonly Label _status;
        private readonly Button _run;
        private readonly Button _edit;
        private readonly Button _openFolder;
        private readonly Button _remove;
        private List<AliasRecord> _all = new List<AliasRecord>();

        public AliasManagerForm(AliasService service)
        {
            _service = service;

            SuspendLayout();
            Ui.InitForm(this, "Alias Manager");
            ClientSize = new Size(900, 520);
            MinimumSize = new Size(680, 360);
            KeyPreview = true;

            // Toolbar row: create/manage actions (docked first so it sits at the very top).
            Button add = Ui.Button("&New alias…", (s, e) => OpenAdd());
            add.Margin = new Padding(0, 0, 6, 0);
            Button import = Ui.Button("&Import…", (s, e) => ImportAliases());
            Button export = Ui.Button("E&xport…", (s, e) => ExportAliases());
            Button backups = Ui.Button("&Backups…", (s, e) => OpenBackups());
            var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = false, Margin = new Padding(0), Padding = new Padding(0, 0, 0, 6) };
            toolbar.Controls.AddRange(new Control[] { add, import, export, backups });

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            top.Controls.Add(new Label { Text = "&Filter:", AutoSize = true, Margin = new Padding(0, 8, 6, 0) });
            _search = Ui.InputBox(string.Empty, 240);
            _search.Margin = new Padding(0, 4, 0, 8);
            top.Controls.Add(_search);

            _list = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false,
                Dock = DockStyle.Fill,
                ShowItemToolTips = true,
            };
            _list.Columns.Add("Alias", 150);
            _list.Columns.Add("Type", 120);
            _list.Columns.Add("Target", 370);
            _list.Columns.Add("Runs as", 150);
            _list.Columns.Add("Scope", 80);
            _list.ContextMenuStrip = BuildContextMenu();

            _run = Ui.Button("&Run", (s, e) => RunSelected());
            _edit = Ui.Button("&Edit…", (s, e) => EditSelected());
            _openFolder = Ui.Button("&Open folder", (s, e) => OpenFolder());
            _remove = Ui.Button("&Delete", (s, e) => RemoveSelected());
            Button refresh = Ui.Button("Re&fresh", (s, e) => Reload());
            Button close = Ui.Button("Close");
            close.DialogResult = DialogResult.Cancel;

            _status = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 0), ForeColor = SystemColors.GrayText };
            var left = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            left.Controls.Add(_status);
            FlowLayoutPanel right = Ui.ButtonRow(_run, _edit, _openFolder, _remove, refresh, close);
            right.Margin = new Padding(0);

            var bottom = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, AutoSize = true, Dock = DockStyle.Bottom, Margin = new Padding(0, 8, 0, 0) };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottom.Controls.Add(left, 0, 0);
            bottom.Controls.Add(right, 1, 0);

            // Dock order: Fill first, then Top-docked panels bottom-up so the toolbar ends up above the filter.
            Controls.Add(_list);
            Controls.Add(bottom);
            Controls.Add(top);
            Controls.Add(toolbar);
            CancelButton = close;
            ResumeLayout(false);
            PerformLayout();

            _search.TextChanged += (s, e) => ApplyFilter();
            _list.SelectedIndexChanged += (s, e) => UpdateButtons();
            _list.DoubleClick += (s, e) => RunSelected();
            _list.KeyDown += OnListKeyDown;
            Load += (s, e) => Reload();
        }

        private AliasRecord Selected => _list.SelectedItems.Count == 1 ? (AliasRecord)_list.SelectedItems[0].Tag : null;

        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Run", null, (s, e) => RunSelected());
            menu.Items.Add("Edit…", null, (s, e) => EditSelected());
            menu.Items.Add("Open containing folder", null, (s, e) => OpenFolder());
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Copy alias name", null, (s, e) => CopyText(Selected?.Name));
            menu.Items.Add("Copy target path", null, (s, e) => CopyText(Selected?.Target ?? Selected?.LaunchValue));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Delete", null, (s, e) => RemoveSelected());
            menu.Opening += (s, e) => e.Cancel = Selected == null;
            return menu;
        }

        private void OnListKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) { RemoveSelected(); e.Handled = true; }
            else if (e.KeyCode == Keys.Enter) { RunSelected(); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { Reload(); e.Handled = true; }
            else if (e.KeyCode == Keys.F2) { EditSelected(); e.Handled = true; }
        }

        private void Reload()
        {
            _all = _service.ListAll().ToList();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string q = _search.Text.Trim();
            IEnumerable<AliasRecord> shown = string.IsNullOrEmpty(q)
                ? _all
                : _all.Where(r =>
                    (r.Name?.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (r.Target?.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0));

            System.Collections.Generic.ISet<string> shadowed = _service.GetShadowedNames();

            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (AliasRecord r in shown)
            {
                string target = r.Target ?? r.LaunchValue;
                bool isShadow = shadowed.Contains(r.Name);
                bool hidden = isShadow && r.Scope == AliasScope.Machine; // the user alias wins
                var item = new ListViewItem(r.Name) { Tag = r, ToolTipText = DescribeTooltip(r, isShadow, hidden) };
                item.SubItems.Add(r.Kind.DisplayName());
                item.SubItems.Add(target);
                item.SubItems.Add(r.Kind.IsElevated() ? (r.RunAsAccount ?? "admin") : string.Empty);
                ListViewItem.ListViewSubItem scope = item.SubItems.Add(
                    (r.Scope == AliasScope.Machine ? "All users" : "Just me") + (hidden ? " (hidden)" : isShadow ? " (overrides)" : string.Empty));
                if (hidden) scope.ForeColor = Ui.ToneColor(Ui.Tone.Warning);
                if (!r.ManagedByEasyRun) item.ForeColor = SystemColors.GrayText;
                _list.Items.Add(item);
            }
            _list.EndUpdate();
            _status.Text = _list.Items.Count + " of " + _all.Count + " aliases";
            UpdateButtons();
        }

        private static string DescribeTooltip(AliasRecord r, bool isShadow, bool hidden)
        {
            string tip = "Type \"" + r.Name + "\" in the Run box (Win+R).\n" + (r.Target ?? r.LaunchValue);
            if (!string.IsNullOrEmpty(r.Arguments)) tip += " " + r.Arguments;
            if (r.Kind.IsElevated()) tip += "\nRuns as " + r.RunAsAccount;
            if (hidden) tip += "\nHidden: your user alias of the same name runs instead.";
            else if (isShadow) tip += "\nThis user alias overrides the all-users alias of the same name.";
            if (!r.ManagedByEasyRun) tip += "\n(Not created by EasyRun.)";
            return tip;
        }

        private void UpdateButtons()
        {
            AliasRecord r = Selected;
            _run.Enabled = r != null;
            _edit.Enabled = r != null;
            _openFolder.Enabled = r != null;
            _remove.Enabled = r != null;
            Ui.SetShield(_remove, r != null && (r.Scope == AliasScope.Machine || r.Kind == AliasKind.ElevatedTask));
        }

        private void OpenAdd()
        {
            using (var dialog = new AddAliasDialog(_service))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) Reload();
            }
        }

        private void EditSelected()
        {
            AliasRecord r = Selected;
            if (r == null) return;
            if (r.Kind.IsElevated())
            {
                Ui.Info(this, "Elevated shortcuts can't be edited in place.\n\nDelete this one and create a new elevated shortcut instead.");
                return;
            }
            if (!r.ManagedByEasyRun &&
                !Ui.Confirm(this, "'" + r.Name + "' wasn't created by EasyRun. Edit it anyway? EasyRun will take it over."))
            {
                return;
            }
            using (var dialog = new AddAliasDialog(_service, r))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK) Reload();
            }
        }

        private void RunSelected()
        {
            AliasRecord r = Selected;
            if (r == null) return;
            Ui.Guard(this, () =>
            {
                var psi = new ProcessStartInfo(r.LaunchValue) { UseShellExecute = true };
                if (!string.IsNullOrEmpty(r.WorkingDirectory)) psi.WorkingDirectory = r.WorkingDirectory;
                using (Process.Start(psi))
                {
                }
            });
        }

        private void OpenFolder()
        {
            AliasRecord r = Selected;
            if (r == null) return;
            try
            {
                string path = r.Target ?? r.LaunchValue;
                string folder = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder)) Ui.OpenFolder(folder);
                else Ui.Warn(this, "The target folder doesn't exist:\n\n" + folder);
            }
            catch (ArgumentException ex)
            {
                Ui.Error(this, ex.Message);
            }
        }

        private void CopyText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            try
            {
                Clipboard.SetText(text);
            }
            catch (Exception ex)
            {
                Ui.Error(this, "Couldn't copy to the clipboard: " + ex.Message);
            }
        }

        private void OpenBackups()
        {
            using (var form = new BackupsForm(_service))
            {
                form.ShowDialog(this);
                if (form.RestoredSomething) Reload();
            }
        }

        private void ExportAliases()
        {
            System.Collections.Generic.IReadOnlyList<AliasExportItem> items = _service.GetExportItems();
            if (items.Count == 0)
            {
                Ui.Info(this, "There are no EasyRun aliases to export yet.");
                return;
            }
            using (var sfd = new SaveFileDialog { Title = "Export aliases", Filter = "EasyRun aliases (*.json)|*.json", FileName = "easyrun-aliases.json", DefaultExt = "json" })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;
                if (Ui.Guard(this, () => AliasExport.Save(sfd.FileName, items)))
                {
                    Ui.Info(this, "Exported " + items.Count + (items.Count == 1 ? " alias." : " aliases."));
                }
            }
        }

        private void ImportAliases()
        {
            using (var ofd = new OpenFileDialog { Title = "Import aliases", Filter = "EasyRun aliases (*.json)|*.json|All files|*.*", CheckFileExists = true })
            {
                if (ofd.ShowDialog(this) != DialogResult.OK) return;
                bool overwrite = Ui.Confirm(this, "Replace aliases that already have the same name?\n\nYes = overwrite, No = keep existing and skip duplicates.");
                ImportSummary summary = null;
                if (!Ui.Guard(this, () => summary = _service.Import(AliasExport.Load(ofd.FileName), overwrite))) return;
                Reload();
                Ui.Info(this, summary.Describe());
            }
        }

        private void RemoveSelected()
        {
            AliasRecord r = Selected;
            if (r == null) return;
            string extra = r.Kind == AliasKind.ElevatedTask ? "\n\nIts scheduled task will also be removed." : string.Empty;
            if (!Ui.Confirm(this, "Delete alias '" + r.Name + "' (" + r.Kind.DisplayName() + ")?" + extra)) return;
            if (Ui.Guard(this, () => _service.Remove(r.Scope, r.Name))) Reload();
        }
    }
}

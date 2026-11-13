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
        private readonly Button _openFolder;
        private readonly Button _remove;
        private readonly Button _add;
        private List<AliasRecord> _all = new List<AliasRecord>();

        public AliasManagerForm(AliasService service)
        {
            _service = service;

            SuspendLayout();
            Ui.InitForm(this, "Alias Manager");
            ClientSize = new Size(900, 520);
            MinimumSize = new Size(640, 360);

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            top.Controls.Add(new Label { Text = "Filter:", AutoSize = true, Margin = new Padding(0, 8, 6, 0) });
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
            _list.Columns.Add("Target", 380);
            _list.Columns.Add("Runs as", 150);
            _list.Columns.Add("Scope", 80);

            _add = Ui.Button("&New alias…", (s, e) => OpenAdd());
            _add.Margin = new Padding(0, 0, 6, 0);
            _run = Ui.Button("&Run", (s, e) => RunSelected());
            _openFolder = Ui.Button("&Open folder", (s, e) => OpenFolder());
            _remove = Ui.Button("&Delete", (s, e) => RemoveSelected());
            Button refresh = Ui.Button("Re&fresh", (s, e) => Reload());
            Button close = Ui.Button("Close");
            close.DialogResult = DialogResult.Cancel;

            _status = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 0), ForeColor = SystemColors.GrayText };
            var left = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            left.Controls.Add(_add);
            left.Controls.Add(_status);
            FlowLayoutPanel right = Ui.ButtonRow(_run, _openFolder, _remove, refresh, close);
            right.Margin = new Padding(0);

            var bottom = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, AutoSize = true, Dock = DockStyle.Bottom, Margin = new Padding(0, 8, 0, 0) };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottom.Controls.Add(left, 0, 0);
            bottom.Controls.Add(right, 1, 0);

            Controls.Add(_list);
            Controls.Add(bottom);
            Controls.Add(top);
            CancelButton = close;
            ResumeLayout(false);
            PerformLayout();

            _search.TextChanged += (s, e) => ApplyFilter();
            _list.SelectedIndexChanged += (s, e) => UpdateButtons();
            _list.DoubleClick += (s, e) => RunSelected();
            Load += (s, e) => Reload();
        }

        private AliasRecord Selected => _list.SelectedItems.Count == 1 ? (AliasRecord)_list.SelectedItems[0].Tag : null;

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

            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (AliasRecord r in shown)
            {
                var item = new ListViewItem(r.Name) { Tag = r };
                item.SubItems.Add(r.Kind.DisplayName());
                item.SubItems.Add(r.Target ?? r.LaunchValue);
                item.SubItems.Add(r.Kind.IsElevated() ? (r.RunAsAccount ?? "admin") : string.Empty);
                item.SubItems.Add(r.Scope == AliasScope.Machine ? "All users" : "Just me");
                if (!r.ManagedByEasyRun) item.ForeColor = SystemColors.GrayText;
                _list.Items.Add(item);
            }
            _list.EndUpdate();
            _status.Text = _list.Items.Count + " of " + _all.Count + " aliases";
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            AliasRecord r = Selected;
            _run.Enabled = r != null;
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

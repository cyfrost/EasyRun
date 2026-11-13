using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using EasyRun.Core;

namespace EasyRun.App
{
    /// <summary>Lists the backups EasyRun takes before overwriting or removing an alias, and restores them.</summary>
    internal sealed class BackupsForm : Form
    {
        private readonly AliasService _service;
        private readonly ListView _list;
        private readonly Button _restore;

        public BackupsForm(AliasService service)
        {
            _service = service;

            SuspendLayout();
            Ui.InitForm(this, "Backups");
            Ui.MakeChild(this);
            ClientSize = new Size(760, 440);
            MinimumSize = new Size(560, 300);
            MinimizeBox = false;

            Label intro = Ui.Label("EasyRun saves an alias before overwriting or deleting it. Restore one here. (Elevated-task aliases must be recreated so you can supply the password.)", 720);
            intro.Dock = DockStyle.Top;

            _list = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                HideSelection = false,
                MultiSelect = false,
                Dock = DockStyle.Fill,
                ShowItemToolTips = true,
            };
            _list.Columns.Add("When", 150);
            _list.Columns.Add("Alias", 130);
            _list.Columns.Add("Type", 110);
            _list.Columns.Add("Target", 280);
            _list.Columns.Add("Scope", 70);
            _list.DoubleClick += (s, e) => RestoreSelected();
            _list.SelectedIndexChanged += (s, e) => UpdateButtons();

            _restore = Ui.Button("&Restore…", (s, e) => RestoreSelected());
            Button openFolder = Ui.Button("Open &folder", (s, e) => OpenFolder());
            Button refresh = Ui.Button("Re&fresh", (s, e) => Reload());
            Button close = Ui.Button("Close");
            close.DialogResult = DialogResult.Cancel;
            FlowLayoutPanel buttons = Ui.ButtonRow(_restore, openFolder, refresh, close);
            buttons.Dock = DockStyle.Bottom;

            Controls.Add(_list);
            Controls.Add(intro);
            Controls.Add(buttons);
            CancelButton = close;
            ResumeLayout(false);
            PerformLayout();

            Load += (s, e) => Reload();
        }

        /// <summary>True when a restore happened, so the caller can refresh its own list.</summary>
        public bool RestoredSomething { get; private set; }

        private AliasBackup Selected => _list.SelectedItems.Count == 1 ? (AliasBackup)_list.SelectedItems[0].Tag : null;

        private void Reload()
        {
            IReadOnlyList<AliasBackup> backups = _service.Backups?.List() ?? new List<AliasBackup>();
            _list.BeginUpdate();
            _list.Items.Clear();
            foreach (AliasBackup b in backups)
            {
                AliasExportItem it = b.Item;
                var item = new ListViewItem(b.TimestampUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)) { Tag = b };
                item.SubItems.Add(it.Name);
                item.SubItems.Add(it.Kind);
                item.SubItems.Add(it.Target + (string.IsNullOrEmpty(it.Arguments) ? string.Empty : " " + it.Arguments));
                item.SubItems.Add(string.Equals(it.Scope, "Machine", StringComparison.OrdinalIgnoreCase) ? "All users" : "Just me");
                _list.Items.Add(item);
            }
            _list.EndUpdate();
            UpdateButtons();
        }

        private void UpdateButtons() => _restore.Enabled = Selected != null;

        private void RestoreSelected()
        {
            AliasBackup b = Selected;
            if (b == null) return;
            if (!Ui.Confirm(this, "Restore alias '" + AliasName.StripExe(b.Item.Name) + "' from " +
                                  b.TimestampUtc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture) + "?\n\nAn existing alias with the same name will be replaced.")) return;
            if (Ui.Guard(this, () => _service.RestoreBackup(b, overwrite: true)))
            {
                RestoredSomething = true;
                Ui.Info(this, "Restored '" + AliasName.StripExe(b.Item.Name) + "'.");
            }
        }

        private void OpenFolder()
        {
            string dir = _service.Backups?.Directory;
            if (string.IsNullOrEmpty(dir) || !System.IO.Directory.Exists(dir))
            {
                Ui.Info(this, "There are no backups yet.");
                return;
            }
            Ui.OpenFolder(dir);
        }
    }
}

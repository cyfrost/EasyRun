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
    /// <summary>Shows the scheduled tasks EasyRun created for elevated aliases, and lets you run or remove them.</summary>
    internal sealed class TasksForm : Form
    {
        private readonly AliasService _service;
        private readonly ListView _list;
        private readonly Label _status;
        private readonly Button _run;
        private readonly Button _remove;

        public TasksForm(AliasService service)
        {
            _service = service;

            SuspendLayout();
            Ui.InitForm(this, "Elevated tasks");
            ClientSize = new Size(820, 440);
            MinimumSize = new Size(620, 320);

            Label intro = Ui.Label("Scheduled tasks EasyRun created for elevated shortcuts. An orphan has no alias pointing at it (safe to remove).", 780);
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
            _list.Columns.Add("Task", 170);
            _list.Columns.Add("Runs as", 150);
            _list.Columns.Add("State", 70);
            _list.Columns.Add("Used by alias", 120);
            _list.Columns.Add("Program", 290);
            _list.SelectedIndexChanged += (s, e) => UpdateButtons();

            _run = Ui.Button("&Run", (s, e) => RunSelected());
            _remove = Ui.Button("&Remove", (s, e) => RemoveSelected());
            Button openScheduler = Ui.Button("Open &Task Scheduler", (s, e) => OpenScheduler(), 150);
            Button refresh = Ui.Button("Re&fresh", (s, e) => Reload());
            Button close = Ui.Button("Close");
            close.DialogResult = DialogResult.Cancel;

            _status = new Label { AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 8, 0, 0), ForeColor = SystemColors.GrayText };
            var left = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            left.Controls.Add(_status);
            FlowLayoutPanel right = Ui.ButtonRow(_run, _remove, openScheduler, refresh, close);
            right.Margin = new Padding(0);
            var bottom = new TableLayoutPanel { ColumnCount = 2, RowCount = 1, AutoSize = true, Dock = DockStyle.Bottom, Margin = new Padding(0, 8, 0, 0) };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            bottom.Controls.Add(left, 0, 0);
            bottom.Controls.Add(right, 1, 0);

            Controls.Add(_list);
            Controls.Add(bottom);
            Controls.Add(intro);
            CancelButton = close;
            ResumeLayout(false);
            PerformLayout();

            Load += (s, e) => Reload();
        }

        private sealed class Row
        {
            public ElevatedTaskInfo Task;
            public string Alias;
            public bool Orphan;
        }

        private Row Selected => _list.SelectedItems.Count == 1 ? (Row)_list.SelectedItems[0].Tag : null;

        private void Reload()
        {
            IReadOnlyList<ElevatedTaskInfo> tasks;
            try
            {
                tasks = _service.Elevation.ListManagedTasks();
            }
            catch (Exception ex)
            {
                Ui.Error(this, "Couldn't read scheduled tasks: " + ex.Message);
                tasks = new List<ElevatedTaskInfo>();
            }

            Dictionary<string, string> byTask = _service.ListAll()
                .Where(r => r.Kind == AliasKind.ElevatedTask && !string.IsNullOrEmpty(r.TaskName))
                .GroupBy(r => r.TaskName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().Name, StringComparer.OrdinalIgnoreCase);

            _list.BeginUpdate();
            _list.Items.Clear();
            int orphans = 0;
            foreach (ElevatedTaskInfo t in tasks)
            {
                byTask.TryGetValue(t.Name, out string alias);
                bool orphan = alias == null;
                if (orphan) orphans++;
                var row = new Row { Task = t, Alias = alias, Orphan = orphan };
                var item = new ListViewItem(t.Name) { Tag = row, ToolTipText = t.Action };
                item.SubItems.Add(t.UserId);
                item.SubItems.Add(t.State);
                item.SubItems.Add(orphan ? "(orphan)" : alias);
                item.SubItems.Add(t.Action);
                if (orphan) item.SubItems[3].ForeColor = Ui.ToneColor(Ui.Tone.Warning);
                _list.Items.Add(item);
            }
            _list.EndUpdate();
            _status.Text = tasks.Count + " task" + (tasks.Count == 1 ? string.Empty : "s") +
                           (orphans > 0 ? ", " + orphans + " orphan" + (orphans == 1 ? string.Empty : "s") : string.Empty);
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            Row r = Selected;
            _run.Enabled = r != null;
            _remove.Enabled = r != null;
            Ui.SetShield(_remove, r != null && !_service.Elevation.IsCurrentProcessElevated);
        }

        private void RunSelected()
        {
            Row r = Selected;
            if (r == null) return;
            Ui.Guard(this, () => _service.Elevation.RunTask(r.Task.Name));
        }

        private void RemoveSelected()
        {
            Row r = Selected;
            if (r == null) return;
            string warn = r.Orphan
                ? "Remove the orphaned task '" + r.Task.Name + "'?"
                : "Task '" + r.Task.Name + "' is still used by alias '" + r.Alias + "'.\n\nRemove the task anyway? The alias will stop working until you recreate it.";
            if (!Ui.Confirm(this, warn)) return;
            if (Ui.Guard(this, () => _service.Elevation.RemoveTask(r.Task.Name))) Reload();
        }

        private void OpenScheduler()
        {
            Ui.Guard(this, () =>
            {
                string mmc = Path.Combine(Environment.SystemDirectory, "taskschd.msc");
                using (Process.Start(new ProcessStartInfo(mmc) { UseShellExecute = true }))
                {
                }
            });
        }
    }
}

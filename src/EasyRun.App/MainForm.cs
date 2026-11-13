using System;
using System.Drawing;
using System.Windows.Forms;
using EasyRun.Core;

namespace EasyRun.App
{
    /// <summary>The EasyRun home window: a small hub to create and manage aliases.</summary>
    internal sealed class MainForm : Form
    {
        private readonly AliasService _service;
        private readonly Button _integration;

        public MainForm(AliasService service)
        {
            _service = service;

            SuspendLayout();
            Ui.InitForm(this, "EasyRun");
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            AutoSize = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;

            TableLayoutPanel root = Ui.Column();
            Ui.AddRow(root, Ui.Heading("EasyRun — run things from the Run box (Win+R)"));
            Ui.AddRow(root, Ui.Label("Create a short alias for a program, folder or command, or an elevated shortcut that runs as an admin account.", 420));

            Ui.AddRow(root, BigButton("New &alias…", "A program, folder, or program with arguments", (s, e) => Open(new AddAliasDialog(_service))));
            Ui.AddRow(root, BigButton("New &elevated shortcut…", "Run a program as an admin account", (s, e) => Open(new AddElevatedDialog(_service))));
            Ui.AddRow(root, BigButton("Alias &Manager…", "See, run, edit, import/export and remove aliases", (s, e) => Open(new AliasManagerForm(_service))));
            Ui.AddRow(root, BigButton("Elevated &tasks…", "Review and clean up the tasks behind elevated shortcuts", (s, e) => Open(new TasksForm(_service))));
            Ui.AddRow(root, BigButton("&Backups…", "Restore an alias you changed or deleted", (s, e) => Open(new BackupsForm(_service))));

            _integration = BigButton("Explorer integration", string.Empty, (s, e) => ToggleIntegration());
            Ui.AddRow(root, _integration);

            var bottom = Ui.ButtonRow(CloseButton(), AboutButton());
            Ui.AddRow(root, bottom);

            Controls.Add(root);
            ResumeLayout(false);
            PerformLayout();

            Load += (s, e) => RefreshIntegration();
        }

        private static Button BigButton(string text, string subtitle, EventHandler onClick)
        {
            string caption = string.IsNullOrEmpty(subtitle) ? text : text + "\n" + subtitle;
            var b = new Button
            {
                Text = caption,
                AutoSize = false,
                Size = new Size(420, 52),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                FlatStyle = FlatStyle.System,
                UseVisualStyleBackColor = true,
                Margin = new Padding(3, 3, 3, 6),
            };
            b.Click += onClick;
            return b;
        }

        private Button CloseButton()
        {
            Button b = Ui.Button("Close");
            b.Click += (s, e) => Close();
            return b;
        }

        private Button AboutButton()
        {
            return Ui.Button("&About", (s, e) => Ui.Info(this,
                "EasyRun " + typeof(MainForm).Assembly.GetName().Version.ToString(3) + "\n\n" +
                "Create Run-box aliases and elevated shortcuts.\n" +
                "Copyright (c) 2018-2026 cyfrost. MIT License."));
        }

        private void Open(Form dialog)
        {
            using (dialog)
            {
                dialog.ShowDialog(this);
            }
        }

        private void ToggleIntegration()
        {
            Ui.Guard(this, () =>
            {
                if (ShellIntegration.IsRegistered()) ShellIntegration.Unregister();
                else ShellIntegration.Register(EasyRunServices.SelfPath());
            });
            RefreshIntegration();
        }

        private void RefreshIntegration()
        {
            bool on = false;
            try
            {
                on = ShellIntegration.IsRegistered();
            }
            catch (Exception)
            {
                // Treat as off.
            }
            _integration.Text = (on ? "Disable" : "Enable") + " Explorer integration" + "\n" +
                                 "Right-click a file or folder to add it as an alias" + (on ? " (on)" : " (off)");
        }
    }
}

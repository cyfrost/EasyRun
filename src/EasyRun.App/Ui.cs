using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using EasyRun.Core;

namespace EasyRun.App
{
    /// <summary>
    /// Shared look and wording for every window, so dialogs can't drift apart:
    /// one title format, one padding, one button size, one set of note colours and one vocabulary.
    /// </summary>
    internal static class Ui
    {
        public const string AppName = "EasyRun";

        public const int WindowPadding = 12;
        public const int ButtonHeight = 28;
        public const int ButtonMinWidth = 88;
        public const int ContentWidth = 480;

        public const string AdminNote = "needs administrator approval";

        private static Icon _icon;

        public enum Tone
        {
            Info,
            Warning,
            Error,
        }

        public static Color ToneColor(Tone tone)
        {
            switch (tone)
            {
                case Tone.Error: return Color.FromArgb(176, 0, 32);
                case Tone.Warning: return Color.FromArgb(150, 85, 0);
                default: return SystemColors.GrayText;
            }
        }

        public static Icon AppIcon
        {
            get
            {
                if (_icon == null)
                {
                    try
                    {
                        _icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                    }
                    catch (ArgumentException)
                    {
                        _icon = SystemIcons.Application;
                    }
                }
                return _icon;
            }
        }

        /// <summary>"Backups" becomes "Backups - Path++".</summary>
        public static string Title(string window) => window + " - " + AppName;

        /// <summary>
        /// The wording for a scope choice, used by every dialog:
        /// "Just me: your user PATH" / "All users: the system PATH (needs administrator approval)".
        /// </summary>
        public static string ScopeChoice(AliasScope scope, string detail = null, bool adminNote = true)
        {
            var notes = new List<string>();
            if (!string.IsNullOrEmpty(detail)) notes.Add(detail);
            if (scope == AliasScope.Machine && adminNote) notes.Add(AdminNote);
            string text = scope == AliasScope.User ? "Just me (current user)" : "All users";
            return notes.Count == 0 ? text : text + " (" + string.Join(", ", notes) + ")";
        }

        /// <summary>Standard setup for a resizable window. Call between SuspendLayout/ResumeLayout.</summary>
        public static void InitForm(Form form, string window)
        {
            form.AutoScaleDimensions = new SizeF(96F, 96F);
            form.AutoScaleMode = AutoScaleMode.Dpi;
            form.Font = SystemFonts.MessageBoxFont;
            form.Text = Title(window);
            form.Icon = AppIcon;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.ShowInTaskbar = true;
            form.Padding = new Padding(WindowPadding);
        }

        /// <summary>Fixed-size dialog that sizes itself to its content.</summary>
        public static void InitDialog(Form form, string window)
        {
            InitForm(form, window);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.AutoSize = true;
            form.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        /// <summary>A dialog owned by another window (not shown in the taskbar, centred on its owner).</summary>
        public static void MakeChild(Form form)
        {
            form.ShowInTaskbar = false;
            form.StartPosition = FormStartPosition.CenterParent;
        }

        public static TableLayoutPanel Column()
        {
            var panel = new TableLayoutPanel
            {
                ColumnCount = 1,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            return panel;
        }

        public static void AddRow(TableLayoutPanel panel, Control control)
        {
            panel.RowCount++;
            panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            panel.Controls.Add(control, 0, panel.RowCount - 1);
        }

        /// <summary>Bold first line of a dialog.</summary>
        public static Label Heading(string text, int wrapWidth = ContentWidth)
        {
            Label label = Label(text, wrapWidth);
            label.Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold);
            label.Margin = new Padding(3, 0, 3, 8);
            return label;
        }

        public static Label Label(string text, int wrapWidth = ContentWidth)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                MaximumSize = new Size(wrapWidth, 0),
                Margin = new Padding(3, 3, 3, 6),
            };
        }

        /// <summary>A coloured note under the inputs; hidden while empty.</summary>
        public static Label Note(int wrapWidth = ContentWidth)
        {
            Label label = Label(string.Empty, wrapWidth);
            label.Visible = false;
            return label;
        }

        public static void SetNote(Label note, string text, Tone tone)
        {
            note.Text = text ?? string.Empty;
            note.ForeColor = ToneColor(tone);
            note.Visible = !string.IsNullOrEmpty(text);
        }

        /// <summary>Small grey text at the bottom of a dialog (e.g. the backup reminder).</summary>
        public static Label Hint(string text, int wrapWidth = ContentWidth)
        {
            Label label = Label(text, wrapWidth);
            label.ForeColor = SystemColors.GrayText;
            label.Margin = new Padding(3, 8, 3, 0);
            return label;
        }

        public static TextBox ReadOnlyBox(string text, int width = ContentWidth)
        {
            return new TextBox
            {
                Text = text,
                ReadOnly = true,
                TabStop = false, // focus starts on the first choice, not on selected read-only text
                Width = width,
                Margin = new Padding(3, 0, 3, 8),
            };
        }

        public static TextBox InputBox(string text, int width = ContentWidth)
        {
            return new TextBox { Text = text ?? string.Empty, Width = width, Margin = new Padding(3, 0, 3, 8) };
        }

        public static GroupBox Group(string text, params Control[] options)
        {
            var group = new GroupBox
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 4, 8, 4),
                Margin = new Padding(3, 4, 3, 4),
            };
            TableLayoutPanel column = Column();
            foreach (Control option in options)
            {
                option.Margin = new Padding(3, 3, 3, 3);
                AddRow(column, option);
            }
            group.Controls.Add(column);
            return group;
        }

        public static Button Button(string text, EventHandler onClick = null, int minWidth = ButtonMinWidth)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                MinimumSize = new Size(minWidth, ButtonHeight),
                FlatStyle = FlatStyle.System, // required for the UAC shield
                UseVisualStyleBackColor = true,
                Margin = new Padding(6, 0, 0, 0),
            };
            if (onClick != null) b.Click += onClick;
            return b;
        }

        /// <summary>Right-aligned button row (primary button first).</summary>
        public static FlowLayoutPanel ButtonRow(params Button[] buttons)
        {
            var row = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 12, 0, 0),
                WrapContents = false,
            };
            // RightToLeft: add in reverse so the first button ends up leftmost.
            for (int i = buttons.Length - 1; i >= 0; i--) row.Controls.Add(buttons[i]);
            return row;
        }

        /// <summary>Wires up the standard primary/Cancel pair and returns the Cancel button.</summary>
        public static Button CancelButton(Form form)
        {
            Button cancel = Button("Cancel");
            cancel.DialogResult = DialogResult.Cancel;
            form.CancelButton = cancel;
            return cancel;
        }

        /// <summary>Shows or hides the UAC shield on a button.</summary>
        public static void SetShield(Button button, bool show)
        {
            if (button.FlatStyle != FlatStyle.System) button.FlatStyle = FlatStyle.System;
            void Apply() => NativeMethods.SendMessage(button.Handle, NativeMethods.BCM_SETSHIELD, IntPtr.Zero, new IntPtr(show ? 1 : 0));
            if (button.IsHandleCreated)
            {
                Apply();
            }
            else
            {
                button.HandleCreated += (s, e) => Apply();
            }
        }

        public static void Info(IWin32Window owner, string text) =>
            MessageBox.Show(owner, text, AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static void Warn(IWin32Window owner, string text) =>
            MessageBox.Show(owner, text, AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public static void Error(IWin32Window owner, string text) =>
            MessageBox.Show(owner, text, AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);

        public static bool Confirm(IWin32Window owner, string text) =>
            MessageBox.Show(owner, text, AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes;

        /// <summary>
        /// Runs an EasyRun operation, turning a cancel into a quiet no-op and any expected failure into a
        /// message box. Returns true only when the action completed without error.
        /// </summary>
        public static bool Guard(IWin32Window owner, Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (EasyRunException ex) when (ex.Error == EasyRunError.Cancelled)
            {
                return false;
            }
            catch (EasyRunException ex)
            {
                Error(owner, ex.Message);
                return false;
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException))
            {
                Error(owner, "Unexpected error: " + ex.Message);
                return false;
            }
        }

        public static void OpenFolder(string folder)
        {
            string explorer = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
            using (Process.Start(new ProcessStartInfo(explorer, "\"" + folder + "\"") { UseShellExecute = false }))
            {
            }
        }
    }
}

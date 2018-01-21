/*

The MIT License (MIT)
---------------------

EasyRun - EasyRun has supercow powers!

Copyright © 2018 cyfrost <cyrus.frost@hotmail.com>

Permission is hereby granted, free of charge, to any person obtaining 
a copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies
or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
FOR AND CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
DEALINGS IN THE SOFTWARE.

*/


using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System.Text.RegularExpressions;
using System.Diagnostics;
using IWshRuntimeLibrary;
namespace EasyRun
{
    public partial class UACDelegate : Form
    {
        UDialogs msg;


        bool addsh = false;
        public string exec = null;
        public string tskname = null;
        public string alias = null;
        public string action_file_selected_path = null;
        public string location_temp = null;
        public string loc = null;
        public string type = null;
        public string SAFE_LOCATION_DIRECTORY;
        public string args = null;
        public string propargs = null;
        public string wdir = null;
        public string filename = null;

        public UACDelegate()
        {
            try
            {

                InitializeComponent();

                msg = new UDialogs();
                this.SAFE_LOCATION_DIRECTORY = new Form1().SAFE_LOCATION_DIRECTORY;
                textBox1.KeyDown += new KeyEventHandler(RetKey);
                textBox2.KeyDown += new KeyEventHandler(RetKey);
                textBox3.KeyDown += new KeyEventHandler(RetKey);
                textBox4.KeyDown += new KeyEventHandler(RetKey);
                checkBox2.KeyDown += new KeyEventHandler(RetKey);
                checkBox1.KeyDown += new KeyEventHandler(RetKey);
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
               
                this.Close();
                
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void UACDelegate_Load(object sender, EventArgs e)
        {
           
            textBox1.Select();
        }
        private void RetKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                button1.PerformClick();
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    CheckFileExists = true,
                    FileName = "",
                    Title = "Open"
                };
                DialogResult resl = ofd.ShowDialog();
                if (resl == DialogResult.OK)
                {
                    res robj = new res();
                    exec = ofd.FileName;
                    loc = robj.getLocation(exec);
                    type = robj.getExtension(exec);
                    filename = robj.getFileName(exec);
                    textBox1.Text = exec;
                }
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }

          
        }
        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == false)
            {
                textBox4.Enabled = false;
                label4.ForeColor = Color.Gray;
            }
            else {
                textBox4.Enabled = true;
                label4.ForeColor = Color.Black;
            }
        }
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked == true)
                addsh = true;
            else
                addsh = false;
        }
        private void Act() {
            try
            {
                if (!Directory.Exists(SAFE_LOCATION_DIRECTORY)) {
                    Directory.CreateDirectory(SAFE_LOCATION_DIRECTORY);
                }



                if (string.IsNullOrEmpty(textBox1.Text) || !System.IO.File.Exists(textBox1.Text))
                {
                    msg.ShowWarning("One or more required fields are missing values");
                }
                else if (checkBox2.Checked == true && string.IsNullOrEmpty(textBox4.Text))
                {
                    msg.ShowWarning("Alias cannot be empty or contain spaces");
                }
                else if (checkBox2.Checked == true && textBox4.Text.Contains(" "))
                {
                    msg.ShowWarning("Alias cannot be empty or contain spaces");
                }
                else
                {
                    var resul = msg.ConfirmAction("Do you want to confirm changes?");
                    if (resul == DialogResult.Yes)
                    {
                        propargs = textBox2.Text;
                        wdir = textBox3.Text;
                        TaskService tsObj = new TaskService();
                        TaskDefinition tsDef = tsObj.NewTask();
                        tsDef.Principal.RunLevel = TaskRunLevel.Highest;
                        tsDef.Actions.Add(new ExecAction(exec, propargs, wdir));
                        tsDef.Settings.AllowDemandStart = true;
                        tsDef.RegistrationInfo.Author = "EasyRun";
                        tsDef.RegistrationInfo.Description = "Allows creation of Trusted UAC Shortcuts by the user";
                        tsDef.Principal.DisplayName = filename;
                        tsDef.Settings.AllowHardTerminate = true;
                        tsDef.Settings.DisallowStartIfOnBatteries = false;
                        tsDef.Settings.MultipleInstances = TaskInstancesPolicy.StopExisting;
                        tsDef.Settings.Enabled = true;
                        tsDef.Settings.StopIfGoingOnBatteries = false;
                        tsDef.Settings.WakeToRun = true;
                        tskname = "EasyRun-" + filename;
                        tsObj.RootFolder.RegisterTaskDefinition("EasyRun-" + filename, tsDef);
                        if (addsh == true)
                        {
                            string x = "\"" + tskname +"\"";
                            string cmd = "@echo off" + Environment.NewLine + @"C:\Windows\System32\schtasks.exe /run /tn "+ x + Environment.NewLine + "exit";
                            EasyRun.res x1 = new EasyRun.res();
                            tskname = x1.removeExtension(tskname);
                            tskname += ".cmd";
                            string cmd_file = SAFE_LOCATION_DIRECTORY + tskname;
                            System.IO.File.WriteAllText(cmd_file, cmd);
                            action_file_selected_path = cmd_file;
                            location_temp = x1.getLocation(cmd_file);
                            WshShellClass wsh = new WshShellClass();
                            IWshRuntimeLibrary.IWshShortcut shortcut = wsh.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + tskname + ".lnk") as IWshRuntimeLibrary.IWshShortcut;
                            shortcut.Description = "EasyRun Trusted UAC Bypass shortcut for " + tskname;
                            shortcut.WindowStyle = 1;
                           shortcut.TargetPath = action_file_selected_path;
                            shortcut.Save();
                        }
                        if (checkBox2.Checked)
                        {
                            tskname = "EasyRun-" + filename;
                            alias = textBox4.Text;
                            alias += ".exe";
                                string x = "\"" + tskname + "\"";
                                string cmd = "@echo off" + Environment.NewLine + @"C:\Windows\System32\schtasks.exe /run /tn " + x + Environment.NewLine + "exit";
                                EasyRun.res x1 = new EasyRun.res();
                                tskname = x1.removeExtension(tskname);
                                tskname += ".cmd";
                                string cmd_file = SAFE_LOCATION_DIRECTORY + tskname;
                                System.IO.File.WriteAllText(cmd_file, cmd);
                                action_file_selected_path = cmd_file;
                                location_temp = x1.getLocation(cmd_file);
                                try
                                {
                                    string EXISTING_ALIAS_FILEMAPPED = (string)Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias).GetValue("");
                                    string regexSearch = new string(Path.GetInvalidPathChars());
                                    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                                    EXISTING_ALIAS_FILEMAPPED = r.Replace(EXISTING_ALIAS_FILEMAPPED, "");
                                    FileVersionInfo inf = FileVersionInfo.GetVersionInfo(EXISTING_ALIAS_FILEMAPPED);
                                DialogResult res = msg.ConfirmAction("A shortcut already exists with the name: " + alias + "\n\nDetails\n-------\nAlias: " + alias + "\n\nDescription: " + inf.FileDescription + "\nFile: " + EXISTING_ALIAS_FILEMAPPED.Substring(EXISTING_ALIAS_FILEMAPPED.LastIndexOf(@"\") + 1) + "\nPath: " + EXISTING_ALIAS_FILEMAPPED + "\n\nWould you like to overwrite it?");
                                    if (res == DialogResult.Yes)
                                    {
                                            FileVersionInfo ins = FileVersionInfo.GetVersionInfo(action_file_selected_path);
                                            Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias).Close();
                                            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias, true).SetValue("", action_file_selected_path);
                                            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias, true).SetValue("Path", location_temp + @"\");
                                           msg.ShowMessage("Success!");
                                            return;
                                        }
                                }
                                catch (NullReferenceException excep)
                                {
                                    FileVersionInfo ins = FileVersionInfo.GetVersionInfo(action_file_selected_path);
                                    DialogResult re = msg.ConfirmAction("Confirm new shortcut:\n\nDetails\n------\nNew Alias:  " + alias + "\n\nFile:  " + action_file_selected_path + "\nDescription:  " + ins.FileDescription + "\nPath:  " + location_temp + "\n\nWould you like to proceed?");

                                if (re == DialogResult.Yes) {
                                    Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias).Close();
                                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias, true).SetValue("", action_file_selected_path);
                                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias, true).SetValue("Path", location_temp + @"\");
                                    msg.ShowMessage("Success!");
                                    return;

                                }

                            }
                        }
                        msg.ShowMessage("Success!");
                    }
                }
            }
            catch (Exception exp)
            {
                msg.ShowError( exp.Message);
            }
        }
        private void Button4_Click(object sender, EventArgs e)
        {
        }
        private void Button5_Click(object sender, EventArgs e)
        {
           
        }
        private void UACDelegate_FormClosing(object sender, FormClosingEventArgs e)
        {
            Form AlObj = Application.OpenForms["Form1"];
            if (AlObj == null)
            {
                Form1 a = new Form1();
                a.Show();
                a.BringToFront();
            }
            else
            {
                AlObj.BringToFront();
                AlObj.Show();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form AlObj = Application.OpenForms["Form1"];
            if (AlObj == null)
            {
                Form1 a = new Form1();
                a.Show();
                a.BringToFront();
            }
            else
            {
                AlObj.BringToFront();
                AlObj.Show();
            }
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Act();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
           msg.ShowMessage("In a case where you need to run a program as Administrator without attending the User Account Control (UAC) prompt, this utility allows you to achieve that by creating a Trusted UAC shortcut to that program.\n\nThrough use of this utility:\n\n1. You can skip UAC prompts and directly run selected programs with Administrator rights\n\n2. You don't have to give up User Account Control (UAC) feature completely, a strong security implementation baked inside Windows since Windows Vista.\nOn the brighter side, you now have the freedom to skip the UAC prompt only for those programs you trust.");
        }
    }
}

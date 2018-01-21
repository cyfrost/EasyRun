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
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;

namespace EasyRun
{
    class CommandLineHandler
    {
        UDialogs msg = new UDialogs();
        res res = new res();


        public string targetImage;
        public string userInput;
        public string alias;
        public string filepath;
        public string dirpath;
        string SAFE_LOCATION_DIRECTORY;
        string DIRECTORY_ROOT = "C:";
        public bool clearMRU = false;



        public void CreateFileShortcut(string alias_p, string filepath_p) {
            this.SAFE_LOCATION_DIRECTORY = new Form1().SAFE_LOCATION_DIRECTORY;
            this.alias = alias_p;
            this.filepath = filepath_p;
            try
            {
                try
                {
                    if (alias.Contains(".") == false || alias.Substring(alias.LastIndexOf(".")) != ".exe")
                    {
                        alias += ".exe";

                    }
                    string alias_obj = (string)Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias).GetValue("");
                    string regexSearch = new string(Path.GetInvalidPathChars());
                    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    alias_obj = r.Replace(alias_obj, "");
                    FileVersionInfo inf = FileVersionInfo.GetVersionInfo(alias_obj);
                    DialogResult res = msg.ConfirmAction("A shortcut with the name you specified already exists" + "\n\nDetails\n-------\nAlias: " + alias + "\nFile: " + alias_obj.Substring(alias_obj.LastIndexOf(@"\") + 1) + "\nDescription: " + inf.FileDescription + "\nLocation: " + alias_obj + "\n\nWould you like to overwrite it?");

                    if (res == DialogResult.Yes)
                    {

                        targetImage = filepath;
                        WriteAlias();
                    }



                }

                catch (NullReferenceException excep)
                {

                    /*  DialogResult re = MessageBox.Show("Confirm new shortcut:\n\nDetails\n------\nNew Alias:  " + alias + "\n\nTarget:  " + filepath +  "\n\nWould you like to proceed?", "Confirm Settings", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                     if (re == DialogResult.OK)
                     {*/
                    targetImage = filepath;
                    WriteAlias();
                    /* }*/
                }
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }



         







        }




         public void CreateDirectoryShortcut(string alias_p, string dirpath_p)
         {
            this.SAFE_LOCATION_DIRECTORY = new Form1().SAFE_LOCATION_DIRECTORY;
            string COMMAND_FILE=null;

            try
            {

                try { if (!Directory.Exists(SAFE_LOCATION_DIRECTORY)) Directory.CreateDirectory(SAFE_LOCATION_DIRECTORY); } catch (Exception ex) { msg.ShowError(ex.Message); }

                try
                {

                    this.alias = alias_p;
                    this.dirpath = dirpath_p;
                    DIRECTORY_ROOT = dirpath.Substring(0, 2);
                    string cmd = "@echo off" + Environment.NewLine + DIRECTORY_ROOT + Environment.NewLine + "cd \"" + dirpath + "\"" + Environment.NewLine + "start ." + Environment.NewLine;
                    if (clearMRU)
                        cmd = cmd + @"Reg delete HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU /f" + Environment.NewLine;
                    cmd = cmd + "exit";
                    string CMD_FILE_NAME = alias;
                    if (CMD_FILE_NAME.Contains("."))
                        CMD_FILE_NAME = CMD_FILE_NAME.Substring(0, CMD_FILE_NAME.IndexOf("."));
                    CMD_FILE_NAME += ".cmd";
                    COMMAND_FILE = SAFE_LOCATION_DIRECTORY + CMD_FILE_NAME;

                    File.WriteAllText(COMMAND_FILE, cmd);

                    //cmdfilename and image = command_file
                }
                catch (Exception e)
                {
                    msg.ShowError(e.Message + "\n\n\n" + e.ToString());
                }



                try
                {


                    if (alias.Contains(".") == false || alias.Substring(alias.LastIndexOf(".")) != ".exe")
                    {
                        alias += ".exe";

                    }
                    string alias_obj = (string)Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias).GetValue("");
                    string regexSearch = new string(Path.GetInvalidPathChars());
                    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    alias_obj = r.Replace(alias_obj, "");
                    FileVersionInfo inf = FileVersionInfo.GetVersionInfo(alias_obj);
                    DialogResult res = msg.ConfirmAction("A shortcut with the name you specified already exists" + "\n\nDetails\n-------\nAlias: " + alias + "\nFile: " + alias_obj.Substring(alias_obj.LastIndexOf(@"\") + 1) + "\nDescription: " + inf.FileDescription + "\nLocation: " + alias_obj + "\n\nWould you like to overwrite it?");

                    if (res == DialogResult.Yes)
                    {
                        targetImage = COMMAND_FILE;

                        WriteAlias();
                    }



                }

                catch (NullReferenceException excep)
                {

                    /* DialogResult re = MessageBox.Show("Confirmation:\n\nDetails\n------\nNew Alias:  " + alias + "\n\nTarget:  " + dirpath + "\n\nWould you like to proceed?", "Confirm Settings", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                     if (re == DialogResult.OK)
                     {

                     */
                    targetImage = COMMAND_FILE;
                    WriteAlias();
                    /*}
                     */


                }
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }









        }


        void WriteAlias()
        {
            try
            {
               
                Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias).Close();
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias, true).SetValue("", targetImage);
                Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias, true).SetValue("Path", res.getLocation(targetImage) + @"\");
                SuccessMessage(alias);
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message + "\n\n\n" + exp.ToString());
            }
        }

        void SuccessMessage(string al)
        {
          
            al = al.Replace(".exe", "");
            msg.ShowMessage("Success!\n\nTry running your new shortcut:\n\nStep 1: open Run Dialog Box (WIN+R)\n\nStep 2: type \"" + al + "\"\n\nStep 3: hit OK");

        }

     

        public void askUser(string title, bool Show_Mru_Clear_CB, string formTitle)
        {
            try
            {
                Form foo = new Form();
                TextBox tb = new TextBox();
                CheckBox CB_ClearMru = new CheckBox();
                CB_ClearMru.Text = "Clear Run MRU on execution";
                RadioButton rb_onlyme = new RadioButton();
                rb_onlyme.Text = "Just me";
                RadioButton rb_all = new RadioButton();
                rb_all.Text = "All users";
                Button accept = new Button();
                Label val = new Label();


                val.Text = title;
                val.AutoSize = true;


                CB_ClearMru.AutoSize = true;
                CB_ClearMru.Checked = false;

                val.Location = new Point(12, 10);
                tb.Location = new Point(12, 34);
                accept.Location = new Point(188, 31);
                accept.Size = new Size(75, 23);

                accept.FlatStyle = FlatStyle.Flat;

                accept.UseVisualStyleBackColor = false;

                accept.BackColor = Color.Transparent;

                accept.BackgroundImage = Properties.Resources.normal;

                accept.MouseDown += delegate (object sender, MouseEventArgs e)
                {
                    accept.BackgroundImage = Properties.Resources.click;
                };

                accept.MouseEnter += delegate (object sender, EventArgs e)
                {
                    accept.BackgroundImage = Properties.Resources.hover;
                };

                accept.MouseUp += delegate (object sender, MouseEventArgs e)
                {
                    accept.BackgroundImage = Properties.Resources.hover;
                };
                accept.MouseLeave += delegate (object sender, EventArgs e)
                {
                    accept.BackgroundImage = Properties.Resources.normal;
                };
                accept.FlatAppearance.BorderSize = 0;
                accept.FlatAppearance.MouseDownBackColor = Color.Transparent;
                accept.FlatAppearance.MouseOverBackColor = Color.Transparent;


                foo.Controls.Add(val);
                foo.Controls.Add(tb);
                foo.Controls.Add(accept);

                foo.Icon = Properties.Resources.Dakirby309_Simply_Styled_Run;


                foo.ShowIcon = true;
                foo.ShowInTaskbar = true;
                foo.BackColor = Color.White;

                foo.Text = formTitle;
                foo.AllowDrop = false;
                foo.FormBorderStyle = FormBorderStyle.FixedSingle;
                tb.Size = new Size(new Point(165, 20));
                accept.Text = "OK";
                tb.KeyDown += delegate (object sender, KeyEventArgs e)
                {
                    if (e.KeyCode == Keys.Enter) { accept.PerformClick(); }
                    if (e.KeyCode == Keys.Escape)
                    {
                        if (msg.ConfirmAction("Do you want to exit?") == DialogResult.Yes)
                        {
                            Application.Exit();
                        }


                    }
                };
                CB_ClearMru.KeyDown += delegate (object sender, KeyEventArgs e)
                {
                    if (e.KeyCode == Keys.Enter) { accept.PerformClick(); }
                };
                accept.Click += delegate
                {
                    if (string.IsNullOrEmpty(tb.Text) || string.IsNullOrWhiteSpace(tb.Text) || tb.Text.Contains(" "))
                    {
                        msg.ShowWarning("Alias cannot be empty or contain spaces");
                    }
                    else
                    {
                        if (CB_ClearMru.Checked)
                            clearMRU = true;
                        userInput = tb.Text;
                        foo.Close();
                    }

                };

                foo.KeyDown += delegate (object sender, KeyEventArgs e)
                {
                    if (e.KeyCode == Keys.Escape)
                    {
                        if (msg.ConfirmAction("Do you want to exit?") == DialogResult.Yes) {
                            Application.Exit();
                        }
                        

                    }
                };

                foo.FormClosing += delegate
                {
                    if (string.IsNullOrEmpty(tb.Text))
                        Environment.Exit(0);
                    userInput = tb.Text;
                };

                foo.MaximizeBox = false;

                foo.MinimizeBox = false;
                foo.StartPosition = FormStartPosition.CenterScreen;

                foo.Width = 284;
                foo.Height = 108;
                if (Show_Mru_Clear_CB)
                {
                    CB_ClearMru.Location = new Point(12, 60);
                    foo.Controls.Add(CB_ClearMru);
                    foo.Height = 115;
                }


                foo.ShowDialog();
                tb.Focus();
                tb.Select();
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }


         
        }
    }
}

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
using Microsoft.Win32;


namespace EasyRun
{
    public partial class editConfig : Form
    {

        public string alias = null;
        public string target = null;
        public string regkey = null;
        public string access = null;
        public string newalias = null;
        public string newtarget = null;
        public short result = 0;

        private aliases inst;

        UDialogs msg;

        public editConfig(string aliasObj, string targetObj, string regkeyObj, aliases child)
        {
            try
            {

                InitializeComponent();


                msg = new UDialogs();

                textBox1.KeyDown += new KeyEventHandler(RetKey);
                textBox2.KeyDown += new KeyEventHandler(RetKey);

                this.alias = aliasObj;
                this.target = targetObj;
                this.regkey = regkeyObj;
                textBox1.Text = alias;
                textBox2.Text = target;
                this.Text = "Edit " + alias;
                if (regkey.Contains("HKEY_CURRENT_USER"))
                    access = "HKCU";
                else if (regkey.Contains("HKEY_LOCAL_MACHINE"))
                    access = "HKLM";
                this.inst = child;
                inst.editDropped = 0;
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
                inst.editDropped = 1;
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void RetKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
                button1.PerformClick();
        }



        private void editConfig_Load(object sender, EventArgs e)
        {
            textBox1.Select();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            inst.editDropped = 1;

            this.Close();
        }



        private void ClickBait() {

            try
            {
                if (string.IsNullOrEmpty(textBox1.Text) || string.IsNullOrEmpty(textBox2.Text))
                {
                    msg.ShowWarning("One or more required fields is missing values");
                }
                else if (textBox1.Text == alias && textBox2.Text == target)
                {
                    inst.editDropped = 1; //bail immediately cause no changes

                    this.Close();
                }
                else if (textBox1.Text.Contains(" "))
                    msg.ShowWarning("Alias cannot be empty or contain white spaces.");
                else
                {
                    if (textBox1.Text.Contains(".") == false || textBox1.Text.Substring(textBox1.Text.LastIndexOf(".")) != ".exe")
                    {
                        textBox1.Text = textBox1.Text + ".exe";

                    }
                    newalias = textBox1.Text;
                    newtarget = textBox2.Text;
                    var resul = msg.ConfirmAction("Are you sure you want to confirm changes?");
                    if (resul == DialogResult.Yes)
                    {
                        if (access == "HKLM")
                        {
                            Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + newalias).Close();
                            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + newalias, true).SetValue("", newtarget);
                            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + newalias, true).SetValue("Path", newtarget.Substring(0, newtarget.LastIndexOf(@"\")) + @"\");
                            if (newalias != alias)
                            {
                                Registry.LocalMachine.DeleteSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\" + alias, false);

                            }

                            result = 1;
                            inst.editDropped = 0; //trigger list refresh
                            this.Close();
                        }
                        else if (access == "HKCU")
                        {
                            Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + newalias).Close();
                            Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + newalias, true).SetValue("", newtarget);
                            Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + newalias, true).SetValue("Path", newtarget.Substring(0, newtarget.LastIndexOf(@"\")) + @"\");
                            if (newalias != alias)
                            {
                                Registry.CurrentUser.DeleteSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\" + alias, false);


                            }

                            result = 1;
                            inst.editDropped = 0; //trigger list refresh

                            this.Close();
                        }
                    }


                }

            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e) 
        {
            ClickBait();

        }

        private void button3_Click(object sender, EventArgs e)
        {

            openFileDialog1.Title = "Select file";
            openFileDialog1.Filter = "All Files|*.*";
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.FileName = "";
            DialogResult res = openFileDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                newtarget = openFileDialog1.FileName;
                textBox2.Text = newtarget;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void editConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
             if (textBox1.Text == alias && textBox2.Text == target)
            {
                inst.editDropped = 1; //bail immediately cause no changes

               
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ClickBait();
        }

      







    }
}

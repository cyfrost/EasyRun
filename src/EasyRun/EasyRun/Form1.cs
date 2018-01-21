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

using Microsoft.Win32;
using System;
using Microsoft.Win32.TaskScheduler;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Linq;

namespace EasyRun
{
    public partial class Form1 : Form
    {

        UDialogs msg = new UDialogs();


        public string appname = Assembly.GetExecutingAssembly().GetName().Name;
        public string copyright = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false).Cast<AssemblyCopyrightAttribute>().FirstOrDefault().Copyright;
        public string desc = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).Cast<AssemblyDescriptionAttribute>().FirstOrDefault().Description;
        public string version = Assembly.GetExecutingAssembly().GetName().Version.ToString() + " (build " + DateTime.Now.ToString("Myy") + ")";
        

        public static string curr = new res().getLocation(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public string lib1 = curr + @"\Interop.IWshRuntimeLibrary.dll";
        public string lib2 = curr + @"\Microsoft.Win32.TaskScheduler.dll";
        public string lib3 = curr + @"\Explintegrate.dll";
        public string lib4 = curr + @"\CommandLine.dll";


        bool isxp = false;
        bool ACTION_FOLDER = false;
        bool ACTION_FILE = true;
        bool PATH_VERIFIED_ACTION_FOLDER = false;
        bool PATH_VERIFIED_ACTION_FILE = false;
        string ACTION_FILE_SELECTED_FILEPATH = null;
        string ACTION_FILE_SELECTED_FILELOCATION = null;
        string ACTION_FILE_SELECTED_FILENAME = null;
        public string ALIAS = null;
        bool ALIAS_VERIFIED = false;
        string COMMAND_FILE_LOCATION = null;
        bool OPTION_SELECTED_USESAFELOCATION = false;
        bool OPTION_SELECTED_CUSTOM_ALIAS = true;
        string CMD_FILE_NAME = null;
        string ACTION_FOLDER_SELECTED_FOLDER = null;
        string DIRECTORY_ROOT = "C:";
        public string SAFE_LOCATION_DIRECTORY = @"C:\Program Files\EasyRun\usr_meta\";
        public string SELF_SHORTCUT_PATH = @"C:\Program Files\EasyRun\";
        string COMMAND_FILE = null;
        string username = " (" + Environment.UserDomainName + @"\" + Environment.UserName + ")";
        string OPTION_WHO_CAN_USE = "Everyone";
        string successalias = null;
        int PTR_FILEDIALOG = 0;
        int PTR_FOLDERDIALOG = 0;
        bool UniversalAlias = true;
        //string oldfilters = "Executables|*.exe;*.cmd;*.bat;*.com;*.vbs;*.ps1";
        string AppFilters = "All Files|*.*";
        string LOCATION_TEMP = null;


        public Form1()
        {
          //  new FailSafeInit().CheckFailSafe();
            InitializeComponent();

            try
            {

                

                this.Text = appname;



                textBox1.KeyDown += new KeyEventHandler(ExpressionHandler);
                textBox2.KeyDown += new KeyEventHandler(ExpressionHandler);
                textBox3.KeyDown += new KeyEventHandler(ExpressionHandler);

                radioButton1.KeyDown += new KeyEventHandler(ExpressionHandler);
                radioButton2.KeyDown += new KeyEventHandler(ExpressionHandler);
                radioButton3.KeyDown += new KeyEventHandler(ExpressionHandler);
                radioButton4.KeyDown += new KeyEventHandler(ExpressionHandler);

                checkBox2.KeyDown += new KeyEventHandler(ExpressionHandler);
                checkBox3.KeyDown += new KeyEventHandler(ExpressionHandler);

            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }


          
            /*
            try
            {
                if (!Directory.Exists(SAFE_LOCATION_DIRECTORY))
                    Directory.CreateDirectory(SAFE_LOCATION_DIRECTORY);
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }
            */




        }
        /*
        static void SetRunAsAdmin(string exeFilePath)
        {
                var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers", true);
                if (key == null)
                    throw new InvalidOperationException(@"Cannot open registry key HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers.");
                using (key)
                    key.SetValue(exeFilePath, "RUNASADMIN");
        }
        static void RemoveRunAsAdmin(string exeFilePath)
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers");
            if (key == null)
                throw new InvalidOperationException(@"Cannot open registry key HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers.");
                if (!string.IsNullOrEmpty((string)key.GetValue(exeFilePath))) {
                    using (key)
                        key.DeleteValue(exeFilePath);
                }
        }

             try
            {

            }
            catch (Exception ex) {
                msg.ShowError(ex.Message);
            }











     */

        private bool IsWinXP()
        {
            try
            {
                OperatingSystem os = Environment.OSVersion;
                Version vs = os.Version;
                if (os.Platform == PlatformID.Win32NT)
                {
                    switch (vs.Major)
                    {
                        case 5:
                            if (vs.Minor != 0)
                            {
                                //XP Here
                                isxp = true;
                                addNewTrustedUACShortcutToolStripMenuItem.Visible = false;
                                toolStripSeparator1.Visible = false;
                                return true;
                            }
                            break;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
                return false;

            }


           

        }
        private void Form1_Load(object sender, EventArgs e)
        {
           
            try
            {

                IsWinXP();
                textBox3.Hide();
                button3.Hide();
                label6.Hide();
                radioButton3.Text += username;
                if (isxp == true)
                    checkBox3.Hide();

                //new ExplorerIntegration().CreateIntegration();
               

                if (new ExplorerIntegration().IntegrationEnabled())
                {
                    enableToolStripMenuItem.Checked = true;
                    disableToolStripMenuItem.Checked = false;
                }
                else
                {
                    enableToolStripMenuItem.Checked = false;
                    disableToolStripMenuItem.Checked = true;


                }

                if (new ExplorerIntegration().IntegrationCorrupt()) {
                    if (msg.ConfirmAction("The explorer integration module is corrupt. would you like to fix it?") == DialogResult.Yes)
                        new ExplorerIntegration().CreateIntegration(false);

               }
                
                            textBox2.Select();

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
                UDialogs t = new UDialogs();
                if (t.ConfirmAction("Are you sure you want to exit?") == DialogResult.Yes) {
                    this.Close();
                    return true;

                }
                return false;

            }
            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void ExpressionHandler(object sender, KeyEventArgs n) {
            try
            {
                if (n.KeyCode == Keys.Enter)
                {
                    button1.PerformClick();
                }
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }


        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            

            if (radioButton1.Checked == true)
            {
                textBox1.Enabled = false;
                OPTION_SELECTED_CUSTOM_ALIAS = false;
            }
            else {
                textBox1.Enabled = true;
                OPTION_SELECTED_CUSTOM_ALIAS = true;
            }
        }
        private short CheckSettings() {

            try
            {
                if (!Directory.Exists(SAFE_LOCATION_DIRECTORY))
                    Directory.CreateDirectory(SAFE_LOCATION_DIRECTORY);

            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
                return -1;
            }
            try
            {
                if (ACTION_FOLDER)
                {
                    if (System.IO.Directory.Exists(textBox3.Text))
                        PATH_VERIFIED_ACTION_FOLDER = true;
                    else
                        PATH_VERIFIED_ACTION_FOLDER = false;
                    if (PATH_VERIFIED_ACTION_FOLDER == false)
                        return 3; //folder path not verified or invalid
                    ALIAS = textBox1.Text;
                    if (string.IsNullOrEmpty(ALIAS))
                    {
                        return 1; //file alias is empty or has white spaces
                    }
                    if (ALIAS.Contains(" "))
                        return 5; // aliases cannot contain white spaces
                    CMD_FILE_NAME = ALIAS;
                    if (ALIAS.Contains("."))
                        ALIAS = ALIAS.Substring(0, ALIAS.IndexOf("."));
                    ALIAS += ".exe";
                    ALIAS_VERIFIED = true;
                    if (CMD_FILE_NAME.Contains("."))
                        CMD_FILE_NAME = CMD_FILE_NAME.Substring(0, CMD_FILE_NAME.IndexOf("."));
                    CMD_FILE_NAME += ".cmd";
                    string cmd = "@echo off" + Environment.NewLine + DIRECTORY_ROOT + Environment.NewLine + "cd \"" + ACTION_FOLDER_SELECTED_FOLDER + "\"" + Environment.NewLine + "start ." + Environment.NewLine;
                    if (checkBox3.Checked == true)
                        cmd = cmd + @"Reg delete HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU /f" + Environment.NewLine;
                    cmd = cmd + "exit";
                    COMMAND_FILE = SAFE_LOCATION_DIRECTORY + CMD_FILE_NAME;
                    File.WriteAllText(COMMAND_FILE, cmd);
                    COMMAND_FILE_LOCATION = COMMAND_FILE.Substring(0, COMMAND_FILE.LastIndexOf(@"\"));
                    ACTION_FILE_SELECTED_FILEPATH = COMMAND_FILE;
                    ACTION_FILE_SELECTED_FILENAME = CMD_FILE_NAME;
                    ACTION_FILE_SELECTED_FILELOCATION = ACTION_FILE_SELECTED_FILEPATH.Substring(0, ACTION_FILE_SELECTED_FILEPATH.LastIndexOf(@"\"));
                    return 0;
                }
                else if (ACTION_FILE)
                {
                    string pth = textBox2.Text;
                    if (!string.IsNullOrEmpty(pth) && System.IO.File.Exists(pth))
                        PATH_VERIFIED_ACTION_FILE = true;
                    else
                        PATH_VERIFIED_ACTION_FILE = false;
                    if (PATH_VERIFIED_ACTION_FILE == false)
                        return 2;//file path not verified
                    ACTION_FILE_SELECTED_FILEPATH = textBox2.Text;
                    ACTION_FILE_SELECTED_FILENAME = ACTION_FILE_SELECTED_FILEPATH.Substring(ACTION_FILE_SELECTED_FILEPATH.LastIndexOf(@"\") + 1);
                    ACTION_FILE_SELECTED_FILELOCATION = ACTION_FILE_SELECTED_FILEPATH.Substring(0, ACTION_FILE_SELECTED_FILEPATH.LastIndexOf(@"\"));
                    ALIAS = textBox1.Text;
                    if (OPTION_SELECTED_CUSTOM_ALIAS == false)
                    {
                        ALIAS = ACTION_FILE_SELECTED_FILENAME;
                        textBox1.Text = ALIAS;
                    }
                    if (string.IsNullOrEmpty(ALIAS))
                    {
                        return 1; //file alias is empty or has white spaces
                    }
                    if (ALIAS.Contains(" "))
                        return 5; // aliases cannot contain white spaces
                    if (ALIAS.Contains("."))
                    {
                        ALIAS = ALIAS.Substring(0, ALIAS.IndexOf("."));
                    }
                    ALIAS += ".exe";
                    ALIAS_VERIFIED = true;
                    if (OPTION_SELECTED_USESAFELOCATION)
                    {
                        if (!Directory.Exists(SAFE_LOCATION_DIRECTORY))
                            Directory.CreateDirectory(SAFE_LOCATION_DIRECTORY);
                        try
                        {
                            File.Copy(ACTION_FILE_SELECTED_FILEPATH, SAFE_LOCATION_DIRECTORY + ACTION_FILE_SELECTED_FILENAME, true);
                            ACTION_FILE_SELECTED_FILEPATH = SAFE_LOCATION_DIRECTORY + ACTION_FILE_SELECTED_FILENAME;
                            ACTION_FILE_SELECTED_FILELOCATION = ACTION_FILE_SELECTED_FILEPATH.Substring(0, ACTION_FILE_SELECTED_FILEPATH.LastIndexOf(@"\"));
                        }
                       catch (Exception exp)
                        {
                             msg.ShowError(exp.Message);
                        }
                    }
                    else
                    {
                        ACTION_FILE_SELECTED_FILELOCATION = ACTION_FILE_SELECTED_FILEPATH.Substring(0, ACTION_FILE_SELECTED_FILEPATH.LastIndexOf(@"\"));
                    }
                    return 0;
                }
                return 0;
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
                return 6;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {

                short ret_ptr = CheckSettings();
                if (ret_ptr == 0)
                {
                    try
                    {
                        string EXISTING_ALIAS_FILEMAPPED = (string)Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + ALIAS).GetValue("");
                        string regexSearch = new string(Path.GetInvalidPathChars());
                        Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                        EXISTING_ALIAS_FILEMAPPED = r.Replace(EXISTING_ALIAS_FILEMAPPED, "");
                        FileVersionInfo inf = FileVersionInfo.GetVersionInfo(EXISTING_ALIAS_FILEMAPPED);
                        DialogResult res = msg.ConfirmAction("A shortcut with the name you specified already exists\n\nDetails\n-------\nAlias: " + ALIAS + "\n\nDescription: " + inf.FileDescription + "\nFile: " + EXISTING_ALIAS_FILEMAPPED.Substring(EXISTING_ALIAS_FILEMAPPED.LastIndexOf(@"\") + 1) + "\nPath: " + EXISTING_ALIAS_FILEMAPPED + "\n\nWould you like to overwrite it?");

                        if (res == DialogResult.Yes)
                        {
                            WriteAlias();
                        }
                    }
                    catch (NullReferenceException excep)
                    {
                        FileVersionInfo ins = FileVersionInfo.GetVersionInfo(ACTION_FILE_SELECTED_FILEPATH);
                        DialogResult re = msg.ConfirmAction("Confirm new shortcut:\n\nDetails\n------\nNew Alias:  " + ALIAS + "\nWho can use this shortcut:  " + OPTION_WHO_CAN_USE + "\nUsing Safe Location:  " + OPTION_SELECTED_USESAFELOCATION + "\n\nFile:  " + ACTION_FILE_SELECTED_FILENAME + "\nDescription:  " + ins.FileDescription + "\nPath:  " + ACTION_FILE_SELECTED_FILELOCATION + "\n\nWould you like to proceed?");
                        if (re == DialogResult.Yes)
                        {
                            WriteAlias();
                        }
                    }
                }
                else if (ret_ptr == 1)
                {
                    msg.ShowWarning("One or more required fields are missing values.");
                    return;
                }
                else if (ret_ptr == 2)
                {
                    msg.ShowWarning("The specified path is not valid");
                    return;
                }
                else if (ret_ptr == 3)
                {
                    msg.ShowWarning("The specified path is not valid");
                    return;
                }
                else if (ret_ptr == 5)
                {
                    msg.ShowWarning("Alias cannot be empty or contain Spaces.");
                    return;

                }
                else if (ret_ptr == -1)
                {
                    msg.ShowWarning("An internal error occured.");
                    return;
                }
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }

        }
        void WriteAlias() {
            try
            {
                FileVersionInfo ins = FileVersionInfo.GetVersionInfo(ACTION_FILE_SELECTED_FILEPATH);
                    if (UniversalAlias)
                    {
                        Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + ALIAS).Close();
                        Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + ALIAS, true).SetValue("", ACTION_FILE_SELECTED_FILEPATH);
                        Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + ALIAS, true).SetValue("Path", LOCATION_TEMP + @"\");
                        SuccessMessage(ALIAS);
                    }
                    else
                    {
                        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + ALIAS).Close();
                        Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + ALIAS, true).SetValue("", ACTION_FILE_SELECTED_FILEPATH);
                        Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + ALIAS, true).SetValue("Path", LOCATION_TEMP + @"\");
                        SuccessMessage(ALIAS);
                    }
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }
        void SuccessMessage(string al) {
            successalias = al;
            successalias = successalias.Replace(".exe", "");
            textBox1.Clear();
            textBox2.Clear();
            radioButton2.Checked = true;
            msg.ShowMessage("Success!\n\nTry running your new shortcut:\n\nStep 1: open Run Dialog Box (WIN+R)\n\nStep 2: type \"" + successalias + "\"\n\nStep 3: hit OK");

        }
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked == true)
            {
                UniversalAlias = false;
                OPTION_WHO_CAN_USE = username;
            }
            else {
                UniversalAlias = true;
                OPTION_WHO_CAN_USE = "Everyone";
            }
        }
        private void getAllAliasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void managealiases(object sender, EventArgs e) {
            Form AlObj = Application.OpenForms["aliases"];
            if (AlObj == null)
            {
               aliases a = new aliases();
                a.Show();
                a.BringToFront();
            }
            else
            {
                AlObj.Show();
                AlObj.BringToFront();
            }
        }
        private void usageToolStripMenuItem_Click(object sender, EventArgs e)
        {
           // string msg = "EasyRun allows you to " + desc.ToLower() + "\n\nAn action is performed by running a custom Alias chosen by the user. The target actions include but are not limited to opening any file/folder, creating trusted UAC shortcuts for Programs --Useful when a program needs to bypass the UAC prompt";
           // MessageBox.Show(msg, "Usage", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new about().ShowDialog();

        }
        private void restartEasyRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked == true)
                OPTION_SELECTED_USESAFELOCATION = true;
            else
                OPTION_SELECTED_USESAFELOCATION = false;
        }
        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton6.Checked == false)
            {
                textBox3.Show();
                button3.Show();
                label6.Show();

                label1.Hide();
                button2.Hide();
                textBox2.Hide();



                ACTION_FILE = false;
                ACTION_FOLDER = true;
                checkBox3.Enabled = true;
                
                


                radioButton1.Enabled = false;
                checkBox2.Enabled = false;
                textBox3.Select();
           
            }
            else {


                textBox3.Hide();
                button3.Hide();
                label6.Hide();


                label1.Show();
                button2.Show();
                textBox2.Show();

                
                ACTION_FILE = true;
                ACTION_FOLDER = false;
              
               
               
               
                radioButton1.Enabled = true;
                checkBox2.Enabled = true;
                checkBox3.Enabled = false;

                textBox2.Select();
                
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                folderBrowserDialog1.ShowNewFolderButton = true;
                folderBrowserDialog1.SelectedPath = "";
                folderBrowserDialog1.Description = "Select folder";
                DialogResult res = folderBrowserDialog1.ShowDialog();
                if (res == DialogResult.OK)
                {
                    ACTION_FOLDER_SELECTED_FOLDER = folderBrowserDialog1.SelectedPath;
                    textBox3.Text = ACTION_FOLDER_SELECTED_FOLDER;
                    //FolderForOpenPathVerified = true;
                    DIRECTORY_ROOT = ACTION_FOLDER_SELECTED_FOLDER.Substring(0, 2);
                    PATH_VERIFIED_ACTION_FOLDER = true;
                    PTR_FOLDERDIALOG = 1;
                    return;
                }
                PTR_FOLDERDIALOG = 0;
                PATH_VERIFIED_ACTION_FOLDER = false;
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }
        private void button2_click(object sender, EventArgs e) {
            try
            {
                openFileDialog1.Title = "Select file";
                openFileDialog1.Filter = AppFilters;
                openFileDialog1.CheckFileExists = true;
                openFileDialog1.FileName = "";
                DialogResult res = openFileDialog1.ShowDialog();
                if (res == DialogResult.OK)
                {
                  textBox2.Text = openFileDialog1.FileName;
                    ACTION_FILE_SELECTED_FILEPATH = openFileDialog1.FileName;
                    ACTION_FILE_SELECTED_FILENAME = ACTION_FILE_SELECTED_FILEPATH.Substring(ACTION_FILE_SELECTED_FILEPATH.LastIndexOf(@"\") + 1);
                    ACTION_FILE_SELECTED_FILELOCATION = ACTION_FILE_SELECTED_FILEPATH.Substring(0, ACTION_FILE_SELECTED_FILEPATH.LastIndexOf(@"\"));
                    PATH_VERIFIED_ACTION_FILE = true;
                    PTR_FILEDIALOG = 1;
                    return;
                }
                PTR_FILEDIALOG = 0;
                PATH_VERIFIED_ACTION_FILE = false;
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }
        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            button1.BackgroundImage = Properties.Resources.click;
        }
        private void button1_MouseEnter(object sender, EventArgs e)
        {
            button1.BackgroundImage = Properties.Resources.hover;
        }
        private void button1_MouseLeave(object sender, EventArgs e)
        {
            button1.BackgroundImage = Properties.Resources.normal;
        }
        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            button1.BackgroundImage = Properties.Resources.hover;
        }

        public void AddSelfShortcut() {
            try
            {
                if (IsWinXP())
                {
                    SelfIntegrationInitChecks();
                   


                    radioButton6.Checked = true;

                    textBox2.Text = @"C:\Program Files\EasyRun\EasyRun.exe";
                    textBox1.Text = "er";


                    radioButton4.Checked = true;
                    System.Threading.Thread.Sleep(1000);
                    button1.PerformClick();
                    //  SuccessMessage("er");
                }


                else 
                  
                        addERDelegate();
                   
                  
            }
            catch (Exception ex)
            {
                
                msg.ShowError(ex.Message);
            }
           

        }


        private void addEasyRunAsAShortcutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddSelfShortcut();
            /*   if (!AliasExists("er.exe"))
               {

                     AddSelfShortcut();

               }
               else {
                   string ExistingTarget = (string)Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\er.exe").GetValue("");
                   if (ExistingTarget != @"C:\Program Files\EasyRun\EasyRun.exe")
                   {

                       AddSelfShortcut();
                   }
                   else {
                       msg.ShowMessage("Success!");
                   }


               }
               */
            //  
        }

        public bool AliasExists(string MatchAlias)
        {
            try
            {
                string EXISTING_ALIAS_FILEMAPPED = (string)Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + MatchAlias).GetValue("");
                return true;

            }
            catch (NullReferenceException excep)
            {
                return false;
            }
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox3.Clear();
            radioButton6.Checked = true;
            radioButton2.Checked = true;
            radioButton4.Checked = true;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            textBox2.Clear();
            textBox1.Clear();
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form AlObj = Application.OpenForms["aliases"];
            if (AlObj == null)
            {
                aliases a = new aliases();
                a.Show();
                a.BringToFront();
            }
            else {
                AlObj.Show();
                AlObj.BringToFront();
            }
        }
        private void addNewTrustedUACShortcutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form AlObj = Application.OpenForms["UACDelegate"];
            if (AlObj == null)
            {
                UACDelegate a = new UACDelegate();
                this.Hide();
                this.SendToBack();
                a.Show();
                a.BringToFront();
            }
            else
            {
                this.Hide();
                this.SendToBack();
                AlObj.Show();
                AlObj.BringToFront();
            }
        }
        private string quotestr(string str) { 
             return "\"" + str + "\"";
        }
        private string remIllegal(string str) {
            try
            {
                string regexSearch = new string(Path.GetInvalidPathChars());
                Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                return r.Replace(str, "");
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
                return null;
            }
         
        }

     


        public void CopyLibrary(string srcFile, string destFile) {

            try {

                if (File.Exists(destFile))

                {
                    File.SetAttributes(destFile, FileAttributes.Normal);
                    File.Delete(destFile);
                }

                srcFile = quotestr(srcFile);
                srcFile = remIllegal(srcFile);

                File.Copy(srcFile, destFile, true);
                File.SetAttributes(destFile, FileAttributes.Normal);


            }

            catch (Exception e) { msg.ShowError(e.Message); }


        }

        private void SelfIntegrationInitChecks() {
            try
            {
               // string ExistingTarget = (string)Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\er.exe").GetValue("");


                string curr = System.Reflection.Assembly.GetExecutingAssembly().Location;
                EasyRun.res newres = new EasyRun.res();
                curr = newres.getLocation(curr);




                if (!Directory.Exists(SELF_SHORTCUT_PATH))
                {
                    Directory.CreateDirectory(SELF_SHORTCUT_PATH);
                }
                if (!Directory.Exists(SAFE_LOCATION_DIRECTORY))
                {
                    Directory.CreateDirectory(SAFE_LOCATION_DIRECTORY);
                }



               


                string sfelocation = SELF_SHORTCUT_PATH + "EasyRun.exe";

               

              
                    try
                    {
                    if (curr != @"C:\Program Files\EasyRun")
                    {
                        System.IO.File.Copy(System.Reflection.Assembly.GetExecutingAssembly().Location, SELF_SHORTCUT_PATH + "EasyRun.exe", true);
                        CopyLibrary(lib1, SELF_SHORTCUT_PATH + "Interop.IWshRuntimeLibrary.dll");
                        CopyLibrary(lib2, SELF_SHORTCUT_PATH + "Microsoft.Win32.TaskScheduler.dll");
                        CopyLibrary(lib3, SELF_SHORTCUT_PATH + "Explintegrate.dll");
                        CopyLibrary(lib4, SELF_SHORTCUT_PATH + "CommandLine.dll");

                    }
                   
                    }

                    catch (Exception exp)
                    {
                        msg.ShowError(exp.Message);
                    }
                
            }
            catch (Exception e) {
                
                msg.ShowError(e.Message + "\n\nRaw:\n\n" + e.ToString());
            }
           
        }


        private void addERDelegate() {
            try {
                string sfelocation = SELF_SHORTCUT_PATH + "EasyRun.exe";

                 SelfIntegrationInitChecks();
              
                
                TaskService tsObj = new TaskService();
                TaskDefinition tsDef = tsObj.NewTask();
                tsDef.Principal.RunLevel = TaskRunLevel.Highest;
                tsDef.Actions.Add(new ExecAction(sfelocation));
                tsDef.Settings.AllowDemandStart = true;
                tsDef.RegistrationInfo.Author = "EasyRun";
                tsDef.RegistrationInfo.Description = "Start EasyRun control panel on-demand";
                tsDef.Settings.AllowHardTerminate = true;
                tsDef.Settings.DisallowStartIfOnBatteries = false;
                tsDef.Settings.MultipleInstances = TaskInstancesPolicy.StopExisting;
                tsDef.Settings.Enabled = true;
                tsDef.Settings.StopIfGoingOnBatteries = false;
                tsDef.Settings.WakeToRun = true;
                string tskname = "EasyRunUACDelegate";
                tsObj.RootFolder.RegisterTaskDefinition("EasyRunUACDelegate", tsDef);
                string cmd = "@echo off" + Environment.NewLine + @"C:\Windows\System32\schtasks.exe /run /tn EasyRunUACDelegate" + Environment.NewLine + "exit";
                string alias = "er.exe";
                tskname += ".cmd";
                string cmd_file = SAFE_LOCATION_DIRECTORY + tskname;
                System.IO.File.WriteAllText(cmd_file, cmd);
                string action_file_selected_path = cmd_file;
                EasyRun.res x1 = new EasyRun.res();
                string location_temp = x1.getLocation(cmd_file);
                try
                {
                    string EXISTING_ALIAS_FILEMAPPED = (string)Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\er.exe").GetValue("");
                    string regexSearch = new string(Path.GetInvalidPathChars());
                    Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
                    EXISTING_ALIAS_FILEMAPPED = r.Replace(EXISTING_ALIAS_FILEMAPPED, "");
                    FileVersionInfo inf = FileVersionInfo.GetVersionInfo(EXISTING_ALIAS_FILEMAPPED);
                    FileVersionInfo ins = FileVersionInfo.GetVersionInfo(action_file_selected_path);
                    Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\er.exe").Close();
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\er.exe", true).SetValue("", action_file_selected_path);
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\er.exe", true).SetValue("Path", location_temp + @"\");
                    SuccessMessage("er");
                    return;
                }
                catch (NullReferenceException excep)
                {
                    FileVersionInfo ins = FileVersionInfo.GetVersionInfo(action_file_selected_path);
                   // DialogResult re = MessageBox.Show("Confirm new shortcut:\n\nDetails\n------\nNew Alias:  " + alias + "\n\nFile:  " + action_file_selected_path + "\nDescription:  " + ins.FileDescription + "\nPath:  " + location_temp + "\n\nWould you like to proceed?", "Confirm Settings", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias).Close();
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias, true).SetValue("", action_file_selected_path);
                    Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + alias, true).SetValue("Path", location_temp + @"\");
                    SuccessMessage("er");
                    return;
                }
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }
        private void exploreSafeLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void resetViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
          
        }

        private void button4_Click(object sender, EventArgs e)
        {
           

        }

        private void disableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                if (disableToolStripMenuItem.Checked)
                {


                    new ExplorerIntegration().CreateIntegration(false);
                    disableToolStripMenuItem.Checked = false;
                    enableToolStripMenuItem.Checked = true;

                }


                else
                {




                    new ExplorerIntegration().DeleteIntegration();
                    msg.ShowMessage("Explorer Integration has been disabled");
                    enableToolStripMenuItem.Checked = false;

                    disableToolStripMenuItem.Checked = true;

                }
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }

               
        }

        private void enableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (enableToolStripMenuItem.Checked)
                {


                    new ExplorerIntegration().DeleteIntegration();
                    msg.ShowMessage("Explorer Integration has been disabled");
                    enableToolStripMenuItem.Checked = false;

                    disableToolStripMenuItem.Checked = true;



                }


                else
                {



                    new ExplorerIntegration().CreateIntegration(false);
                    
                    disableToolStripMenuItem.Checked = false;
                    enableToolStripMenuItem.Checked = true;



                }
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }

         
        }




        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void creditsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Credits().ShowDialog();
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
    }
}

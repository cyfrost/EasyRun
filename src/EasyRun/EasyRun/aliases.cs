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
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace EasyRun
{
    public partial class aliases : Form
    {
        private int sortColumn = -1;


        UDialogs msg = new UDialogs();

        public int editDropped = 0;
        public string SAFE_LOCATION_DIRECTORY;
        short DC_ACTION = 1;
        string selecteditemalias = null;
        string selecteditempublisher = null;
        string selecteditemimagepath = null;
        string selecteditemdescription = null;
        string selecteditemregistrykey = null;
        string selecteditemextensiontype = null;
        bool SelectedSomething = false;
        string ALIAS = null;
        string Target = null;
        string Description = null;
        int con_hkcu = 0;
        int unverified_publishers = 0;
        int con_hklm = 0;
        int con = 0;
        string Publisher = null;
        string extension = null;



        public aliases()
        {
            try
            {
                InitializeComponent();
                this.SAFE_LOCATION_DIRECTORY = new Form1().SAFE_LOCATION_DIRECTORY;
            }
            catch (Exception ex)
            {
                msg.ShowError(ex.Message);
            }

          
        }
        
       


        private void ShowPropertiesWindow(string ALIAS, string file, string registrykey) {
            try
            {
                string usingalias = ALIAS;
                string imagepath = file;
                string publisher = selecteditempublisher;
                string productname = null;
                string description = selecteditemdescription;
                string filename = null;
                string productversion = null;
                string fileversion = null;
                string internalname = null;
                string language = null;
                bool ismissing = false;
                bool isreadonly = false;
                string filetype = selecteditemextensiontype;
                string modiftime = null;
                string createtime = null;
                string exectime = null;
                long filesize = 0;
                string direc = null;
                string attribs = null;
                string regkey = registrykey;
                FileInfo abc = new FileInfo(imagepath);
                FileVersionInfo hnd = FileVersionInfo.GetVersionInfo(imagepath);
                modiftime = abc.LastWriteTime.ToString();
                createtime = abc.CreationTime.ToString();
                exectime = abc.LastAccessTime.ToString();
                filesize = abc.Length;
                attribs = abc.Attributes.ToString();
                if (abc.Exists)
                    ismissing = false;
                else
                    ismissing = true;
                isreadonly = abc.IsReadOnly;
                direc = abc.Directory.FullName;
                productname = hnd.ProductName;
                filename = imagepath.Substring(imagepath.LastIndexOf(@"\") + 1);
                productversion = hnd.ProductVersion;
                fileversion = hnd.FileVersion;
                internalname = hnd.InternalName;
                language = hnd.Language;
                props objProps = new props();
                objProps.ShowDetails(usingalias, imagepath, publisher, productname, description, filename, productversion, filetype, modiftime, createtime, exectime, filesize, attribs, regkey, fileversion, language, internalname, ismissing, isreadonly, direc);
                objProps.ShowDialog();
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        

        private void aliases_Load(object sender, EventArgs e)
        {
            this.Text = "Alias Manager";

            textBox2.TextChanged += new EventHandler(textBox2_TextChanged);
            listView1.Enter += new EventHandler(listView1_Enter);
            listView1.KeyDown += new KeyEventHandler(ListviewKeyevents);
            listView1.MouseDoubleClick += new MouseEventHandler(openItemdetails);
            listView1.KeyDown += new KeyEventHandler(ReturnPressed);
            RefreshList();
            textBox2.Focus();
        }
        private void ReturnPressed(object sender, KeyEventArgs e) {
            try
            {
                if (SelectedSomething)
                {
                    if (e.KeyCode == Keys.Enter)
                        ShowPropertiesWindow(selecteditemalias, selecteditemimagepath, selecteditemregistrykey);
                }
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }
        private void openItemdetails(object sender, MouseEventArgs e) {
            if (SelectedSomething) {
                switch (DC_ACTION) {
                    case 1:
                        ShowPropertiesWindow(selecteditemalias, selecteditemimagepath, selecteditemregistrykey);
                        break;
                    case 2:
                        Process.Start(selecteditemimagepath);
                        break;
                    case 3:
                        DelItem();
                        break;
                    case 4:
                        ViewInRegistry();
                        break;
                }
            }
        }
        void ListviewKeyevents(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Delete)
                DelItem();
        }
        void RefreshList() {
            try
            {
                textBox2.Clear();
                con_hkcu = 0;
                con_hklm = 0;
                toolStripStatusLabel1.Text = "Loading...";
                Cursor = Cursors.WaitCursor;
                listView1.Enabled = false;
                textBox2.Enabled = false;
                toolStripButton1.Enabled = false;
                menuStrip1.Enabled = false;
                try
                {
                    listView1.Clear();
                    listView1.View = View.Details;
                    listView1.Columns.Add("Alias");
                    listView1.Columns.Add("Publisher");
                    listView1.Columns.Add("Image Path");
                    listView1.Columns.Add("Description");
                    listView1.Columns.Add("Type");
                    listView1.Columns.Add("Registry");
                    string ALIAS = null;
                    string target = null;
                    string description = null;
                    string hive = null;
                    using (RegistryKey keyh = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths"))
                    {
                        if (keyh != null)
                        {
                            foreach (string keyname in keyh.GetSubKeyNames())
                            {
                                try
                                {
                                    ALIAS = keyname;
                                    RegistryKey item = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\" + keyname);
                                    target = (string)item.GetValue("");
                                    FileVersionInfo h = FileVersionInfo.GetVersionInfo(target);
                                    description = h.FileDescription;
                                    if (String.IsNullOrEmpty(h.CompanyName))
                                        Publisher = "Unknown";
                                    else
                                        Publisher = h.CompanyName;
                                    extension = target.Substring(target.LastIndexOf(".") + 1).ToUpper();
                                    hive = @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\" + ALIAS;
                                    listView1.Items.Add(new ListViewItem(new string[] { ALIAS, Publisher, target, description, extension, hive }));
                                    con_hkcu++;
                                }
                                catch (Exception ex) { continue; }
                            }
                        }
                        else
                            Console.WriteLine("Error: No items under HKCU");
                    }
                    if (listView1.SelectedItems.Count > 0)
                        listView1.SelectedItems.Clear();


                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths"))
                    {
                        if (key != null)
                        {
                            foreach (string keyname in key.GetSubKeyNames())
                            {
                                try
                                {
                                    ALIAS = keyname;
                                    RegistryKey item = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\" + keyname);
                                    target = (string)item.GetValue("");
                                    FileVersionInfo h = FileVersionInfo.GetVersionInfo(target);
                                    description = h.FileDescription;
                                    if (String.IsNullOrEmpty(h.CompanyName))
                                        Publisher = "Unknown";
                                    else
                                        Publisher = h.CompanyName;
                                    hive = @"HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\App Paths\" + ALIAS;
                                    extension = target.Substring(target.LastIndexOf(".") + 1).ToUpper();
                                    listView1.Items.Add(new ListViewItem(new string[] { ALIAS, Publisher, target, description, extension, hive }));
                                    con_hklm++;
                                }
                                catch (Exception ex) { continue; }
                            }
                        }
                        else
                            msg.ShowWarning("No aliases found on this computer");
                    }
                    if (listView1.SelectedItems.Count > 0)
                        listView1.SelectedItems.Clear();

                }
                catch (Exception exp)
                {
                    msg.ShowError(exp.Message);
                }

                con = listView1.Items.Count;
              
                toolStripStatusLabel1.Text = con + " items (Local: " + con_hkcu + " | Machine: " + con_hklm + ")";
                listView1.Columns[0].Width = 130;
                listView1.Columns[1].Width = 135;
                listView1.Columns[2].Width = 350;
                listView1.Columns[3].Width = 200;
                listView1.Columns[4].Width = 50;
                listView1.Columns[5].Width = 600;
                Cursor = Cursors.Default;
                listView1.Enabled = true;
                textBox2.Enabled = true;
                toolStripButton1.Enabled = true;
                menuStrip1.Enabled = true;
                textBox2.Focus();
                if (listView1.SelectedItems.Count > 0)
                    listView1.SelectedItems.Clear();
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
           RefreshList();
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {

           


          

            
            if (textBox2.Text == "" && listView1.Items.Count != con)
                RefreshList();
          

        }
        private void listView1_Enter(object sender, EventArgs e)
        {
           
        }
        private void DelItem() {
            try
            {
                if (listView1.SelectedItems.Count > 0)
                {
                    string ALIAS = null;
                    string target = null;
                    string description = null;
                    string filename = null;
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\" + listView1.SelectedItems[0].Text))
                    {
                        if (key != null)
                        {
                            ALIAS = listView1.SelectedItems[0].Text;
                            target = (string)key.GetValue("");
                            FileVersionInfo ab = FileVersionInfo.GetVersionInfo(target);
                            description = ab.FileDescription;
                            filename = target.Substring(target.LastIndexOf(@"\") + 1);
                            DialogResult re = msg.ConfirmAction("Details\n-------\nAlias: " + ALIAS + "\nFilename: " + filename + "\nDescription: " + description + "\nPath: " + target + "\n\nAre you sure you want to delete this item?");

                            if (re == DialogResult.Yes)
                            {
                                Registry.LocalMachine.DeleteSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\" + listView1.SelectedItems[0].Text, false);
                                // MessageBox.Show("Success!", "EasyRun", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                RefreshList();
                            }
                        }
                        else
                        {
                            using (RegistryKey keyhkcu = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\" + listView1.SelectedItems[0].Text))
                            {
                                if (keyhkcu != null)
                                {
                                    ALIAS = listView1.SelectedItems[0].Text;
                                    target = (string)key.GetValue("");
                                    FileVersionInfo ab = FileVersionInfo.GetVersionInfo(target);
                                    description = ab.FileDescription;
                                    filename = target.Substring(target.LastIndexOf(@"\") + 1);
                                    DialogResult re = msg.ConfirmAction("Details\n-------\nAlias: " + ALIAS + "\nFilename: " + filename + "\nDescription: " + description + "\nPath: " + target + "\n\nAre you sure you want to delete this item");

                                    if (re == DialogResult.Yes)
                                    {
                                        Registry.CurrentUser.DeleteSubKey(@"Software\Microsoft\Windows\CurrentVersion\App Paths\" + listView1.SelectedItems[0].Text, false);
                                        // MessageBox.Show("Success!", "EasyRun", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        RefreshList();
                                    }
                                }
                                else
                                    msg.ShowWarning("Failed to delete item " + ALIAS);

                            }
                        }
                    }
                }
                
            }

            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }

        }
        private void copyAliasNameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedSomething) {
                Clipboard.SetText(ALIAS);
            }
        }
        private void copyTargetPathToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedSomething)
            {
                Clipboard.SetText(Target);
            }
        }
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (!SelectedSomething)
            {
                toolStripMenuItem1.Enabled = false;
                toolStripMenuItem2.Enabled = false;
                toolStripMenuItem3.Enabled = false;
                exportAllItemsToTextFileToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
                openContainingFolderToolStripMenuItem.Enabled = false;
                openInRegistryEditorToolStripMenuItem.Enabled = false;
            }
            else {
                toolStripMenuItem1.Enabled = true;
                toolStripMenuItem2.Enabled = true;
                toolStripMenuItem3.Enabled = true;
                exportAllItemsToTextFileToolStripMenuItem.Enabled = true;
                deleteToolStripMenuItem.Enabled = true;
                openContainingFolderToolStripMenuItem.Enabled = true;
                openInRegistryEditorToolStripMenuItem.Enabled = true;
            }
        }
        private void exportAllItemsToTextFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string contents = "ALIAS\tPublisher\tImage Path\tDescription\tExtension\tHive" + Environment.NewLine + "-----\t---------\t----------\t----------\t---------\t----\n\n";
                StringBuilder sb;
                int i = 1;
                if (listView1.Items.Count > 0)
                {
                    // the actual data
                    foreach (ListViewItem lvi in listView1.Items)
                    {
                        sb = new StringBuilder();
                        foreach (ListViewItem.ListViewSubItem listViewSubItem in lvi.SubItems)
                        {
                            sb.Append(string.Format("{0}\t", listViewSubItem.Text));
                        }
                        contents += i + ". " + sb.ToString() + Environment.NewLine;
                        i++;
                        //sw.WriteLine(sb.ToString());
                    }
                    //sw.WriteLine();
                    File.WriteAllText(SAFE_LOCATION_DIRECTORY + "Export.txt", contents);
                    Process.Start(SAFE_LOCATION_DIRECTORY + "Export.txt");
                }
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }
        private void InvokeAdminProc(string fn)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                try
                {
                    ProcessStartInfo info = new ProcessStartInfo(fn);
                    info.UseShellExecute = true;
                    info.Verb = "runas";
                    Process.Start(info);
                }
                catch (Exception exp)
                {
                    msg.ShowError(exp.Message);
                }
            }
           
        }
        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                SelectedSomething = true;
                selecteditemalias = listView1.SelectedItems[0].SubItems[0].Text;
                selecteditemdescription = listView1.SelectedItems[0].SubItems[3].Text;
                selecteditemextensiontype = listView1.SelectedItems[0].SubItems[4].Text;
                selecteditemimagepath = listView1.SelectedItems[0].SubItems[2].Text;
                selecteditempublisher = listView1.SelectedItems[0].SubItems[1].Text;
                selecteditemregistrykey = listView1.SelectedItems[0].SubItems[5].Text;
                if (listView1.SelectedItems[0].SubItems[4].Text == "EXE")
                {
                    runAsAdministratorToolStripMenuItem.Visible = true;
                    toolStripButton9.Enabled = true;
                }
                else {
                    runAsAdministratorToolStripMenuItem.Visible = false;
                    toolStripButton9.Enabled = false;
                }
            }
            else
            {
                SelectedSomething = false;
            }
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                Process.Start(selecteditemimagepath);
           
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                DelItem();
          
        }
        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0) {
                try
                {
                    string loc = listView1.SelectedItems[0].SubItems[2].Text;
                    loc = loc.Substring(0, loc.LastIndexOf(@"\"));
                    Process.Start(loc);
                }
                catch (Exception exp)
                {
                    msg.ShowError(exp.Message);
                }
            }
           
        }
        private void openInRegistryEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0) {
                try
                {
                    using (RegistryKey mnt = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", true))
                    {
                        mnt.SetValue("LastKey", @"Computer\" + selecteditemregistrykey);
                        Process.Start(@"C:\Windows\regedit.exe");
                    }
                }
                catch (Exception exp)
                {
                    msg.ShowError(exp.Message);
                }
            }
           
        }
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Clipboard.SetText(selecteditemalias);
            }
            
        }
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Clipboard.SetText(selecteditemimagepath);
            }
           
        }
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Clipboard.SetText(selecteditempublisher);
            }
           
        }
        private void descriptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Clipboard.SetText(selecteditemdescription);
            }
            
        }
        private void registryPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                Clipboard.SetText(selecteditemregistrykey);
            }
          
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count > 0)
            {
                ShowPropertiesWindow(selecteditemalias, selecteditemimagepath, selecteditemregistrykey);
            }
          
        }
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                Process.Start(selecteditemimagepath);
            
        }
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
                DelItem();
          
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ShowPropertiesWindow(selecteditemalias, selecteditemimagepath, selecteditemregistrykey);
            }
          
        }
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                string loc = listView1.SelectedItems[0].SubItems[2].Text;
                loc = loc.Substring(0, loc.LastIndexOf(@"\"));
                Process.Start(loc);
            }
          
        }
        private void ViewInRegistry() {
            try
            {
                using (RegistryKey mnt = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", true))
                {
                    mnt.SetValue("LastKey", @"Computer\" + selecteditemregistrykey);
                    Process.Start(@"C:\Windows\regedit.exe");
                }
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ViewInRegistry();
            }
           
        }
        private void EditConfig() {
            try
            {
                editDropped = 0;
                new editConfig(selecteditemalias, selecteditemimagepath, selecteditemregistrykey, this).ShowDialog();

                if (editDropped !=1)
                    RefreshList();

            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }


     
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                EditConfig();
            }
         
        }
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            RefreshList();
        }
        private void runAsAdministratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InvokeAdminProc(selecteditemimagepath);
        }
        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            InvokeAdminProc(selecteditemimagepath);
        }
        private void toolStripButton10_Click(object sender, EventArgs e)
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
                AlObj.Show();
                AlObj.BringToFront();
            }
        }
        private void openToHKCUAppPathsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (RegistryKey mnt = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", true))
                {
                    mnt.SetValue("LastKey", @"Computer\HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\App Paths\");
                    Process.Start(@"C:\Windows\regedit.exe");
                }
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }
        private void openToHKLMNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (RegistryKey mnt = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", true))
                {
                    mnt.SetValue("LastKey", @"Computer\HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\CurrentVersion\App Paths\");
                    Process.Start(@"C:\Windows\regedit.exe");
                }
            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }
        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            RefreshList();
        }
        private void showPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showPropertiesToolStripMenuItem.Checked = true;
            runApplicationToolStripMenuItem.Checked = false;
            deleteEntryToolStripMenuItem.Checked = false;
            viewInRegistryEditorToolStripMenuItem.Checked = false;
            DC_ACTION = 1;
        }
        private void runApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DC_ACTION = 2;
            showPropertiesToolStripMenuItem.Checked = false;
            runApplicationToolStripMenuItem.Checked = true;
            deleteEntryToolStripMenuItem.Checked = false;
            viewInRegistryEditorToolStripMenuItem.Checked = false;
        }
        private void deleteEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DC_ACTION = 3;
            showPropertiesToolStripMenuItem.Checked = false;
            runApplicationToolStripMenuItem.Checked = false;
            deleteEntryToolStripMenuItem.Checked = true;
            viewInRegistryEditorToolStripMenuItem.Checked = false;
        }
        private void viewInRegistryEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DC_ACTION = 4;
            showPropertiesToolStripMenuItem.Checked = false;
            runApplicationToolStripMenuItem.Checked = false;
            deleteEntryToolStripMenuItem.Checked = false;
            viewInRegistryEditorToolStripMenuItem.Checked = true;
        }
        private void showGridLinesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showGridLinesToolStripMenuItem.Checked == true)
            {
                showGridLinesToolStripMenuItem.Checked = false;
                listView1.GridLines = false;
            }
            else {
                showGridLinesToolStripMenuItem.Checked = true;
                listView1.GridLines = true;
            }
        }
        private void autoSizeColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }
        private void showToolTipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (showToolTipsToolStripMenuItem.Checked == true)
            {
                showToolTipsToolStripMenuItem.Checked = false;
                listView1.ShowItemToolTips = false;
            }
            else
            {
                showToolTipsToolStripMenuItem.Checked = true;
                listView1.ShowItemToolTips = true;
            }
        }
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form AlObj = Application.OpenForms["about"];
            if (AlObj == null)
            {
                about a = new about();
                a.ShowDialog();
                a.BringToFront();
            }
            else
            {
                AlObj.ShowDialog();
                AlObj.BringToFront();
            }
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                EditConfig();

            }
           
        }

        public void StartSearch() {
            try
            {
                if (textBox2.Text != "")
                {
                    
                   

                    for (int i = listView1.Items.Count - 1; i >= 0; i--)
                    {
                        
                        var item = listView1.Items[i];
                        string searchStr = textBox2.Text.ToLower();

                        if (item.Text.ToLower().Contains(searchStr) ||  item.SubItems[2].Text.ToLower().Contains(searchStr))
                        {
                            item.BackColor = Color.Yellow;

                        }
                        else
                        {
                            listView1.Items.Remove(item);
                        }
                    }
                    if (listView1.SelectedItems.Count == 1)
                    {
                        listView1.Focus();
                    }
                    toolStripStatusLabel1.Text = listView1.Items.Count + " search results";

                }


            }
            catch (Exception exp)
            {
                msg.ShowError(exp.Message);
            }
        }


        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
               StartSearch();
            }
           
            
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != sortColumn)
            {
                // Set the sort column to the new column.
                sortColumn = e.Column;
                // Set the sort order to ascending by default.
                listView1.Sorting = SortOrder.Ascending;
            }
            else
            {
                // Determine what the last sort order was and change it.
                if (listView1.Sorting == SortOrder.Ascending)
                    listView1.Sorting = SortOrder.Descending;
                else
                    listView1.Sorting = SortOrder.Ascending;
            }

            // Call the sort method to manually sort.
            listView1.Sort();
            // Set the ListViewItemSorter property to a new ListViewItemComparer
            // object.
            this.listView1.ListViewItemSorter = new ListViewItemComparer(e.Column,
                                                              listView1.Sorting);
        }
    }
}

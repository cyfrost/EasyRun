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
namespace EasyRun
{
    public partial class props : Form
    {
        public void ShowDetails(string usingalias, string imagepath, string publisher, string productname, string description, string filename, string version, string filetype, string modiftime, string createtime, string exectime, long filesize, string attribs, string registrykey, string fileversion, string language, string internalname, bool ismissing, bool isreadonly, string directory) {
            textBox1.Text = usingalias;
            textBox2.Text = imagepath;
            textBox3.Text = publisher;
            textBox4.Text = productname;
            textBox5.Text = description;
            textBox6.Text = filename;
            textBox7.Text = version;
            textBox8.Text = filetype.ToUpper();
            textBox9.Text = filesize + " bytes";
            textBox10.Text = modiftime;
            textBox11.Text = createtime;
            textBox12.Text = exectime;
            textBox13.Text = attribs;
            textBox14.Text = registrykey;
            textBox15.Text = fileversion;
            textBox16.Text = internalname;
            textBox17.Text = language;
            textBox19.Text = "" + ismissing;
            textBox18.Text = "" + isreadonly;
            textBox20.Text = directory;
        }
        public props()
        {
            InitializeComponent();
        
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void props_Load(object sender, EventArgs e)
        {
            label1.Select();
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
        private void button1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox15_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

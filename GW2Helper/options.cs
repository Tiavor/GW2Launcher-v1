using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

/*
 * The MIT License (MIT)
 * 
 * Copyright © 2015, Tiavor
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
namespace GW2Helper
{
    public partial class options : Form
    {

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        internal Form1 thatParentForm { get; set; }
        private Label[] listMail, listName, listPW;
        private TextBox[] listMailbox, listNamebox, listPWbox;

        public options()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            listMail = new Label[] { labelMail0, labelMail1, labelMail2, labelMail3, labelMail4,
                                     labelMail5, labelMail6, labelMail7, labelMail8, labelMail9};
            listName = new Label[] { labelName0, labelName1, labelName2, labelName3, labelName4,
                                     labelName5, labelName6, labelName7, labelName8, labelName9};
            listPW = new Label[] { labelPW0, labelPW1, labelPW2, labelPW3, labelPW4,
                                     labelPW5, labelPW6, labelPW7, labelPW8, labelPW9};
            listMailbox = new TextBox[] { textBoxmail0, textBoxmail1, textBoxmail2, textBoxmail3, textBoxmail4,
                                          textBoxmail5, textBoxmail6, textBoxmail7, textBoxmail8, textBoxmail9};
            listNamebox = new TextBox[] { textBoxname0, textBoxname1, textBoxname2, textBoxname3, textBoxname4,
                                          textBoxname5, textBoxname6, textBoxname7, textBoxname8, textBoxname9};
            listPWbox = new TextBox[] { textBoxPw0, textBoxPw1, textBoxPw2, textBoxPw3, textBoxPw4,
                                        textBoxPw5, textBoxPw6, textBoxPw7, textBoxPw8, textBoxPw9};
            toolTip1.SetToolTip(textBoxCMD, "add additional command line arguments here like -maploadinfo, seperated by a space\n -nopatchui is already included, just not listed here");
            toolTip1.SetToolTip(textBoxname0, "Could be anything e.g.the account name'name.1234' or simply an enumeration");
            toolTip1.SetToolTip(textBoxPw0, "Fill in the pw as last of the 3 fields!\nPress tab to save.\nField length will get reseted after saving, don't worry");
            toolTip1.SetToolTip(buttonPlus, "Add an account to the list\n10 is max");
        }
        internal void setEntry(int id, string mail, string name)
        {
            listMailbox[id].Text = mail;
            if (name == null || name.Length < 2)
                listNamebox[id].Text = "";
            else
                listNamebox[id].Text = name;
            if (mail.Length > 3)
                listPWbox[id].Text = "*****";
        }
        internal void setPath(string path)
        {
            textBoxPath.Text = path;
        }
        internal void setCmd(string cmd)
        {
            textBoxCMD.Text = cmd;
        }
        internal void setVisibleTo(int id)
        {
            int i = 0;
            for (i = 0; i < id; i++)
            {
                listMail[i].Visible = true;
                listMailbox[i].Visible = true;
                listName[i].Visible = true;
                listNamebox[i].Visible = true;
                listPW[i].Visible = true;
                listPWbox[i].Visible = true;
            }
            if (i > 5)
            {
                Height = 426;
            }
            else
            {
                if (i > 0)
                    Height = 426 - (5 - i) * 70;
                else
                    Height = 87;
            }
            while (i < 10)
            {
                listMail[i].Visible = false;
                listMailbox[i].Visible = false;
                listName[i].Visible = false;
                listNamebox[i].Visible = false;
                listPW[i].Visible = false;
                listPWbox[i].Visible = false;
                i++;
            }
        }


        private int getID(object sender)
        {
            TextBox box = sender as TextBox;
            int id = Convert.ToInt32(box.AccessibleName);
            if (id >= 0 && id <= 100)
                return id;
            else
                return 0;
        }
        //button click events
        private void buttonPlus_Click(object sender, EventArgs e)
        {
            thatParentForm.addAccount();
        }

        private void buttonMinus_Click(object sender, EventArgs e)
        {
            thatParentForm.remAccount();
        }

        private void textBox1pw_Leave(object sender, EventArgs e)
        {
            int i = getID(sender);
            thatParentForm.configPut("pw" + i.ToString(),
                StringCipher.Encrypt(listPWbox[i].Text,
                    listNamebox[i].Text +
                    listMailbox[i].Text));
            listPWbox[i].Text = "*****";
        }

        private void textBoxmail0_Leave(object sender, EventArgs e)
        {
            int i = getID(sender);
            thatParentForm.configPut("mail" + i.ToString(), listMailbox[i].Text);
        }

        private void textBoxname0_Leave(object sender, EventArgs e)
        {
            int i = getID(sender);
            thatParentForm.configPut("account" + i.ToString(), listNamebox[i].Text);
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog searchDialog = new OpenFileDialog();
            searchDialog.Filter = "Executeables(*.exe,*.bat)|*.exe;*.bat";
            System.Windows.Forms.DialogResult dr = searchDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                textBoxPath.Text = searchDialog.FileName;
                thatParentForm.configPut("path", searchDialog.FileName);
            }
        }

        private void textBoxPath_Leave(object sender, EventArgs e)
        {
            thatParentForm.configPut("path", textBoxPath.Text);
        }

        private void textBoxCMD_Leave(object sender, EventArgs e)
        {
            thatParentForm.configPut("cmd", textBoxCMD.Text);
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Collections.Specialized;
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
    public partial class Form1 : Form
    {
        private options fO = new options();
        int oldX = 0, oldY = 0;
        Label[] lastloginlabels, accountlabels;
        Button[] startButtons, setButtons;
        private info fI = new info();

        public Form1()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            // ID check, when the location of the exe or the user/pc changed, delete config
            // the ID is also part of the encrypting
            string ID = StringCipher.ByteArrToHex(StringCipher.generateKey(Environment.UserName + Environment.MachineName +
                System.Reflection.Assembly.GetExecutingAssembly().CodeBase));
            if (ID != ConfigGet("ID"))
            {
                configReset();
                configPut("ID", ID);
            }
            // list all labels and buttons for easier access while coding
            lastloginlabels = new Label[] { labelLastlogin0, labelLastlogin1, labelLastlogin2, labelLastlogin3, labelLastlogin4,
                                            labelLastlogin5, labelLastlogin6, labelLastlogin7, labelLastlogin8, labelLastlogin9};
            accountlabels = new Label[] { labelAccname0, labelAccname1, labelAccname2, labelAccname3, labelAccname4 ,
                                          labelAccname5, labelAccname6, labelAccname7, labelAccname8, labelAccname9};
            startButtons = new Button[] { button_startacc0, button_startacc1, button_startacc2, button_startacc3, button_startacc4,
                                          button_startacc5, button_startacc6, button_startacc7, button_startacc8, button_startacc9};
            setButtons = new Button[] { buttonSetLL0, buttonSetLL1, buttonSetLL2, buttonSetLL3, buttonSetLL4,
                                        buttonSetLL5, buttonSetLL6, buttonSetLL7, buttonSetLL8, buttonSetLL9};
            toolTip1.SetToolTip(setButtons[0], "Sets the last login to now without starting GW2");
            toolTip1.SetToolTip(buttonOptionen, "Beware: if not setup correctly, gw2 will remain white on startup.\n But it may also take a little longer as usual. (10s ony my system till char select)");
            // adding a reference of this form to the options form for accessing functions etc
            fO.thatParentForm = this;
            //loading all variables into labels and textboxes
            configLoad();
        }
        //imports for moving the form without the top bar
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();

        // setup the process and start
        private void startGW(int index)
        {
            //start gw if gw is not running and index is ok 
            Process p = Process.GetProcessesByName("gw2").FirstOrDefault();
            if (p == null && index < Convert.ToInt32(ConfigGet("numberofacc")))
            {
                //set laststarted to "today"
                ConfigSetLastlogin(index);
                //start gw
                string path = ConfigGet("path");
                Process gw2pro = new Process();
                gw2pro.StartInfo.FileName = path; //insert file and path from config
                gw2pro.StartInfo.Arguments = "-email " + getMail(index) + " -password " + getPW(index) + " -nopatchui";
                if (ConfigGet("cmd") != "")
                    gw2pro.StartInfo.Arguments += " " + ConfigGet("cmd");
                gw2pro.StartInfo.WorkingDirectory = path.Substring(0, path.LastIndexOf("\\")+1);
                MessageBox.Show(gw2pro.StartInfo.FileName + "\n" +
                                gw2pro.StartInfo.WorkingDirectory);
                gw2pro.Start();
            }
        }
        /* //could make a different path for each account
        private string getFilename(int index)
        {
            if (index < Convert.ToInt32(ConfigGet("numberofacc")))
                return ConfigGet("exe"+index.ToString());
            return null;
        }*/
        private string getMail(int index)
        {
            if (index < Convert.ToInt32(ConfigGet("numberofacc")))
                return ConfigGet("mail" + index.ToString());
            return null;
        }
        private String getAccountname(int index)
        {
            if (index < Convert.ToInt32(ConfigGet("numberofacc")))
                return ConfigGet("account" + index.ToString());
            return null;
        }
        // most important function ever
        private string getPW(int index)
        {
            string pwEncrypted = ConfigGet("pw" + index.ToString());
            string account = ConfigGet("account" + index.ToString()) +
                             ConfigGet("mail" + index.ToString());
            return StringCipher.Decrypt(pwEncrypted, account);
        }
        // get ID from accessibleName, all accessible names used in this program are plain numbers
        // used to access all buttons or textboxes with only one fuction assigned
        // the ID is used as index in the lists of all labels/boxes
        private int getID(object sender)
        {
            Button button = sender as Button;
            int id = Convert.ToInt32(button.AccessibleName);
            if (id >= 0 && id <= 100)
                return id;
            else
                return 0;
        }
        private void setLastlogin(int ID, DateTime date)
        {
            DateTime now = DateTime.Now.ToUniversalTime();
            DateTime dateUTC = date.ToUniversalTime();
            if (dateUTC.Year == now.Year && dateUTC.Month == now.Month && dateUTC.Day == now.Day)//&&(date.Hour>=0||date.Minute>55))
                lastloginlabels[ID].Text = "today";
            else
            {
                if (date.Year == now.Year)
                    lastloginlabels[ID].Text = date.Day.ToString() +
                                       "." + date.Month.ToString() +
                                       " " + date.Hour.ToString() +
                                       ":" + date.Minute.ToString();
                else
                    lastloginlabels[ID].Text = date.Day.ToString() +
                                           "." + date.Month.ToString() +
                                           "." + date.Year.ToString() +
                                           " " + date.Hour.ToString() +
                                           ":" + date.Minute.ToString();
            }
        }
        private void setLastlogin(int ID, string date)
        {
            if (date == null || date.Length < 10)
            {
                lastloginlabels[ID].Text = "";
                return;
            }
            setLastlogin(ID, Convert.ToDateTime(date));
        }
        //sets textboxes and labels visible depending on the ammount of accounts
        private void checkAmmount()
        {
            int i = 0;
            for (i = 0; i < Convert.ToInt32(ConfigGet("numberofacc")); i++)
            {
                lastloginlabels[i].Visible = true;
                accountlabels[i].Visible = true;
                startButtons[i].Visible = true;
                setButtons[i].Visible = true;
            }
            Height = 359 - (10 - i) * 29;
            Height = (int)(Height * AutoScaleFactor.Height);
            fO.setVisibleTo(i);
            if (i < 10)
            {
                while (i < 10)
                {
                    lastloginlabels[i].Visible = false;
                    accountlabels[i].Visible = false;
                    startButtons[i].Visible = false;
                    setButtons[i].Visible = false;
                    i++;
                }
            }
        }
        //////////////////////
        // App.config methods
        /*
        <add key="ID" value="0"/>
        <add key="path" value=""/>
        <add key="cmd" value="-maploadinfo -StreamingClient"/>
        <add key="numberofacc" value="1" />
        <add key="mail0" value="0" />
        <add key="account0" value="0" />
        <add key="pw0" value="0" />
        <add key="lastlogin0" value="0" />
         */
        private void configLoad()
        {
            checkAmmount();
            for (int i = 0; i < Convert.ToInt32(ConfigGet("numberofacc")) && i < 10; i++)
            {
                fO.setEntry(i,
                    ConfigGet("mail" + i.ToString()),
                    ConfigGet("account" + i.ToString()));
                setLastlogin(i, ConfigGet("lastlogin" + i.ToString()));
                accountlabels[i].Text = ConfigGet("account" + i.ToString());
            }
            fO.setPath(ConfigGet("path"));
            fO.setCmd(ConfigGet("cmd"));
        }
        private void configReset()
        {
            NameValueCollection sAll;
            sAll = ConfigurationManager.AppSettings;

            foreach (string s in sAll.AllKeys)
            {
                if (s != "cmd")
                {
                    if (s.Contains("pw") || s.Contains("number") || s == "ID" || s.Contains("last"))
                    {
                        configPut(s, "0");
                    }
                    else
                    {
                        configPut(s, "");
                    }
                }
            }
            configPut("numberofacc", "1");
            configPut("account0", "acc #1");
            configPut("mail0", "your.account@mail.com");
            configPut("lastlogin0", "03.01.2015 04:15:00");
        }

        internal void configPut(string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, value);
            ConfigurationManager.AppSettings.Set(key, value);
            config.Save(ConfigurationSaveMode.Minimal);
        }
        private string ConfigGet(string key)
        {
            string ret = "";
            try
            {
                ret = ConfigurationManager.AppSettings.Get(key);
            }
            catch (Exception e)
            {
                return "";
            }
            if (ret == null)
                return "";
            else
                return ret;
        }
        private void ConfigSetLastlogin(int ID)
        {
            string date = System.DateTime.Now.ToString();
            configPut("lastlogin" + ID.ToString(), date);
            setLastlogin(ID, System.DateTime.Now);
        }
        internal void addAccount()
        {
            int n = Convert.ToInt32(ConfigGet("numberofacc"));
            if (n < 10)
            {
                n++;
                configPut("numberofacc", n.ToString());
                checkAmmount();
            }
        }
        internal void remAccount()
        {
            int n = Convert.ToInt32(ConfigGet("numberofacc"));
            if (n > 0)
            {
                n--;
                configPut("numberofacc", n.ToString());
                checkAmmount();
            }
        }
        //////////////////////
        //button click events
        private void buttonOptionen_Click(object sender, EventArgs e)
        {
            if (fO.Visible)
            {
                fO.Hide();
            }
            else
            {
                fO.Show();
                fO.Top = this.Top;
                fO.Left = this.Left + this.Width;
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button_startacc1_Click(object sender, EventArgs e)
        {
            int i = getID(sender);
            if (getAccountname(i) != "" && getMail(i) != "")
                startGW(i);
        }
        private void buttonSetacc_Click(object sender, EventArgs e)
        {
            int i = getID(sender);
            if (getAccountname(i) != "" && getMail(i) != "")
                ConfigSetLastlogin(i);
        }

        /////////////////
        // window events
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                if (fO.Visible)
                {
                    fO.Focus();
                    this.Focus();
                    fO.Left = this.Left + this.Width;
                    fO.Top = this.Top;
                }
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (Opacity < 0.8)
                if (e.X < 5 || e.Y < 5 || e.X > Size.Width - 5 || e.Y > Size.Height - 5)
                {
                    Opacity = 0.3;
                }
                else
                {
                    Opacity = 0.7;
                }
            if (fO.Visible)
                Opacity = 1;
            oldX = e.X;
            oldY = e.Y;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            if (!fO.Visible)
                Opacity = 0.3;
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            Opacity = 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.MinimizeBox = true;
        }

        private void buttonInfo_Click(object sender, EventArgs e)
        {
            // if the form is not closed, show it
            if (fI == null || fI.IsDisposed)
            {
                fI = new info();

                // attach the handler
                fI.FormClosed += InfoFormClosed;
            }

            // show it
            fI.Show();
        }
        void InfoFormClosed(object sender, FormClosedEventArgs args)
        {
            // detach the handler
            fI.FormClosed -= InfoFormClosed;

            // let GC collect it (and this way we can tell if it's closed)
            fI = null;
        }

    }
}

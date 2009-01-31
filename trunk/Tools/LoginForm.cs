using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace EasyTravian
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            comboBox1.Items.AddRange(GetServers());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Globals.Cfg.Server = comboBox1.Text;
            Globals.Cfg.UserName = comboBox2.Text;
            Globals.Cfg.PassWord = textBox1.Text;

            DialogResult = DialogResult.OK;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            comboBox1.Text = Globals.Cfg.Server;
            comboBox2.Text = Globals.Cfg.UserName;
            //textBox1.Text = Globals.Cfg.PassWord;
            Globals.Translator.TranslateForm(this);
        }

        private string[] GetServers()
        {
            List<string> ret = new List<string>();
            try
            {
                foreach (DirectoryInfo dirinf in Config.DataBaseDir.GetDirectories())
                {
                    ret.Add(dirinf.ToString());
                }
            }
            catch { }

            return ret.ToArray();
        }

        private string[] GetUserNames(string server)
        {
            List<string> ret = new List<string>();
            try
            {
                DirectoryInfo di = new DirectoryInfo(Config.DataBaseDir.FullName + server );
                foreach (DirectoryInfo dirinf in di.GetDirectories() )
                {
                    ret.Add(dirinf.ToString());
                }
            }
            catch { }

            return ret.ToArray();

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();
            comboBox2.Items.AddRange(GetUserNames(comboBox1.Text));
            if (comboBox2.Items.Count > 0)
                comboBox2.Text = comboBox2.Items[0].ToString();
            textBox1.Text = "";
            textBox1.Focus();
        }
    }
}

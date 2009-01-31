using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;

namespace EasyTravian
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            Globals.Translator.TranslateForm(this);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Globals.Cfg.Language = (int)cbLang.SelectedValue;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Globals.Cfg.Language = (int)cbLang.SelectedValue;

            using (TranslatorForm f = new TranslatorForm())
                f.ShowDialog();
        }

        private void SettingsForm_Activated(object sender, EventArgs e)
        {
            cbLang.DataSource = Globals.Translator.GetLanguages();

            //cbLang.Items.AddRange(Globals.Translator.GetLanguages());
            cbLang.DisplayMember = "NativeName";
            cbLang.ValueMember = "LCID";
            //cbLang.Text = Globals.Cfg.Language;
            cbLang.SelectedValue = Globals.Cfg.Language;
        }

        private void cbLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*
            if (cbLang.SelectedItem != null)
                Text = ((CultureInfo)cbLang.SelectedItem).LCID.ToString();
             */ 

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

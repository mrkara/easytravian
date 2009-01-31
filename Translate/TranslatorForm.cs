using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Net.Mail;

namespace EasyTravian
{
    public partial class TranslatorForm : Form
    {
        List<TransItem> items = new List<TransItem>();

        public TranslatorForm()
        {
            InitializeComponent();
            items.Clear();
            items.AddRange(Globals.Translator.Items);
            //items = Globals.Translator.Items;
        }

        private void TranslatorForm_Load(object sender, EventArgs e)
        {
            bindingSource1.DataSource = items;
            textBox1.DataBindings.Add("Text", bindingSource1, "Key");
            textBox2.DataBindings.Add("Text", bindingSource1, "Value");
            //toolStripTextBox1.Text = Globals.Cfg.Language;
            Globals.Translator.TranslateForm(this);


            cbCultures.ComboBox.DataSource = CultureInfo.GetCultures(CultureTypes.InstalledWin32Cultures);
            cbCultures.ComboBox.DisplayMember = "NativeName";
            cbCultures.ComboBox.ValueMember = "LCID";
            cbCultures.SelectedIndexChanged += new System.EventHandler(this.cbCultures_SelectedIndexChanged);


            cbCultures.ComboBox.SelectedValue = Globals.Cfg.Language;

        }

        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dataGridView1.Columns.Clear();

            DataGridViewTextBoxColumn col;

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Key";
            col.HeaderText = Globals.Translator["Original"];
            col.ReadOnly = true;
            dataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Value";
            col.HeaderText = Globals.Translator["Translated"];
            dataGridView1.Columns.Add(col);


            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //Globals.Cfg.Language = toolStripTextBox1.Text;
            //Globals.Translator.Save();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Globals.Cfg.Language = (int)cbCultures.ComboBox.SelectedValue;
            Globals.Translator.Items = items;
            Globals.Translator.Save();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void cbCultures_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Globals.Translator.LangExists((CultureInfo)cbCultures.SelectedItem))
            {
                Globals.Cfg.Language = ((CultureInfo)cbCultures.ComboBox.SelectedItem).LCID;
                Globals.Translator.Load();
                items.Clear();
                items.AddRange(Globals.Translator.Items);
                bindingSource1.ResetBindings(false);

            }
            //CultureInfo ci = CultureInfo.GetCultures(CultureTypes.FrameworkCultures)[cbCultures.SelectedIndex]; 

            //if (File.Exists(
        }

        private void cbCultures_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("Please attach the file \"" + Globals.Translator.GetFileName(Globals.Cfg.Language) + "\"!");

            string s = "mailto:translate@easytravian.net?subject=translation&body=see attachment&attachments=\"" + Globals.Translator.GetFileName(Globals.Cfg.Language) + "\"";
            System.Diagnostics.Process.Start( s );

            /*
            Globals.Cfg.Language = (int)cbCultures.ComboBox.SelectedValue;
            Globals.Translator.Items = items;
            Globals.Translator.Save();

            MailMessage msg = new MailMessage();
            msg.From = new MailAddress("en@en.hu");
            msg.To.Add( new MailAddress( "translate@easytravian.net" ));
            msg.Attachments.Add(new Attachment(Globals.Translator.GetFileName(Globals.Cfg.Language)));

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "localhost";
            smtp.Send(msg);
             */ 
        }
    }
}

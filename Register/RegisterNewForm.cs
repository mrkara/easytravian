using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EasyTravian
{
    public partial class RegisterNewForm : Form
    {
        public RegistrationItem item = new RegistrationItem();

        public RegisterNewForm()
        {
            InitializeComponent();
            comboBox1.Items.Clear();
            foreach (int i in Enum.GetValues(typeof(TraviModule)))
            {
                comboBox1.Items.Add(Globals.Translator[((TraviModule)i).ToString()]);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void DataCanged()
        {
            textBox3.Text = Globals.Register.GenerateKey( (TraviModule)comboBox1.SelectedIndex );
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            DataCanged();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (Globals.Register.IsValidKeyCodePair(textBox3.Text, textBox4.Text))
            {
                MessageBox.Show(Globals.Translator["Registered succesfully!"]);

                RegistrationItem it = new RegistrationItem();
                it.Module = comboBox1.SelectedIndex;
                it.Key = textBox3.Text;
                it.Code = textBox4.Text;
                it.Entered = DateTime.Now;

                Globals.Register.NewItem(it);

                Close();
            }
            else
            {
                MessageBox.Show(Globals.Translator["Not a valid code for this key!"]);
            }

        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataCanged();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.Clear();
            Clipboard.SetText(textBox3.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox4.Text = Clipboard.GetText();
        }

        private void RegisterNewForm_Load(object sender, EventArgs e)
        {
            Globals.Translator.TranslateForm(this);
        }
    }
}

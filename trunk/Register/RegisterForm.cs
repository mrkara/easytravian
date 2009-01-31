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
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
            bindingSource1.DataSource = Globals.Register.Items;

            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Entered";
            col.HeaderText = Globals.Translator["Entered"];
            dataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ModuleName";
            col.HeaderText = Globals.Translator["Module"];
            dataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Key";
            col.HeaderText = Globals.Translator["Key"];
            dataGridView1.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Code";
            col.HeaderText = Globals.Translator["Code"];
            dataGridView1.Columns.Add(col);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            RegisterNewForm f = new RegisterNewForm();
            f.ShowDialog(this);
            bindingSource1.ResetBindings(false);
        }

        private void bindingSource1_ListChanged(object sender, ListChangedEventArgs e)
        {
            bDelete.Enabled = bindingSource1.Count > 0;
        }

        private void bDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Globals.Translator["Are you sure?"], Globals.Translator["Warning!"], MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Globals.Register.DeleteItem((RegistrationItem)bindingSource1.Current);
                bindingSource1.ResetBindings(false);
            }
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {
            Globals.Translator.TranslateForm(this);
        }
    }
}

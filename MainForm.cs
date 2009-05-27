using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EasyTravian.Properties;
using System.Deployment.Application;
using System.Diagnostics;



namespace EasyTravian
{
    public partial class MainForm : Form
    {
        TraviController controller = null;
        MapPainterProps mapPainterProps = new MapPainterProps();
        //List<MapElement> map = null;

        bool FirstActivate = true;
        ToolTip MapToolTip = new ToolTip();
        DateTime AutobuildStarted;

        public MainForm()
        {
            InitializeComponent();

            Globals.Web = webBrowser;
            controller = new TraviController();

            MapToolTip.ShowAlways = true;
            MapToolTip.ToolTipTitle = Globals.Translator["Information"];

            Text = Application.ProductName;

            if (ApplicationDeployment.IsNetworkDeployed)
                Text += ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();


            if (!Globals.Cfg.Debugging)

                while (tcMain.TabPages.Count > 2)
                {
                    tcMain.TabPages.RemoveAt(2);
                }

        }

        private void LoadAll()
        {
            LoadVillages();
            gridResources.DataSource = controller.bsResources;
            gridProduction.DataSource = controller.bsProductions;
            gridBuildings.DataSource = controller.bsBuildings;
            gridResourceOveralls.DataSource = controller.bsResourceOverall;
            gridConstructions.DataSource = controller.bsConstruction;
            gridCanBuild.DataSource = controller.bsCanBuild;

            controller.CheckTrials();
        }


        private void LoadVillages()
        {
            string[] vs = controller.GetVillages();
            bVillages.DropDownItems.Clear();
            foreach (string v in vs)
                bVillages.DropDownItems.Add(v);
            if (bVillages.DropDownItems.Count > 0)
                SelectVillage(bVillages.DropDownItems[0].Text);
        }

        private void bVillages_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            SelectVillage( e.ClickedItem.Text );
        }

        private void SelectVillage( string VillageName )
        {
            bVillages.Text = VillageName;
            controller.SetActiveVillageByName(VillageName);
        }

        private void toolStripSplitButton1_ButtonClick(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            controller.SaveData();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // tcMain.TabIndex = 1;
            tcMain.SelectTab(1);

            Globals.Translator.TranslateForm(this);
        }

        private void tmrAutoBuild_Tick(object sender, EventArgs e)
        {
            //controller.Refresh();
            //LoadAll();
            tmrAutoBuild.Enabled = false;
            try
            {
                try
                {
                    controller.Build();
                }
                catch (Exception ex)
                {
                    //throw;
                    Globals.Logger.Log(ex.Message, LogType.ltDebug);
                }

            }
            finally
            {
                if (!Globals.Register.IsRegistered(TraviModule.Builder)
                    &&
                    DateTime.Now.Subtract(AutobuildStarted).Hours > 0)
                    bAutoBuild.Checked = false;
                else
                    tmrAutoBuild.Enabled = true;

            }

        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
        }

        private void pnlMap_Paint(object sender, PaintEventArgs e)
        {
            mapPainterProps.zoom = trackBar1.Value;

            controller.DrawActMap(e.Graphics, new Point(1000, 1000), mapPainterProps);
            //e.Graphics.DrawEllipse(new Pen(Color.Red), new Rectangle(10, 10, 100, 100));
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            RedrawMap();
        }

        private void pnlMap_MouseMove(object sender, MouseEventArgs e)
        {
            MapElement me = controller.GetMapInfo(((Panel)sender).Bounds.Size, e.Location, trackBar1.Value);
            if (me != null)
            {
                string s = "";
                s += Globals.Translator["Village"] + ':' + me.Village + '\n';
                s += Globals.Translator["Player"] + ':' + me.Player + '\n';
                s += Globals.Translator["Alliance"] + ':' + me.Alliance + '\n';
                s += Globals.Translator["Population"] + ':' + me.Population + '\n';
                s += Globals.Translator["Tribe"] + ':' + Globals.Translator[((TribeType)me.Tid - 1).ToString()];
                if (me.Terrain != null)
                    s += '\n' + Globals.Translator["Terrain"] + ':' + Globals.Translator[me.Terrain];
                MapToolTip.SetToolTip(pnlMap, s);
            }
           
        }

        private void pnlMap_MouseClick(object sender, MouseEventArgs e)
        {
            controller.MapClicked(((Panel)sender).Bounds.Size, e.Location, trackBar1.Value);
            RedrawMap();
        }

        private void RedrawMap()
        {
            pnlMap.Invalidate();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Login()
        {
            using (LoginForm f = new LoginForm())
            {
                bool succlogin = false;
                while (!succlogin)
                {
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        if (controller.Login())
                        {
                            succlogin = true;
                            LoadAll();
                        }
                    }
                    else
                    {
                        succlogin = true;
                        Close();
                    }
                }
            }

            Text = Globals.Cfg.UserName + '@' + Globals.Cfg.Server + ' ' + Text;

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
        }

        private void MapPainAllies()
        {
            if (controller.ActiveClans() != null)
            {
                cbMapAllies.Items.AddRange(controller.ActiveClans().ToArray());
            }
            mapPainterProps.Coloring = MapColoring.Ally;
        }

        private void tcMapProps_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tcMapProps.SelectedIndex)
            {
                case 0: //tribe
                    mapPainterProps.Coloring = MapColoring.Tribe;
                    break;
                case 1: //ally
                    MapPainAllies();
                    break;
                case 2: //popu
                    mapPainterProps.Coloring = MapColoring.Population;
                    break;
                default:
                    break;
            }
            RedrawMap();
        }

        private void bMapAlliesColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                if (cd.ShowDialog() == DialogResult.OK)
                    ((Button)sender).BackColor = cd.Color;
            }
        }

        private void bMapAlliesAdd_Click(object sender, EventArgs e)
        {
            if (cbMapAllies.Text != "")
            {
                mapPainterProps.Alliances[cbMapAllies.Text] = bMapAlliesColor.BackColor;
                DrawMapAlliesList();
                RedrawMap();
            }
        }

        private void DrawMapAlliesList()
        {
            lvMapAllies.Items.Clear();
            foreach (KeyValuePair<string, Color> item in mapPainterProps.Alliances)
            {
                ListViewItem li = new ListViewItem(item.Key);
                li.BackColor = item.Value;
                lvMapAllies.Items.Add(li);
            }
        }

        private void lvMapAllies_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void bMapAlliesRemove_Click(object sender, EventArgs e)
        {
            if (lvMapAllies.SelectedItems.Count > 0)
            {
                mapPainterProps.Alliances.Remove(lvMapAllies.SelectedItems[0].Text);
                DrawMapAlliesList();
                RedrawMap();
            }
        }

        private void gridResources_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            InitGridColumns();
        }

        private void InitGridColumns()
        {
            gridResources.Columns.Clear();

            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Name";
            col.HeaderText = Globals.Translator["Name"];
            col.ReadOnly = true;
            col.FillWeight = 60;
            gridResources.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Level";
            col.HeaderText = Globals.Translator["Level"];
            col.ReadOnly = true;
            col.FillWeight = 20;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridResources.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Target";
            col.HeaderText = Globals.Translator["Target level"];
            col.FillWeight = 20;
            col.DefaultCellStyle.BackColor = Color.Yellow;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridResources.Columns.Add(col);

            gridResources.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void gridBuildings_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            gridBuildings.Columns.Clear();

            DataGridViewComboBoxColumn ccol = new DataGridViewComboBoxColumn();
            ccol.DataPropertyName = "Name";
            ccol.HeaderText = Globals.Translator["Name"];
            ccol.ReadOnly = false;
            ccol.FillWeight = 60;
            ccol.Items.Add(Globals.Translator[BuildingType.None.ToString()]);
            for (int i = 4; i < 40; i++)
                ccol.Items.Add( Globals.Translator[((BuildingType)i).ToString()]);
            ccol.DefaultCellStyle.BackColor = Color.Yellow;
            gridBuildings.Columns.Add(ccol);
            
            DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Level";
            col.HeaderText = Globals.Translator["Level"];
            col.ReadOnly = true;
            col.FillWeight = 20;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridBuildings.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Target";
            col.HeaderText = Globals.Translator["Target level"];
            col.FillWeight = 20;
            col.DefaultCellStyle.BackColor = Color.Yellow;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridBuildings.Columns.Add(col);

            gridBuildings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void gridBuildings_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (gridBuildings.Columns[e.ColumnIndex].DataPropertyName == "Name")
            {
                if ((int)gridBuildings.Rows[e.RowIndex].Cells[1].Value != 0)
                    e.Cancel = true;
            }
            if (gridBuildings.Columns[e.ColumnIndex].DataPropertyName == "Target")
            {
                if ((string)gridBuildings.Rows[e.RowIndex].Cells[0].Value == Globals.Translator[BuildingType.None.ToString()])
                    e.Cancel = true;
            }
             
        }

        private void gridBuildings_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if (FirstActivate)
            {
                FirstActivate = false;
                Login();
            }

            for (int i = 1; i < tcMain.TabPages.Count; i++)
			{
                tcMain.TabPages[i].Text = ((TraviModule)i-1).ToString();
                if (!Globals.Register.IsRegistered((TraviModule)i-1))
                    tcMain.TabPages[i].Text += " (" + Globals.Translator["Trial"] + ")";
			}
          
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void gridProduction_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            gridProduction.Columns.Clear();

            DataGridViewTextBoxColumn col;
            
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "TypeName";
            col.HeaderText = Globals.Translator["Resource"];
            col.ReadOnly = true;
            gridProduction.Columns.Add(col);
            
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Capacity";
            col.HeaderText = Globals.Translator["Storage capacity"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Stock";
            col.HeaderText = Globals.Translator["Available"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Producing";
            col.HeaderText = Globals.Translator["Producing"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "TargetPercent";
            col.HeaderText = Globals.Translator["Target %"];
            col.DefaultCellStyle.BackColor = Color.Yellow;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "ActPercent";
            col.HeaderText = Globals.Translator["Actual %"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridProduction.Columns.Add(col);

            gridProduction.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void toolStripButton1_Click_2(object sender, EventArgs e)
        {
        }

        private void gridResourceOveralls_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            gridResourceOveralls.Columns.Clear();

            DataGridViewTextBoxColumn col;
            /*
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Type";
            col.HeaderText = Globals.Translator["Type"];
            col.ReadOnly = true;
            gridResourceOveralls.Columns.Add(col);
             */ 

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Stock";
            col.HeaderText = Globals.Translator["Available"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridResourceOveralls.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Producing";
            col.HeaderText = Globals.Translator["Producing"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridResourceOveralls.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Capacity";
            col.HeaderText = Globals.Translator["Storage capacity"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridResourceOveralls.Columns.Add(col);

            gridResourceOveralls.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void gridConstructions_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            gridConstructions.Columns.Clear();

            DataGridViewTextBoxColumn col;
            
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Name";
            col.HeaderText = Globals.Translator["Name"];
            col.ReadOnly = true;
            gridConstructions.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Level";
            col.HeaderText = Globals.Translator["Level"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            gridConstructions.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Ends";
            col.HeaderText = Globals.Translator["Ends"];
            col.ReadOnly = true;
            gridConstructions.Columns.Add(col);

            gridConstructions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void gridConstructions_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void gridCanBuild_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            gridCanBuild.Columns.Clear();

            DataGridViewTextBoxColumn col;

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Name";
            col.HeaderText = Globals.Translator["Name"];
            col.ReadOnly = true;
            col.FillWeight = 50;
            gridCanBuild.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Level";
            col.HeaderText = Globals.Translator["Level"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.FillWeight = 15;
            gridCanBuild.Columns.Add(col);

            /*
            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Cost";
            col.HeaderText = Globals.Translator["Cost"];
            col.ReadOnly = true;
            gridCanBuild.Columns.Add(col);
            */

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Lumber";
            col.HeaderText = Globals.Translator["Lumber"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.FillWeight = 20;
            gridCanBuild.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Clay";
            col.HeaderText = Globals.Translator["Clay"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.FillWeight = 20;
            gridCanBuild.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Iron";
            col.HeaderText = Globals.Translator["Iron"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.FillWeight = 20;
            gridCanBuild.Columns.Add(col);

            col = new DataGridViewTextBoxColumn();
            col.DataPropertyName = "Crop";
            col.HeaderText = Globals.Translator["Crop"];
            col.ReadOnly = true;
            col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            col.FillWeight = 20;
            gridCanBuild.Columns.Add(col);

            gridCanBuild.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
        }

        private void tcMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void tcMain_Selecting(object sender, TabControlCancelEventArgs e)
        {
            /*
            e.Cancel =
                !(
                e.TabPageIndex == 0
                ||
                (Globals.Register.IsRegistered((TraviModule)e.TabPageIndex-1))
                );
             */
        }

        private void bBuildRefresh_Click(object sender, EventArgs e)
        {
            pnlBrowser.Enabled = false;
            try
            {
                controller.Refresh();
            }
            finally
            {
                pnlBrowser.Enabled = true;
            }
        }


        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            pnlBrowser.Enabled = false;
            try
            {
                controller.Refresh();
            }
            finally
            {
                pnlBrowser.Enabled = true;
            }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (!Globals.Register.IsRegistered(TraviModule.Builder))
                MessageBox.Show(Globals.Translator["In the trial version the autobuild works for 1 hour only!"]);

            AutobuildStarted = DateTime.Now;

            tmrAutoBuild.Enabled = bAutoBuild.Checked;
            //tabControl1.Enabled = !bAutoBuild.Checked;

            if (bAutoBuild.Checked)
                MessageBox.Show(Globals.Translator["Please don't log on to this account elsewhere while autobuild is in use!"]);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            pnlBrowser.Enabled = false;
            try
            {
                controller.Build();
            }
            finally
            {
                pnlBrowser.Enabled = true;
            }
        }

        private void registrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegisterForm f = new RegisterForm();
            f.ShowDialog();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SettingsForm f = new SettingsForm())
            {
                f.ShowDialog();
            }
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void gridCanBuild_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
        }

        private void gridCanBuild_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex >= 0)
            {
                if (!((BuildingDisplay)controller.bsCanBuild[e.RowIndex]).Buildable)
                    e.CellStyle.BackColor = Color.LightPink;
            }

            if (e.ColumnIndex == 2 && e.RowIndex >= 0)
            {
                if (((BuildingDisplay)controller.bsCanBuild[e.RowIndex]).Lumber > 
                    ((Production)controller.bsProductions[0]).Stock)
                    e.CellStyle.BackColor = Color.LightPink;
            }
            if (e.ColumnIndex == 3 && e.RowIndex >= 0)
            {
                if (((BuildingDisplay)controller.bsCanBuild[e.RowIndex]).Clay >
                    ((Production)controller.bsProductions[1]).Stock)
                    e.CellStyle.BackColor = Color.LightPink;
            }
            if (e.ColumnIndex == 4 && e.RowIndex >= 0)
            {
                if (((BuildingDisplay)controller.bsCanBuild[e.RowIndex]).Iron >
                    ((Production)controller.bsProductions[2]).Stock)
                    e.CellStyle.BackColor = Color.LightPink;
            }
            if (e.ColumnIndex == 5 && e.RowIndex >= 0)
            {
                if (((BuildingDisplay)controller.bsCanBuild[e.RowIndex]).Crop >
                    ((Production)controller.bsProductions[3]).Stock)
                    e.CellStyle.BackColor = Color.LightPink;
            }

            e.Handled = false;

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SaveData();
        }

        private void gridResources_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            /*
            if (gridResources.Columns[e.ColumnIndex].DataPropertyName == "Target")
            {
                if (!Globals.Register.IsRegistered(TraviModule.Builder)
                    &&
                    (int)gridResources.Rows[e.RowIndex].Cells[e.ColumnIndex].Value > 1)
                {
                    MessageBox.Show(Globals.Translator["Trial version only builds to level 5."]);
                    //e.Cancel = true;
                    controller.CheckTrials();

                }
            }
            */  
        }

        private void gridBuildings_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            /*
            if (gridBuildings.Columns[e.ColumnIndex].DataPropertyName == "Target")
            {
                if (!Globals.Register.IsRegistered(TraviModule.Builder)
                    &&
                    (int)gridBuildings.Rows[e.RowIndex].Cells[e.ColumnIndex].Value > 1)
                {
                    MessageBox.Show(Globals.Translator["Trial version only builds to level 1."]);
                    //e.Cancel = true;
                    controller.CheckTrials();

                }
            }
             */ 

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                controller.RefreshMap();
                RedrawMap();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                controller.ReadMap();
                RedrawMap();
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnSendMail_Click(object sender, EventArgs e)
        {
            controller.SendMail2CSRecipients(txtRecipients.Text,
                                             txtSubject.Text,
                                             txtBody.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RegisterForm f = new RegisterForm();
            f.ShowDialog();
        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void webBrowser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            txtbxUri.Text = webBrowser.Url.AbsoluteUri;
        }

        private void btnClipboardUrl_Click(object sender, EventArgs e)
        {
            if (txtbxUri.Text != string.Empty)
                Clipboard.SetDataObject(txtbxUri.Text);
        }

        private void txtbxUri_Click(object sender, EventArgs e)
        {
            txtbxUri.SelectAll();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            int _lumber = Convert.ToInt32(txtbxLumber.Text);
            int _clay = Convert.ToInt32(txtbxClay.Text);
            int _iron = Convert.ToInt32(txtbxIron.Text);
            int _crop = Convert.ToInt32(txtbxCrop.Text);
            int _x = Convert.ToInt32(txtbxCoordX.Text);
            int _y = Convert.ToInt32(txtbxCoordY.Text);
            controller.SendResource(_lumber, _clay, _iron, _crop, _x, _y);
        }

    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
using System.Linq;

namespace EasyTravian
{
    /// <summary>
    /// controller
    /// k�perny�t k�ti �ssze az �zleti klasszokkal
    /// TILOS kiker�lni
    /// </summary>
    class TraviController
    {
        private MapPainter mapPainter = new MapPainter();

        /// <summary>
        /// Glob�l p�ld�ny az adatokb�l
        /// </summary>
        private TravianBase TraviBase = new TravianBase();
        /// <summary>
        /// Ez az legut�bb megjelen�tett t�rk�p k�z�ppontja
        /// </summary>
        private Point MapOrigin = new Point();
    
        /// <summary>
        /// Bindingsorce a b�ny�khoz
        /// </summary>
        public BindingSource bsResources = new BindingSource();
        /// <summary>
        /// Bindingsource a Termel�shez
        /// </summary>
        public BindingSource bsProductions = new BindingSource();
        /// <summary>
        /// BindingSource az �p�letekhez
        /// </summary>
        public BindingSource bsBuildings = new BindingSource();
        /// <summary>
        /// BindingSource a nyersanyag �sszes�t�h�z
        /// </summary>
        public BindingSource bsResourceOverall = new BindingSource();
        /// <summary>
        /// Aktu�lis falu neve
        /// </summary>
        public int ActiveVillageId = 0;
        public BindingSource bsConstruction = new BindingSource();
        public BindingSource bsCanBuild = new BindingSource();


        public TraviController()
        {
            bsResources.CurrentChanged += new EventHandler(bsResources_CurrentChanged);
            bsProductions.CurrentChanged += new EventHandler(bsProductions_CurrentChanged);
            bsBuildings.CurrentChanged += new EventHandler(bsBuildings_CurrentChanged);

            bsResourceOverall.DataSource = TraviBase.Data.Overalls.ResourceOverall.Values;
        }
        

        void bsBuildings_CurrentChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void bsProductions_CurrentChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        void bsResources_CurrentChanged(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        public void Refresh( )
        {
            TraviBase.GetBasicInfo();
        }

        /// <summary>
        /// Visszaadja a falvak nev�t
        /// </summary>
        /// <returns></returns>
        public string[] GetVillages()
        {
            string[] res = new string[TraviBase.Data.Villages.Count];

            var r = from v in TraviBase.Data.Villages.Values
                    select v.Props.Name;

            res = r.ToArray();

            //TraviBase.Data.Villages.Keys.CopyTo(res, 0);
            return res;
        }

        /// <summary>
        /// Be�ll�tja akt�vnak a falut
        /// </summary>
        /// <param name="VillageName">Falunak a neve</param>
        public void SetActiveVillage( int VillageId )
        {
            RefreshBindings(VillageId);

            ActiveVillageId = VillageId;
        }

        private void RefreshBindings(int VillageId)
        {
            VillageData village = null;
            if (TraviBase.Data.Villages.TryGetValue(VillageId, out village))
            {
                bsResources.DataSource = village.Resources.Values;
                bsProductions.DataSource = village.Productions.Values;
                bsBuildings.DataSource = village.Buildings.Values;
                bsConstruction.DataSource = village.Constructing;
                bsCanBuild.DataSource = village.CanBuild;
            }
            bsResourceOverall.DataSource = null;
            bsResourceOverall.DataSource = TraviBase.Data.Overalls.ResourceOverall.Values;
            //bsResourceOverall.ResetBindings(false);
            bsConstruction.ResetBindings(false);
            bsBuildings.ResetBindings(false);
            bsResources.ResetBindings(false);
            bsResourceOverall.ResetBindings(false);
            bsCanBuild.ResetBindings(false);

        }

        public void CheckTrials()
        {
            TraviBase.CheckTrials();
        }

        public void SetActiveVillageByName(string VillageName)
        {
            SetActiveVillage( TraviBase.GetVillageIdFromName(VillageName) );
        }

        /// <summary>
        /// Bet�lti az adatokat
        /// lehet, hogy nem k�ne mindent.
        /// T�rk�p tal�n lehetne k�l�n fileban is
        /// </summary>
        public bool Login()
        {
            //try
            {
                TraviBase.Login();
                RefreshBindings(ActiveVillageId);
                return true;
            }
            //catch
            {
            //    return false;
            }
        }

        /// <summary>
        /// Elmenti az adatokat
        /// lehet, hogy nem k�ne mindent.
        /// T�rk�p tal�n lehetne k�l�n fileban is
        /// </summary>
        public void SaveData()
        {
            TraviBase.Data.Save();
        }

        /// <summary>
        /// Kital�lja, hogy mit kell �p�teni �s meg is �p�ti
        /// </summary>
        public void Build()
        {
            try
            {
                TraviBase.Build();
            }
            catch (Exception)
            {
                //throw;
            }
            RefreshBindings(ActiveVillageId);
        }

        /// <summary>
        /// Szervert�l elk�ri a legfrisebb t�rk�pet
        /// </summary>
        public void RefreshMap()
        {
            TraviBase.RefreshMap();
        }

        /// <summary>
        /// Kirajzolja az aktu�lis t�rk�pet
        /// </summary>
        /// <param name="graphics">Ide rajzol</param>
        /// <param name="origin">K�zpontban l�v� falu koordin�t�i</param>
        /// <param name="zoom">zoom m�rt�ke</param>
        public void DrawActMap( Graphics graphics, Point origin, MapPainterProps props )
        {
            if (ActiveVillageId == -1) return;

            Dictionary<Point, MapElement> map = TraviBase.ActMap();

            if (origin.X == 1000 && origin.Y == 1000)
            {
                if (MapOrigin.X == 0 && MapOrigin.Y == 0)
                    MapOrigin = TraviBase.Data.Villages[ActiveVillageId].Props.Origin;
            }
            else
                MapOrigin = origin;

            mapPainter.Paint(graphics, map, TraviBase.Data.Terrain, MapOrigin, props);
        }

        /// <summary>
        /// Megkeresi, hogy az eg�r mutat� alatt melyik falu van
        /// </summary>
        /// <param name="size">T�rk�p m�rete</param>
        /// <param name="point">eg�rpozici�</param>
        /// <param name="zoom">zoom m�rt�ke</param>
        /// <returns></returns>
        internal MapElement GetMapInfo(Size size, Point point, int zoom)
        {

            MapElement me = mapPainter.GetMapInfo(size, point, MapOrigin, TraviBase.ActMap(), zoom);
            if (me != null)
            {
                TerrainType tt = TerrainType.NA;
                TraviBase.Data.Terrain.TryGetValue(new Point(me.X, me.Y), out tt);
                if (tt != TerrainType.NA)
                    me.Terrain = tt.ToString();
            }

            return me;
        }

        /// <summary>
        /// T�rk�pre kattint�s
        /// Bepoz�cion�lja a kattintott falut
        /// </summary>
        /// <param name="size">T�rk�p m�rete</param>
        /// <param name="point">ide kattintottak</param>
        /// <param name="zoom">zoom m�rt�ke</param>
        internal void MapClicked(Size size, Point point, int zoom)
        {
            MapElement me = mapPainter.GetMapInfo(size, point, MapOrigin, TraviBase.ActMap(), zoom);
            if (me != null)
                MapOrigin = new Point(me.X, me.Y);
        }

        internal void ReadMap()
        {
            TraviBase.ParseMap();
        }

        public List<string> ActiveClans()
        {
            return TraviBase.ActiveClans();
        }

        public void SendMail2CSRecipients(string recipients, string subject, string body)
        {
            TraviBase.SendMail(recipients, subject, body);
        }

    }
}

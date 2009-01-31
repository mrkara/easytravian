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
    /// képernyõt köti össze az üzleti klasszokkal
    /// TILOS kikerülni
    /// </summary>
    class TraviController
    {
        private MapPainter mapPainter = new MapPainter();

        /// <summary>
        /// Globál példány az adatokból
        /// </summary>
        private TravianBase TraviBase = new TravianBase();
        /// <summary>
        /// Ez az legutóbb megjelenített térkép középpontja
        /// </summary>
        private Point MapOrigin = new Point();
    
        /// <summary>
        /// Bindingsorce a bányákhoz
        /// </summary>
        public BindingSource bsResources = new BindingSource();
        /// <summary>
        /// Bindingsource a Termeléshez
        /// </summary>
        public BindingSource bsProductions = new BindingSource();
        /// <summary>
        /// BindingSource az épületekhez
        /// </summary>
        public BindingSource bsBuildings = new BindingSource();
        /// <summary>
        /// BindingSource a nyersanyag összesítõhöz
        /// </summary>
        public BindingSource bsResourceOverall = new BindingSource();
        /// <summary>
        /// Aktuális falu neve
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
        /// Visszaadja a falvak nevét
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
        /// Beállítja aktívnak a falut
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
        /// Betölti az adatokat
        /// lehet, hogy nem kéne mindent.
        /// Térkép talán lehetne külön fileban is
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
        /// lehet, hogy nem kéne mindent.
        /// Térkép talán lehetne külön fileban is
        /// </summary>
        public void SaveData()
        {
            TraviBase.Data.Save();
        }

        /// <summary>
        /// Kitalálja, hogy mit kell építeni és meg is építi
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
        /// Szervertõl elkéri a legfrisebb térképet
        /// </summary>
        public void RefreshMap()
        {
            TraviBase.RefreshMap();
        }

        /// <summary>
        /// Kirajzolja az aktuális térképet
        /// </summary>
        /// <param name="graphics">Ide rajzol</param>
        /// <param name="origin">Központban lévõ falu koordinátái</param>
        /// <param name="zoom">zoom mértéke</param>
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
        /// Megkeresi, hogy az egér mutató alatt melyik falu van
        /// </summary>
        /// <param name="size">Térkép mérete</param>
        /// <param name="point">egérpozició</param>
        /// <param name="zoom">zoom mértéke</param>
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
        /// Térképre kattintás
        /// Bepozícionálja a kattintott falut
        /// </summary>
        /// <param name="size">Térkép mérete</param>
        /// <param name="point">ide kattintottak</param>
        /// <param name="zoom">zoom mértéke</param>
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

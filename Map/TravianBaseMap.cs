using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace EasyTravian
{
    public partial class TravianBase
    {
        Dictionary<Point, MapElement> actMap = new Dictionary<Point, MapElement>();

        /// <summary>
        /// letölti a térkép infókat
        /// meg kéne még nézni, hogy ugyanaz-e, mint az előző, mer akkor nem kéne feleslegesen tárolni
        /// </summary>
        public void RefreshMap()
        {
            MapCollector.Collect("http://" + Globals.Cfg.Server + "/map.sql", Data);
            Data.MapChanged = true;
            actMap.Clear();
        }

        /// <summary>
        /// Visszaadja a legfrisebb térképet
        /// </summary>
        /// <returns></returns>
        public Dictionary<Point, MapElement> ActMap()
        {
            if (Data.Map.Count == 0)
            {
                return null;
            }

            if (actMap.Count == 0)
            {

                DateTime maxDate = Data.Map.Max(m => m.TimeStamp);

                var map = from m in Data.Map
                          where m.TimeStamp == maxDate
                          select m;

                foreach (MapElement me in map.ToList())
                {
                    actMap[new Point(me.X, me.Y)] = me;
                }

            }

            return actMap;

        }

        public void ParseMap()
        {
            Navigate("karte.php");

            HtmlElement el = xpath.SelectElement("id('lmid2')/div[4]/div[2]");
            HtmlElement el2 = xpath.SelectElement("id('lmid2')/div[4]/map");

            Point orig = ParseMapOrigin();

            int n = 0;
            Point p = new Point();
            TerrainType tt = TerrainType.NA;
            string s;

            for (int y = 3; y >= -3; y--)
            {
                for (int x = -3; x <= 3; x++)
                {
                    p.X = orig.X + x;
                    p.Y = orig.Y + y;

                    string[] ss = el.GetElementsByTagName("IMG")[n].GetAttribute("src").Split('/');
                    s = ss[ss.Length-1];

                    tt = TerrainType.NA;

                    switch (s)
                    {
                        case "o1.gif": 
                        case "o2.gif": 
                            tt = TerrainType.oase_lumber25;
                            break;
                        case "o3.gif": 
                            tt = TerrainType.oase_lumber25_crop25;
                            break;
                        case "o4.gif": 
                        case "o5.gif": 
                            tt = TerrainType.oase_clay25;
                            break;
                        case "o6.gif": 
                            tt = TerrainType.oase_clay25_crop25;
                            break;
                        case "o7.gif": 
                        case "o8.gif": 
                            tt = TerrainType.oase_iron25;
                            break;
                        case "o9.gif": 
                            tt = TerrainType.oase_iron25_crop25;
                            break;
                        case "o10.gif": 
                        case "o11.gif": 
                            tt = TerrainType.oase_crop25;
                            break;
                        case "o12.gif": 
                            tt = TerrainType.oase_crop50;
                            break;
                    }

                    Data.Terrain[p] = tt;

                }
            }

            for (int y = 3; y >= -3; y--)
            {
                for (int x = -3; x <= 3; x++)
                {
                    p.X = orig.X + x;
                    p.Y = orig.Y + y;

                    tt = Data.Terrain[p];

                    if (tt == TerrainType.NA)
                    {
                        s = xpath.SelectElement("id('lmid2')/div[4]/map").GetElementsByTagName("area")[8 + n].GetAttribute("href");
                        Navigate(s);
                        HtmlElement f = null;
                        if (xpath.ElementExists("id('lmid2')/div[5]/div"))
                            f = Globals.Web.Document.GetElementById("lmid2").GetElementsByTagName("div")[1];
                        else
                            f = Globals.Web.Document.GetElementById("lmid2").GetElementsByTagName("div")[0];
                        if (f.Id != null)
                        {
                            int r = int.Parse(f.Id.Substring(1, 1));
                            switch (r)
                            {
                                case 1:
                                    tt = TerrainType.field_9crop;
                                    break;
                                case 2:
                                    tt = TerrainType.field_5iron;
                                    break;
                                case 3:
                                    tt = TerrainType.field_4all;
                                    break;
                                case 4:
                                    tt = TerrainType.field_5clay;
                                    break;
                                case 5:
                                    tt = TerrainType.field_5lumber;
                                    break;
                                case 6:
                                    tt = TerrainType.field_15crop;
                                    break;
                            }
                        }
                        Navigate("back");
                    }

                    Data.Terrain[p] = tt;

                    n++;
                }
                
            }
            Data.TerrainChanged = true;

        }

        private Point ParseMapOrigin()
        {
            Point orig = new Point();
            HtmlElement c = xpath.SelectElement("id('map_content')/div[1]/table/tbody/tr/td[2]");
            if (c != null)
            {
                orig.X = int.Parse(c.InnerText.Substring(1));
                c = xpath.SelectElement("id('map_content')/div[1]/table/tbody/tr/td[4]");
                orig.Y = int.Parse(c.InnerText.Substring(0, c.InnerText.Length - 1));
            }
            return orig;
        }

        public List<string> ActiveClans()
        {
            Dictionary<Point, MapElement> map = ActMap();
            if (map == null) return null;

            var res = from m in map.Values
                      select m.Alliance;

            return res.Distinct().ToList();
        }

    }
}

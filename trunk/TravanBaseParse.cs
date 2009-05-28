using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

namespace EasyTravian
{
    //b0y0
    public partial class TravianBase
    {
        /// <summary>
        /// Összeszedi az alap infókat egy birodalomról.
        /// Épületek és szintek, más nem
        /// </summary>
        public void GetBasicInfo()
        {
            //Data.Villages.Clear();
            Navigate("dorf1.php");

            ParseVillages();

            if (Data.Villages.Count > 1)
            {
                foreach (VillageData village in Data.Villages.Values)
                {
                    ChangeToVillage(village);
                    //Navigate(village.url);
                    ParseTribe();
                    Navigate("dorf1.php");
                    ParseOverView();
                    Navigate("dorf2.php");
                    ParseCentre();
                }
            }
            else
            {
                ParseTribe();
                Navigate("dorf1.php");
                ParseOverView();
                Navigate("dorf2.php");
                ParseCentre();
            }

        }

        private void ParseTribe()
        {
            if (Data.Tribe == null)
            {
                // profil linkje
                HtmlElement profil = xpath.SelectElement("id('sleft')/p/a[3]");
                string href = profil.GetAttribute("href");
                Navigate(href);

                HtmlElement tribe = xpath.SelectElement("id('content')/table[1]/tbody/tr[5]/td[2]");

                switch (tribe.InnerText)
                {
                    case "Római":
                        Data.Tribe = TribeType.Roman;
                        break;
                    case "Germán":
                        Data.Tribe = TribeType.Teuton;
                        break;
                    case "Gall":
                        Data.Tribe = TribeType.Gaul;
                        break;
                }
            }
        }

        private void ParseTribe2()
        {
            if (Data.Tribe == null)
            {
                Navigate("dorf2.php");
                // mi ez? falat nézi? Földfal, Cölöpfal, Kőfal
                // id('map2')/area[22]
                if (Globals.Web.Document.Body.InnerHtml.Replace("&amp;", "&").Contains("<DIV class=\"d2_x d2_1\">"))
                {
                    Data.Tribe = TribeType.Gaul;
                    return;
                }

                if (Globals.Web.Document.Body.InnerHtml.Replace("&amp;", "&").Contains("<DIV class=\"d2_x d2_11\">"))
                {
                    Data.Tribe = TribeType.Roman;
                    return;
                }

                if (Globals.Web.Document.Body.InnerHtml.Replace("&amp;", "&").Contains("<DIV class=\"d2_x d2_12\">"))
                {
                    Data.Tribe = TribeType.Teuton;
                    return;
                }

                //<div class="d2_x d2_0">
                //<div class="d2_x d2_1">
                //<div class="d2_x d2_11">
                //<div class="d2_x d2_12">


                if (Globals.Web.Document.Body.InnerHtml.Replace("&amp;", "&").Contains("<DIV class=\"d2_x d2_0\">"))
                {                                                                       

                    Navigate("build.php?id=40");

                    Regex rex = new Regex(@"dorf[1-2]\.php\?a=.*&id=40.*&c=(.{3})");
                    MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("lmid2").InnerHtml.Replace("&amp;", "&"));

                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            switch (match.Value.Substring(12, 2))
                            {
                                case "31":
                                    Data.Tribe = TribeType.Roman;
                                    break;
                                case "32":
                                    Data.Tribe = TribeType.Teuton;
                                    break;
                                case "33":
                                    Data.Tribe = TribeType.Gaul;
                                    break;
                            }
                        }
                    }
                }

            }
        }

        /// <summary>
        /// épületek és szintjeik kimojolása
        /// </summary>
        private void ParseCentre()
        {
            /*
            HtmlElement elem = xpath.SelectElement("id('lmid2')/div[1]/h1");
            //if (e == null) 
            string villageName = elem.InnerText;
             */ 

            HtmlElement f = Globals.Web.Document.GetElementById("lmid2");

            //bool[] x = {false, false, false, false, false, false, false, false, false, false, false, 
            //               false, false, false, false, false, false, false, false, false, false, false };

            //buildings
            foreach (HtmlElement e in f.GetElementsByTagName("img"))
            {
                string eclass = e.GetAttribute("className");
                string img = e.GetAttribute("src");
                string[] imarr = img.Split('/');

                try
                {
                    int pid = int.Parse(eclass.Substring(1, eclass.Length - 1));
                    string b = imarr[imarr.Length - 1].Substring(1, imarr[imarr.Length - 1].Length - 5);
                    if (!b.Equals("so"))
                    {
                        //x[pid - 1] = true;
                        int bid = int.Parse(b);
                        Data.Villages[ActiveVillage].Buildings[pid].Type = (BuildingType)bid - 1;
                    }
                    else
                        if (Data.Villages[ActiveVillage].Buildings[pid].Level != 0)
                            Data.Villages[ActiveVillage].Buildings[pid].Type = BuildingType.None;


                }
                catch (Exception ex)
                {
                    if (tryToLogin)
                        throw ex;
                }
            }
            //wall
            HtmlElement wallEl = f.GetElementsByTagName("map")[0].GetElementsByTagName("area")[0];
            Data.Villages[ActiveVillage].Buildings[22].Level = GetLevel(wallEl);

            switch (Data.Tribe)
            {
                case TribeType.Roman:
                    Data.Villages[ActiveVillage].Buildings[22].Type = BuildingType.City_wall;
                    break;
                case TribeType.Gaul:
                    Data.Villages[ActiveVillage].Buildings[22].Type = BuildingType.Palisade;
                    break;
                case TribeType.Teuton:
                    Data.Villages[ActiveVillage].Buildings[22].Type = BuildingType.Earth_wall;
                    break;
            }

            //levels
            int n = 0;
            foreach (HtmlElement e in f.GetElementsByTagName("map")[1].GetElementsByTagName("area"))
            {
                if (n == 0)
                    Data.Villages[ActiveVillage].Buildings[21].Level = GetLevel(e);
                else
                if (n < 21)
                {
                    Data.Villages[ActiveVillage].Buildings[n].Level = GetLevel(e);
                }

                if (n == 22)
                {

                }
                n++;
            }

            /*
            for (int i = 0; i < 22; i++)
            {
                if (!x[i])
                {
                    if (i < 20)
                        Data.Villages[ActiveVillage].Buildings[i + 1].Type = BuildingType.None;
                    Data.Villages[ActiveVillage].Buildings[i + 1].Level = 0;
                    Data.Villages[ActiveVillage].Buildings[i + 1].Target = 0;
                    Data.Villages[ActiveVillage].Buildings[i + 1].NextLevelCost.Clear();
                }
                
            }
             */ 
        }

        /// <summary>
        /// kimarja az épület szintjét
        /// egyelőre sajnos a hint-ből
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private int GetLevel(HtmlElement e)
        {
            string title = e.GetAttribute("title");
            string[] tt = title.Split(' ');
            int level;
            try
            {
                level = int.Parse(tt[tt.Length - 1]);
            }
            catch (Exception)
            {
                level = 0;
            }
            return level;
        }

        /// <summary>
        /// kiparszolja a nyersanyag termelést és a bámyákat
        /// </summary>
        private void ParseOverView()
        {
            //string villageName = xpath.SelectElement("id('lmid2')/div[1]/h1").InnerText;
            ParseProduction(ActiveVillage);
            ParseResources(ActiveVillage);
        }

        /// <summary>
        /// összenézi hány falu van a birodalomban
        /// </summary>
        private void ParseVillages()
        {
            //többfalus (Falvak:)
            // 2.0: if (xpath.ElementExists("id('lright1')/a/span"))
            if (xpath.ElementExists("id('vlist')/a/span"))
            {

                Regex rex = new Regex(@"newdid=\d+.*");
                MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("vlist").InnerHtml.Replace("&amp;", "&"));
                //MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("lright1").InnerHtml.Replace("&amp;", "&"));

                Regex rexx = new Regex(@"right dlist1.*");
                MatchCollection matchesx = rexx.Matches(Globals.Web.Document.GetElementById("vlist").InnerHtml.Replace("&amp;", "&"));
                //MatchCollection matchesx = rexx.Matches(Globals.Web.Document.GetElementById("lright1").InnerHtml.Replace("&amp;", "&"));

                Regex rexy = new Regex(@"left dlist3.*");
                MatchCollection matchesy = rexy.Matches(Globals.Web.Document.GetElementById("vlist").InnerHtml.Replace("&amp;", "&"));
                //MatchCollection matchesy = rexy.Matches(Globals.Web.Document.GetElementById("lright1").InnerHtml.Replace("&amp;", "&"));

                for (int i = 0; i < matches.Count; i++)
                {
                    int id = int.Parse(matches[i].Value.Substring(7, matches[i].Value.IndexOf('\"') - 7));

                    if (!Data.Villages.ContainsKey(id))
                    {
                        Data.Villages[id] = new VillageData();
                        Data.Villages[id].Props.Id = id;
                        Data.Villages[id].Props.Origin = new Point(
                            int.Parse(matchesx[i].Value.Substring(15, matchesx[i].Value.IndexOf('<')-15)), 
                            int.Parse(matchesy[i].Value.Substring(13, matchesy[i].Value.IndexOf(')')-13)));
                    }

                    // mert amikor 2. falu kész van...
                    if (Data.Villages.ContainsKey(0))
                    {
                        //eh... át kéne másolni az adatokat
                        Data.Villages.Remove(0);
                    }

                    Data.Villages[id].Props.url = "dorf1.php?newdid=" + Data.Villages[id].Props.Id;
                    Data.Villages[id].Props.Name = matches[i].Value.Substring(
                        matches[i].Value.IndexOf('>') + 1,
                        matches[i].Value.IndexOf('<') - matches[i].Value.IndexOf('>') - 1); 

                }


                /*
                int n = 0;
                //for (int n = 0; n < 100; n++)
                {
                    //if (xpath.ElementExists("id('lright1')/table/tbody/tr[" + (n + 1) + "]/td[1]/a"))
                    while (xpath.ElementExists("id('lright1')/table/tbody/tr[" + (n * 2 + 1) + "]/td[1]/a"))
                    {
                        //HtmlElement vil = xpath.SelectElement("id('lright1')/table/tbody/tr[" + (n + 1) + "]/td[1]/a");
                        HtmlElement vil = xpath.SelectElement("id('lright1')/table/tbody/tr[" + (n * 2 + 1) + "]/td[1]/a");

                        string href = vil.GetAttribute("href");
                        int id = int.Parse(href.Substring(href.IndexOf('=') + 1));

                        if (!Data.Villages.ContainsKey(id))
                        {
                            Data.Villages[id] = new VillageData();
                            Data.Villages[id].Props.Id = id;
                            HtmlElement coords = xpath.SelectElement("id('lright1')/table/tbody/tr[" + (n * 2 + 1) + "]/td[2]");
                            //HtmlElement coords = xpath.SelectElement("id('lright1')/table/tbody/tr[" + (n + 1) + "]/td[2]");
                            string[] s = coords.InnerText.Replace(')', ' ').Replace('(', ' ').Split('|');

                            Data.Villages[id].Props.Origin = new Point(int.Parse(s[0]), int.Parse(s[1]));
                        }

                        // mert amikor 2. falu kész van...
                        if (Data.Villages.ContainsKey(0))
                        {
                            //eh... át kéne másolni az adatokat
                            Data.Villages.Remove(0);
                        }

                        Data.Villages[id].Props.url = href;
                        Data.Villages[id].Props.Name = vil.InnerText;
                        n++;
                    }
                }
                */
            }
            //egyfalus
            else
            {
                // 2.0: string name = xpath.SelectElement("id('lmid2')/div[1]/h1").InnerText;
                string name = xpath.SelectElement("id('content')/h1").InnerText;
                if (!Data.Villages.ContainsKey(0))
                {
                    Data.Villages[0] = new VillageData();
                    Data.Villages[0].Props.Name = name;
                    Navigate("karte.php");
                    Data.Villages[0].Props.Origin = ParseMapOrigin();
                }
            }

        }

        /// <summary>
        /// bányákat parszol
        /// </summary>
        /// <param name="VillageName"></param>
        private void ParseResources(int VillageId)
        {
            HtmlElement f = Globals.Web.Document.GetElementById("content").GetElementsByTagName("div")[1];
            if (f.InnerText.Trim() != "") //hűség szarság...
                f = Globals.Web.Document.GetElementById("content").GetElementsByTagName("div")[2];

            //resource map
            int r = int.Parse(f.Id.Substring(1, 1));
            for (int t = 1; t < 19; t++)
                Data.Villages[VillageId].Resources[t].Type = ResourceMaps.Map[r - 1][t - 1];

            bool[] x = {false, false, false, false, false, false, 
                           false, false, false, false, false, false,
                           false, false, false, false, false, false };

            //built level
            foreach (HtmlElement e in f.GetElementsByTagName("img"))
            {
                string eclass = e.GetAttribute("className");
                string[] eclassarr = eclass.Split(' ');

                if (eclassarr.Length > 1)
                {
                    int id = int.Parse(eclassarr[1].Substring(2, eclassarr[1].Length-2 ));
                    int lev = int.Parse(eclassarr[2].Substring(5, eclassarr[2].Length - 5));

                    x[id - 1] = true;
                    if (lev != Data.Villages[VillageId].Resources[id].Level)
                    {
                        Data.Villages[VillageId].Resources[id].Level = lev;
                        Data.Villages[VillageId].Resources[id].NextLevelCost.Clear();
                    }
                }
                    

                //Villages[VillageName].Resources[id].href = 
                //id('lmid2')/table[2]/tbody/tr/td
            }

            for (int i = 0; i < x.Count(); i++)
            {
                if (!x[i])
                {
                    Data.Villages[VillageId].Resources[i + 1].Level = 0;
                    Data.Villages[VillageId].Resources[i + 1].Target = 0;
                    Data.Villages[VillageId].Resources[i + 1].NextLevelCost.Clear();
                }
            }
        }

        /// <summary>
        /// termelést parszol
        /// </summary>
        /// <param name="VillageName"></param>
        private void ParseProduction(int VillageId)
        {
            int sum = 0;
            foreach (int res in Enum.GetValues(typeof(ResourcesType)))
            {
                ResourcesType rt = (ResourcesType)res;

                HtmlElement el = xpath.SelectElement("id('l" + res + "')");
                string[] s = el.InnerText.Split('/');

                Data.Villages[VillageId].Productions[rt].Producing = int.Parse(el.GetAttribute("title"));
                Data.Villages[VillageId].Productions[rt].Stock = int.Parse(s[0]);
                Data.Villages[VillageId].Productions[rt].Capacity = int.Parse(s[1]);
                sum += Data.Villages[VillageId].Productions[rt].Producing;
            }

            foreach (Production prod in Data.Villages[VillageId].Productions.Values)
                prod.ActPercent = prod.Producing * 100 / sum;
        }

        /// <summary>
        /// megnézi, hogy egy épület építhető-e és mennyibe kerül
        /// lehetne reszelni még rajta: pacik, katonák
        /// </summary>
        /// <param name="building"></param>
        private void ParseConstruction(Building building, bool Forced)
        {
            if (building.Type != BuildingType.None && building.Level < building.Target)
            {
                if (Forced || building.NextLevelCost.Sum == 0)
                {
                    Navigate("build.php?id=" + building.BuildId);

                    //build href
                    /*
                    building.BuildHref = "";
                    Regex rex = new Regex(@"dorf[1-2]\.php\?a=.*&c=(.{3})");
                    MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("lmid2").InnerHtml.Replace("&amp;", "&"));
                    if (matches.Count > 0)
                        building.BuildHref = matches[matches.Count - 1].Value;
                    */

                    building.NextLevelCost.Clear();

                    //cost
                    Regex rex = new Regex(@"\d+ \| \d+ \| \d+ \| \d+ \| \d+ \|  \d+\:\d+\:\d+");
                    MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("lmid2").InnerText);
                    if (matches.Count > 0)
                    {
                        string[] sa = matches[matches.Count - 1].Value.Split('|');
                        building.NextLevelCost[ResourcesType.Lumber] = int.Parse(sa[0]);
                        building.NextLevelCost[ResourcesType.Clay] = int.Parse(sa[1]);
                        building.NextLevelCost[ResourcesType.Iron] = int.Parse(sa[2]);
                        building.NextLevelCost[ResourcesType.Crop] = int.Parse(sa[3]);

                        sa = sa[5].Split(':');
                        building.BuildTime =
                            TimeSpan.FromHours(int.Parse(sa[0])) +
                            TimeSpan.FromMinutes(int.Parse(sa[1])) +
                            TimeSpan.FromSeconds(int.Parse(sa[2]));
                    }
                }
            }
        }

        private List<BuildingType> ParseBlankSpace(int id)
        {

            List<BuildingType> ret = new List<BuildingType>();

            Regex rex = new Regex(@"dorf[1-2]\.php\?a=.*&id=.*&c=(.{3})");
            MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("lmid2").InnerHtml.Replace("&amp;", "&"));

            foreach (Match match in matches)
            {
                
            }

            //if (matches.Count > 0)
            //    building.BuildHref = matches[matches.Count - 1].Value;

            return ret;

        }

    }
}

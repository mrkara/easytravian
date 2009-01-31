using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyTravian
{
    public partial class TravianBase
    {
        /// <summary>
        /// Megkeres és megépít egy épületet
        /// lehetne még rajta reszelni...
        /// </summary>
        public void Build()
        {
            Globals.Logger.Log("=====================================", LogType.ltReport);
            Globals.Logger.Log("Build...", LogType.ltDebug);
            foreach (VillageData village in Data.Villages.Values)
            {
                village.CleanConstructions();
                foreach (Construction c in village.Constructing)
                    Globals.Logger.Log("constr> " + c.Name + ' ' + c.Level + ' ' + c.Ends, LogType.ltReport);


                Globals.Logger.Log(village.Props.Name, LogType.ltReport);

                Globals.Logger.Log("Stocks:" + village.Stock, LogType.ltDebug);

                if (Data.Tribe == TribeType.Roman)
                {
                    if (village.NeedToBuildResource()
                        &&
                        !village.ResourceConstructionInProgress()
                        &&
                        (Data.Tribe == TribeType.Roman || !village.BuildingConstructionInProgress()))
                    {
                        ChangeToVillage(village);
                        ParseProduction(village.Props.Id);
                        UpgradeBuilding(village.Resources.Values.ToList(), village);
                    }

                    if (village.NeedToBuildBuilding()
                        &&
                        !village.BuildingConstructionInProgress()
                        &&
                        (Data.Tribe == TribeType.Roman || !village.ResourceConstructionInProgress()))
                    {
                        ChangeToVillage(village);
                        ParseProduction(village.Props.Id);
                        if (!NewBuilding(village))
                        {
                            UpgradeBuilding(village.Buildings.Values.ToList(), village);
                        }
                    }
                }
                else //nem római
                {
                    if (  (village.NeedToBuildBuilding() || village.NeedToBuildResource())
                          &&  
                          (!village.BuildingConstructionInProgress() && !village.ResourceConstructionInProgress())  )
                    {
                        ChangeToVillage(village);
                        ParseProduction(village.Props.Id);
                        if (!NewBuilding(village))
                        {
                            UpgradeBuilding(village.Buildings.Values.Union( village.Resources.Values ).ToList(), village);
                        }
                    }
                }


            }
        }

        private bool NewBuilding(VillageData village)
        {
            Globals.Logger.Log("NewBuilding...", LogType.ltDebug);

            var news = from b in village.Buildings.Values
                       where b.Type != BuildingType.None
                          && b.Level == 0
                          && b.Target > 0
                       select b;

            foreach (Building b in news.ToList())
            {
                Globals.Logger.Log("try: " + b.Name + '@' + b.Id, LogType.ltDebug);
                Navigate("build.php?id=" + b.BuildId);

                Regex rex1 = new Regex(@"dorf[1-2]\.php\?a=\d+&id=\d+&c=(.{3})");
                MatchCollection matches1 = rex1.Matches(Globals.Web.Document.GetElementById("lmid2").InnerHtml.Replace("&amp;", "&"));

                Regex rex2 = new Regex(@"\d+ \| \d+ \| \d+ \| \d+ \| \d+ \|  \d+\:\d+\:\d+");
                MatchCollection matches2 = rex2.Matches(Globals.Web.Document.GetElementById("lmid2").InnerText);

                if (matches1.Count == matches2.Count)
                {
                    for (int i = 0; i < matches1.Count; i++)
                    {
                        int id = int.Parse(matches1[i].Value.Split('?')[1].Split('&')[0].Split('=')[1]);

                        if (id == ((int)b.Type) + 1)
                        {
                            string[] sa = matches2[i].Value.Split('|');
                            sa = sa[5].Split(':');
                            b.BuildTime =
                                TimeSpan.FromHours(int.Parse(sa[0])) +
                                TimeSpan.FromMinutes(int.Parse(sa[1])) +
                                TimeSpan.FromSeconds(int.Parse(sa[2]));

                            Globals.Logger.Log("NewBuild: " + b.Name, LogType.ltReport);
                            Globals.Logger.Log("Href: " + matches1[i].Value, LogType.ltDebug);
                            Navigate(matches1[i].Value);
                            b.Level++;
                            b.BuildHref = "";
                            b.NextLevelCost.Clear();
                            AddToConstructionList(b);
                            return true;
                        }

                    }
                }
                else
                {
                    Regex rex = new Regex(@"dorf[1-2]\.php\?a=" + ((int)b.Type + 1).ToString() + "&id=" + b.BuildId + "&c=(.{3})");
                    MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("lmid2").InnerHtml.Replace("&amp;", "&"));
                    if (matches.Count > 0)
                    {
                        Globals.Logger.Log("NewBuild: " + b.Name, LogType.ltReport);
                        Globals.Logger.Log("Href: " + matches[0].Value, LogType.ltDebug);
                        Navigate(matches[0].Value);
                        b.Level++;
                        b.BuildHref = "";
                        b.NextLevelCost.Clear();
                        AddToConstructionList(b);
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpgradeBuilding(List<Building> buildings, VillageData village)
        {
            bool done = false;

            Globals.Logger.Log("Upgrade... ", LogType.ltDebug);

            foreach (Building building in buildings)
                ParseConstruction(building, false);
            /*
            //épületek árai
            var costs =
                from b in buildings
                where b.Level < b.Target
                   && b.NextLevelCost < village.Stock
                   && (b.Level > 0 || b.BuildId < 19)
                //select BuildingCosts[b.Type][b.Level + 1].Sum;
                select b.NextLevelCost.Sum;

            if (costs.Count() != 0)
            {
                //legoccsóbb
                int minCost = costs.Min();

                Globals.Logger.Log("Cheapest: " + minCost, LogType.ltDebug);

                //ezeket vannak egy árban a legolcsóbbal (10% türés)
                var Buildables =
                    from b in buildings
                    where (b.NextLevelCost.Sum - minCost) < (minCost / 20)
                        //where (BuildingCosts[b.Type][b.Level+1].Sum - minCost) <(minCost / 10)
                        && b.Level < b.Target
                        && b.NextLevelCost < village.Stock
                    select b;

                foreach (Building b in Buildables)
                {
                    Globals.Logger.Log( b.Name + '@' + b.Id + ' ' + b.NextLevelCost, LogType.ltDebug);
                }
            */
                //nyersanyagok közül választani kell
                //ebben a sorrendben kéne építeni
                var prodOrd =
                    from p in village.Productions.Values
                    orderby p.ActPercent - p.TargetPercent
                    select p;

                foreach (Production prod in prodOrd)
                    Globals.Logger.Log(prod.TypeName + ' ' + prod.Producing + ' ' + prod.TargetPercent + ' ' + prod.ActPercent, LogType.ltDebug);


                //végigskera, hogy van-e az olcsók között ijen
                foreach (Production prod in prodOrd)
                {
                    /*
                    var nowBuild =
                        from b in Buildables
                        where b.Producing == prod.Type
                        select b;
                     */

                    var nowBuild =
                        from b in buildings
                        where b.Producing == prod.Type
                        && b.Level < b.Target
                        && b.NextLevelCost < village.Stock
                        && b.BuildId < 19
                        orderby b.NextLevelCost.Sum
                        select b;

                    if (nowBuild.Count() > 0)
                    {
                        if (!done)
                        {
                            BuildIt(nowBuild.First());
                            done = true;
                        }
                    }
                }

                //ha nyersanyagot nem kellet akkor hátha épületet
                if (!done)
                {
                    var Buildables = 
                        from b in buildings
                        where b.BuildId > 18
                        && b.Level > 0
                        && b.Level < b.Target
                        && b.NextLevelCost < village.Stock
                        orderby b.NextLevelCost.Sum
                        select b;

                    if (Buildables.Count() > 0)
                    {
                        BuildIt(Buildables.First());
                        done = true;
                    }
                }
            //}  
        }

        /// <summary>
        /// egy épület megépítése
        /// kéne tudni valóban sikerült-e megépíteni,
        /// és csak akkor emelni a szintet...
        /// </summary>
        /// <param name="b"></param>
        private void BuildIt(Building b)
        {
            Globals.Logger.Log( "Try Upgrade: " + b.Name + '@' + b.Id + ' ' + b.NextLevelCost, LogType.ltDebug);
            
            Navigate("build.php?id=" + b.BuildId);
            ParseConstruction(b, true);

            Regex rex = new Regex(@"dorf[1-2]\.php\?a=.*&c=(.{3})");
            MatchCollection matches = rex.Matches(Globals.Web.Document.GetElementById("lmid2").InnerHtml.Replace("&amp;", "&"));
            if (matches.Count > 0)
                b.BuildHref = matches[matches.Count - 1].Value;

            if (b.BuildHref != null && b.BuildHref.Length != 0)
            {
                Globals.Logger.Log("Upgrade: " + b.href, LogType.ltDebug);
                Navigate(b.BuildHref);
                b.Level++;
                b.BuildHref = "";
                b.NextLevelCost.Clear();
                AddToConstructionList(b);
            }
             
              
        }

        private void AddToConstructionList(Building b)
        {
            Construction c = new Construction();
            c.Building = b.Type;
            c.Started = DateTime.Now;
            if (b.BuildTime == TimeSpan.Zero)
                c.Ends = c.Started.AddMinutes(2);
            else
                c.Ends = c.Started.Add(b.BuildTime);
            c.Level = b.Level;

            Data.Villages[ActiveVillage].Constructing.Add(c);
        }

        internal void CheckTrials()
        {
            /*
            if (!Globals.Register.IsRegistered(TraviModule.Builder))
            {
                foreach(VillageData v in Data.Villages.Values)
                {
                    foreach (Building b in v.Buildings.Values.Union(v.Resources.Values))
                    {
                        if (b.Target > 1)
                            b.Target = 1;
                    }
                }
            }
             */ 
        }
    }
}

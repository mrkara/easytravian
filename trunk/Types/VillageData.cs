using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace EasyTravian
{
    public class VillageData
    {
        public VillageProps Props = new VillageProps();
        public SerializableDictionary<ResourcesType, Production> Productions = new SerializableDictionary<ResourcesType, Production>();
        public SerializableDictionary<int, Building> Resources = new SerializableDictionary<int, Building>();
        public SerializableDictionary<int, Building> Buildings = new SerializableDictionary<int, Building>();
        public List<Construction> Constructing = new List<Construction>();
        public List<BuildingDisplay> CanBuild
        {
            get
            {
                List<BuildingDisplay> list = new List<BuildingDisplay>();

                var res = from b in Resources.Values.Union(Buildings.Values)
                          where b.Level < b.Target
                              //&& b.NextLevelCost < Stock
                          && b.NextLevelCost.Sum != 0
                          orderby b.NextLevelCost.Sum
                          select new { b.Type, b.Level, b.NextLevelCost, buildable = b.NextLevelCost < Stock };

                foreach (var b in res)
                {
                    BuildingDisplay bd = new BuildingDisplay(b.Type, b.Level, b.NextLevelCost, b.buildable);
                    list.Add(bd);
                }

                return list;
            }
        }

        

        public Resources Stock
        {
            get
            {
                return new Resources(Productions[ResourcesType.Lumber].Stock,
                                     Productions[ResourcesType.Clay].Stock,
                                     Productions[ResourcesType.Iron].Stock,
                                     Productions[ResourcesType.Crop].Stock);
            }

        }
        public Resources Capacity
        {
            get
            {
                return new Resources(Productions[ResourcesType.Lumber].Capacity,
                                     Productions[ResourcesType.Clay].Capacity,
                                     Productions[ResourcesType.Iron].Capacity,
                                     Productions[ResourcesType.Crop].Capacity);
            }

        }
        public Resources Producing
        {
            get
            {
                return new Resources(Productions[ResourcesType.Lumber].Producing,
                                     Productions[ResourcesType.Clay].Producing,
                                     Productions[ResourcesType.Iron].Producing,
                                     Productions[ResourcesType.Crop].Producing);
            }

        }

        public VillageData()
        {
            Productions[ResourcesType.Lumber] = new Production();
            Productions[ResourcesType.Lumber].Type = ResourcesType.Lumber;
            Productions[ResourcesType.Lumber].TargetPercent = 30;
            Productions[ResourcesType.Clay] = new Production();
            Productions[ResourcesType.Clay].Type = ResourcesType.Clay;
            Productions[ResourcesType.Clay].TargetPercent = 30;
            Productions[ResourcesType.Iron] = new Production();
            Productions[ResourcesType.Iron].Type = ResourcesType.Iron;
            Productions[ResourcesType.Iron].TargetPercent = 20;
            Productions[ResourcesType.Crop] = new Production();
            Productions[ResourcesType.Crop].Type = ResourcesType.Crop;
            Productions[ResourcesType.Crop].TargetPercent = 20;

            for (int i = 1; i <= 18; i++)
            {
                Resources[i] = new Building();
                Resources[i].Id = i;
                Resources[i].BuildId = i;

            }

            for (int i = 1; i <= 22; i++)
            {
                Buildings[i] = new Building();
                Buildings[i].Id = i;
                Buildings[i].BuildId = i + 18;
            }
            Buildings[21].Type = BuildingType.Rally_point;
            Buildings[21].BuildId = 39;
            Buildings[22].Type = BuildingType.City_wall;
            Buildings[22].BuildId = 40;
        }

        public bool NeedToBuildResource()
        {
            foreach (Building building in Resources.Values)
            {
                if (building.Level < building.Target)
                    return true;
            }
            return false;
        }

        public bool NeedToBuildBuilding()
        {
            foreach (Building building in Buildings.Values)
            {
                if (building.Level < building.Target)
                    return true;
            }
            return false;
        }

        public bool ResourceConstructionInProgress()
        {
            var Res = from c in Constructing
                       where (int)c.Building < 4
                       select c;

            return Res.Count() > 0;
        }

        public bool BuildingConstructionInProgress()
        {
            var Res = from c in Constructing
                       where (int)c.Building > 3
                       select c;

            return Res.Count() > 0;
        }

        public void CleanConstructions()
        {
            int n = 0;
            while (n<Constructing.Count)
            {
                if (Constructing[n].Ends < DateTime.Now)
                    Constructing.RemoveAt(n);
                else
                    n++;
            }
        }

    }
}

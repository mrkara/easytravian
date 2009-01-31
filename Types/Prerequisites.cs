using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyTravian
{
    class BuildingLevel
    {
        public BuildingLevel(BuildingType type, int level)
        {
            Type = type;
            Level = level;
        }

        public BuildingType Type;
        public string Name
        {
            get { return Globals.Translator[Type.ToString()]; }
        }
        public int Level { get; set; }
    }

    class Prerequisites
    {
        public static List<BuildingLevel> Get(BuildingType building)
        {
            List<BuildingLevel> ret = new List<BuildingLevel>();

            switch (building)
            {
                case BuildingType.Sawmill:
                    ret.Add(new BuildingLevel(BuildingType.Woodcutter, 10));
                    ret.Add(new BuildingLevel(BuildingType.Main_building, 5));
                    break;
                case BuildingType.Brickyard:
                    ret.Add(new BuildingLevel(BuildingType.Clay_pit, 10));
                    ret.Add(new BuildingLevel(BuildingType.Main_building, 5));
                    break;
            }

            return ret;
        }
    }
}

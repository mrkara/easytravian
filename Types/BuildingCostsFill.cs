using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyTravian
{
    class BuildingCostsFill
    {
        public static Dictionary<BuildingType, Dictionary<int, Resources>> Fill()
        {
            Dictionary<BuildingType, Dictionary<int, Resources>> ret = new Dictionary<BuildingType, Dictionary<int, Resources>>();

            //ret[BuildingType.Woodcutter][1] = new Resources(40, 100, 50, 60);

            return ret;
        }
    }
}

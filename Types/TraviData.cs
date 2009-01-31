using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml.Serialization;
using System.IO;

namespace EasyTravian
{
    /// <summary>
    /// Minden adat egy hejben
    /// Igazából model-nek kéne hívni
    /// </summary>
    public class TraviData
    {
        
        string ServerPrefix;
        string UserPrefix;

        public SerializableDictionary<int, VillageData> Villages = new SerializableDictionary<int, VillageData>();
        public List<MapElement> Map = new List<MapElement>();
        public SerializableDictionary<Point, TerrainType> Terrain = new SerializableDictionary<Point, TerrainType>();
        [NonSerialized]
        public Overall Overalls;

        public TribeType? Tribe = null;

        public bool MapChanged = false;
        public bool TerrainChanged = false;

        public TraviData()
        {
            ServerPrefix = Config.DataBaseDir + Globals.Cfg.Server + "\\";
            UserPrefix = ServerPrefix + Globals.Cfg.UserName + "\\";

            DirectoryInfo dir = new DirectoryInfo(UserPrefix);
            dir.Create();

            Overalls = new Overall(this);
        }

        public void Load()
        {
            if (File.Exists(UserPrefix + "Villages.xml"))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(SerializableDictionary<int, VillageData>));
                    using (StreamReader rea = new StreamReader(UserPrefix + "Villages.xml"))
                        Villages = (SerializableDictionary<int, VillageData>)ser.Deserialize(rea);
                }
                catch(Exception E)
                {
                    File.Delete(UserPrefix + "Villages.xml");
                    Villages = new SerializableDictionary<int, VillageData>();
                }
            }

            if (File.Exists(ServerPrefix + "Map.xml"))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(List<MapElement>));
                    using (StreamReader rea = new StreamReader(ServerPrefix + "Map.xml"))
                        Map = (List<MapElement>)ser.Deserialize(rea);
                }
                catch
                {
                }
            }

            if (File.Exists(ServerPrefix + "Terrain.xml"))
            {
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(SerializableDictionary<Point, TerrainType>));
                    using (StreamReader rea = new StreamReader(ServerPrefix + "Terrain.xml"))
                        Terrain = (SerializableDictionary<Point, TerrainType>)ser.Deserialize(rea);
                }
                catch
                {
                }
            }
        }

        public void Save()
        {
            if (Villages.Count > 0)
            {
                XmlSerializer ser = new XmlSerializer(typeof(SerializableDictionary<int, VillageData>));
                using (StreamWriter wri = new StreamWriter(UserPrefix + "Villages.xml"))
                    ser.Serialize(wri, Villages);
            }

            if (MapChanged)
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<MapElement>));
                using (StreamWriter wri = new StreamWriter(ServerPrefix + "Map.xml"))
                    ser.Serialize(wri, Map);
                MapChanged = false;
            }

            if (TerrainChanged)
            {
                XmlSerializer ser = new XmlSerializer(typeof(SerializableDictionary<Point, TerrainType>));
                using (StreamWriter wri = new StreamWriter(ServerPrefix + "Terrain.xml"))
                    ser.Serialize(wri, Terrain);
                TerrainChanged = false;
            }

        }

    }

    public class ResourceOverallItem
    {
        public ResourcesType Type { get; set; }
        public int Stock { get; set; }
        public int Producing { get; set; }
        public int Capacity { get; set; }

        public ResourceOverallItem( ResourcesType type )
        {
            Type = type;
        }
    }

    public class Overall
    {
        TraviData data;

        public Overall( TraviData data )
        {
            this.data = data;
        }

        public Dictionary<ResourcesType, ResourceOverallItem> ResourceOverall
        {
            get
            {
                Dictionary<ResourcesType, ResourceOverallItem> ret = new Dictionary<ResourcesType,ResourceOverallItem>(4);

                ret.Add(ResourcesType.Lumber, new ResourceOverallItem(ResourcesType.Lumber));
                ret.Add(ResourcesType.Clay, new ResourceOverallItem(ResourcesType.Clay));
                ret.Add(ResourcesType.Iron, new ResourceOverallItem(ResourcesType.Iron));
                ret.Add(ResourcesType.Crop, new ResourceOverallItem(ResourcesType.Crop));

                foreach (VillageData village in data.Villages.Values)
                {
                    ret[ResourcesType.Lumber].Stock += village.Productions[ResourcesType.Lumber].Stock;
                    ret[ResourcesType.Clay].Stock += village.Productions[ResourcesType.Clay].Stock;
                    ret[ResourcesType.Iron].Stock += village.Productions[ResourcesType.Iron].Stock;
                    ret[ResourcesType.Crop].Stock += village.Productions[ResourcesType.Crop].Stock;

                    ret[ResourcesType.Lumber].Producing += village.Productions[ResourcesType.Lumber].Producing;
                    ret[ResourcesType.Clay].Producing += village.Productions[ResourcesType.Clay].Producing;
                    ret[ResourcesType.Iron].Producing += village.Productions[ResourcesType.Iron].Producing;
                    ret[ResourcesType.Crop].Producing += village.Productions[ResourcesType.Crop].Producing;

                    ret[ResourcesType.Lumber].Capacity += village.Productions[ResourcesType.Lumber].Capacity;
                    ret[ResourcesType.Clay].Capacity += village.Productions[ResourcesType.Clay].Capacity;
                    ret[ResourcesType.Iron].Capacity += village.Productions[ResourcesType.Iron].Capacity;
                    ret[ResourcesType.Crop].Capacity += village.Productions[ResourcesType.Crop].Capacity;
                }

                return ret;
            }
        }
    }

}

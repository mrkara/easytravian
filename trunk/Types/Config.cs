using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Diagnostics;

namespace EasyTravian
{
    public class Config
    {
        public string Server = "Server";
        public string UserName = "UserName";
        [NonSerialized]
        public string PassWord = "Password";
        public int Language = 0;

        public static DirectoryInfo DataBaseDir
        {
            get
            {
                DirectoryInfo di = new DirectoryInfo(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\EasyTravian\\Data\\");
                if (!di.Exists)
                    di.Create();
                return di;
            }
        }

        public static Config LoadConfig()
        {
            string fileName = DataBaseDir + "config.xml";
            if (File.Exists(fileName))
            {
                using (TextReader r = new StreamReader(fileName)) 
                {
                    XmlSerializer ser = new XmlSerializer(typeof(Config));
                    try
                    {
                        return (Config)ser.Deserialize(r);
                    }
                    catch
                    {
                        return new Config();
                    }
                }
            }

            Config cfg = new Config();
            if (Globals.Translator.GetLanguages().Where(l => l.LCID == CultureInfo.CurrentCulture.LCID).Count() > 0)
                cfg.Language = CultureInfo.CurrentCulture.LCID;
            return cfg;
        }

        ~Config()
        {
            string fileName = DataBaseDir + "config.xml";
            using (TextWriter wri = new StreamWriter(fileName))
            {
                XmlSerializer ser = new XmlSerializer(typeof(Config));
                ser.Serialize(wri, this);
            }
        }

        public bool Debugging
        {
            get
            {
                #if DEBUG
                    return true;
                #else
                    return false;
                #endif
            }
        }

    }
}

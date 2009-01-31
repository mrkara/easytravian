using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Management;

namespace EasyTravian
{
    public enum TraviModule
    {
        Builder,
        Map,
        Reports
    }

    public class RegistrationItem
    {
        public DateTime Entered { get; set; }
        public int Module { get; set; }
        public string Key { get; set; }
        public string Code { get; set; }
        public string ModuleName
        {
            get { return Globals.Translator[((TraviModule)Module).ToString()]; }
        }
    }

    class Register
    {
        List<RegistrationItem> items;
        string machineId = "";

        public Register()
        {
            if (File.Exists(Config.DataBaseDir + "register.xml"))
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<RegistrationItem>));
                using (StreamReader sr = new StreamReader(Config.DataBaseDir + "register.xml"))
                    items = (List<RegistrationItem>)ser.Deserialize(sr);
            }
            else
                items = new List<RegistrationItem>();
        }

        ~Register()
        {
            Save();
        }

        public void Save()
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<RegistrationItem>));
            using (StreamWriter sw = new StreamWriter(Config.DataBaseDir + "register.xml"))
                ser.Serialize(sw, items);
        }

        public bool IsValidKeyCodePair(string key, string code )
        {
            return code == CreateCode(key);
        }

        public bool IsRegistered( TraviModule module )
        {
            return true;
            //return CheckRegistration( GenerateKey( module ) );
        }

        private bool CheckRegistration(string key)
        {
            string code = CreateCode(key);

            var lst = from i in items
                      where i.Key == key
                      && i.Code == code
                      select i;

            return lst.Count() > 0;
        }

        public string GenerateKey( TraviModule module )
        {
            string s = "";

            if (machineId == "")
            {
                ManagementClass mc = new ManagementClass("Win32_Processor");
                ManagementObjectCollection mcc = mc.GetInstances();
                foreach (ManagementObject mo in mcc)
                    s += mo.Properties["ProcessorId"].Value.ToString();
                machineId = s;
            }


            return CreateKey(((int)module).ToString() + machineId);
        }

        public List<RegistrationItem> Items
        {
            get
            {
                return items;

            }
        }

        public void DeleteItem(RegistrationItem item)
        {
            items.Remove(item);
            Save();
        }

        public void NewItem(RegistrationItem item)
        {
            items.Add(item);
            Save();
        }

        string CreateKey(string source)
        {
            //return string.Format("{0:X}", source.GetHashCode());
            return createMD5(source);
        }

        private string createMD5(string key)
        {
            byte[] res = MD5.Create().ComputeHash(Encoding.Default.GetBytes(key));

            string s = "";
            foreach (byte b in res)
                //s += string.Format("{0:X}", b);
                s += b.ToString("X2");

            return s;
        }

        string CreateCode(string key)
        {
            byte[] b = Encoding.Default.GetBytes(key);

            b[19] = (byte)'0';
            byte p = b[9];
            if (b.Count() > 29)
            {
                b[9] = b[29];
                b[29] = p;
            }

            string s = Encoding.Default.GetString(b);

            return createMD5(s);
        }
        
    }
}

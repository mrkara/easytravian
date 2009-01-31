using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using System.Globalization;

namespace EasyTravian
{
    public class TransItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    class Translator
    {

        private SerializableDictionary<string, string> dict;

        private int actlang = -1;

        public string this[string input]
        {
            get
            {
                if (actlang != Globals.Cfg.Language)
                    Load();

                string ret = "";
                if (!dict.TryGetValue(input, out ret))
                {
                    Load();
                    if (!dict.TryGetValue(input, out ret))
                    {
                        ret = input;
                        dict.Add(input, input);
                        Save();
                    }
                }
                return ret;
            }
        }

        public string DeTranslate(string s)
        {
            var r = from d in dict
                    where d.Value == s
                    select d.Key;

            if (r.Count() > 0)
                return r.First();
            else
                return s;
        }

        public string GetFileName(int id)
        {
            return Config.DataBaseDir + "lang." + id + ".xml";
        }

        public bool LangExists(CultureInfo ci)
        {
            if (ci == null)
                return false;
            return File.Exists( GetFileName( ci.LCID ));
        }

        public void NewLang(CultureInfo ci)
        {
            if (!LangExists(ci))
            {
                Globals.Cfg.Language = ci.LCID;
                Save();
            }
        }

        public void Load()
        {
            string fn = GetFileName(Globals.Cfg.Language);
            XmlSerializer ser = new XmlSerializer(typeof(SerializableDictionary<string, string>));
            if (File.Exists(fn))
            {
                using (StreamReader sr = new StreamReader(fn))
                    dict = (SerializableDictionary<string, string>)ser.Deserialize(sr);
            }
            else
                dict = new SerializableDictionary<string, string>();
        }

        public void Save()
        {
            string fn = GetFileName(Globals.Cfg.Language);
            XmlSerializer ser = new XmlSerializer(typeof(SerializableDictionary<string, string>));
            using (StreamWriter wr = new StreamWriter(fn))
                ser.Serialize(wr, dict);
        }

        public CultureInfo[] GetLanguages()
        {
            List<CultureInfo> res = new List<CultureInfo>();
            DirectoryInfo di = new DirectoryInfo(Config.DataBaseDir.FullName);
            int LCID;
            foreach (FileInfo fi in di.GetFiles("lang.*.xml"))
            {
                try
                {
                    LCID = int.Parse(fi.Name.Substring(5, fi.Name.Length - 9));
                    res.Add(CultureInfo.GetCultureInfo(LCID));
                }
                catch { }
            }
 
            return res.ToArray();
        }

        public List<TransItem> Items
        {
            get
            {
                List<TransItem> res = new List<TransItem>( dict.Count() );
                foreach (KeyValuePair<string, string> kp in dict)
                {
                    TransItem it = new TransItem();
                    it.Key = kp.Key;
                    it.Value = kp.Value;
                    res.Add(it);
                }
                return res;
            }
            set
            {
                foreach (TransItem item in value)
                {
                    dict[item.Key] = item.Value;
                }
            }
        }

        private void TranslateControls(Control control)
        {
            if (control is Label)
            {
                ((Label)control).Text = this[control.Text];
            }

            if (control is Button)
            {
                ((Button)control).Text = this[control.Text];
            }
            
            if (control is ToolStrip)
            {
                foreach(ToolStripItem it in ((ToolStrip)control).Items)
                    it.Text = this[it.Text];
            }

            if (control is TabControl)
            {
                foreach(TabPage tp in ((TabControl)control).TabPages)
                    tp.Text = this[tp.Text];
            }

            foreach (Control c in control.Controls)
                TranslateControls(c);

        }

        public void TranslateForm(Form form)
        {
            TranslateControls(form);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace EasyTravian
{
    class Globals
    {
        private static Config cfg;

        public static Config Cfg
        {
            get
            {
                if (cfg == null)
                    cfg = Config.LoadConfig();
                return cfg;
            }
        }

        private static WebBrowser web;

        public static WebBrowser Web
        {
            get
            {
                if (web == null)
                {
                    web = new WebBrowser();
                }
                return web;
            }
            set
            {
                web = value;
            }
        }

        private static Translator translator;

        public static Translator Translator
        {
            get
            {
                if (translator == null)
                    translator = new Translator();
                return translator;
            }
        }

        private static Logger logger;

        public static Logger Logger
        {
            get
            {
                if (logger == null)
                    logger = new Logger();
                return logger;
            }
        }

        private static Register register;

        public static Register Register
        {
            get
            {
                if (register == null)
                    register = new Register();
                return register;
            }
        }

    }
}

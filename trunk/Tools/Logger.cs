using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace EasyTravian
{
    enum LogType
	{
        ltReport,
        ltExtended,
        ltDebug
	}

    class Logger
    {
        public bool Active = false;
        public LogType MinLogType = LogType.ltDebug;

        public void Log(string text, LogType type)
        {
            if (Active && type <= MinLogType)
            {
                using( StreamWriter sw = new StreamWriter( Config.DataBaseDir + Globals.Cfg.Server + "\\" + Globals.Cfg.UserName + "\\EasyTravian.log", true ))
                {
                    sw.WriteLine( "[" + DateTime.Now.ToString() + "] " + text );
                }
            }
        }
    }
}

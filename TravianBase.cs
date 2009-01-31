using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Text.RegularExpressions;

namespace EasyTravian
{
    /// <summary>
    /// Legf�bb �zleti oszt�ly
    /// lehet, hogy sz�t is k�ne darabolni
    /// </summary>
    public partial class TravianBase
    {
        private bool pageLoaded;
        public TraviData Data = new TraviData();
        public Dictionary<BuildingType, Dictionary<int, Resources>> BuildingCosts = BuildingCostsFill.Fill();
        private int ActiveVillage;
        bool tryToLogin = false;

        public TravianBase()
        {
            Globals.Web.DocumentCompleted += Web_DocumentCompleted;
            Globals.Logger.MinLogType = LogType.ltDebug;
            Globals.Logger.Active = true;
        }

        /// <summary>
        /// Odainternetezik az oldalra �s meg is v�rja, m�g a b�ng�sz� let�tlt mindent
        /// </summary>
        /// <param name="url"></param>
        private void Navigate(string url)
        {
            if (url != null && url.Length > 0)
            {
                pageLoaded = false;
                if (url == "back")
                    Globals.Web.GoBack();
                else
                    if (url.StartsWith("http://"))
                        Globals.Web.Navigate(url);
                    else
                        Globals.Web.Navigate("http://" + Globals.Cfg.Server + "/" + url);
                Application.DoEvents();
                Thread.Sleep(100);
                //while (web.ReadyState == WebBrowserReadyState.Loading)
                Application.UseWaitCursor = true;
                try
                {
                    while (!pageLoaded)
                    {
                        Application.DoEvents();
                        Thread.Sleep(100);
                    }
                }
                finally
                {
                    Application.UseWaitCursor = false;
                }

            }
        }

        /// <summary>
        /// T�bbfalvas j�t�kn�l, kiv�laszt egy falut
        /// </summary>
        /// <param name="village"></param>
        private void ChangeToVillage(VillageData village)
        {
            //string act = xpath.SelectElement(web.Document, "id('lmid2')/div[1]/h1").InnerText;

            //if (village.Props.Name != act)
            if (village.Props.url == null)
                Navigate("dorf1.php");
            else
                Navigate(village.Props.url);

            ActiveVillage = village.Props.Id;

        }

        /// <summary>
        /// Login adatokat kit�lti �s megnyomja a gombot
        /// akkor ker�l ide a program, ha a navig�l�s eredm�nyek�nt a loginablak j�n be
        /// </summary>
        void DoLogin()
        {
            if (xpath.SetAttribute("id('lmid3')/form/table/tbody/tr/td/table/tbody/tr[1]/td/input", "value", Globals.Cfg.UserName)
               &&
               xpath.SetAttribute("id('lmid3')/form/table/tbody/tr/td/table/tbody/tr[2]/td/input", "value", Globals.Cfg.PassWord))
            {
                HtmlElement el = xpath.SelectElement("id('lmid3')/form/p[2]/input[2]");
                el.InvokeMember("Click");
            }

        }

        public void Login()
        {
            if (Data.Villages.Count > 0)
                Data.Save();
            Data = new TraviData();
            Data.Load();
            //if (TraviBase.Data.Villages.Count == 0)
            try
            {
                Navigate("login.php");
                GetBasicInfo();
            }
            catch 
            {
                MessageBox.Show(Globals.Translator["Login failed!"]);
                Application.Exit();
            }

        }

        /// <summary>
        /// webbrowser component eventje
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (xpath.ElementExists("id('lmid3')/form/table/tbody/tr/td/table/tbody/tr[1]/td/input"))
            {
                if (tryToLogin)
                {
                    //Globals.Web.Stop();
                    pageLoaded = true;
                    throw new Exception("cancel");
                }
                DoLogin();
                tryToLogin = true;
            }
            else
            {
                pageLoaded = true;
                tryToLogin = false;
            }
        }

        public int GetVillageIdFromName(string VillageName)
        {
            var res = from v in Data.Villages.Values
                      where v.Props.Name == VillageName
                      select v.Props.Id;

            if (res.Count() > 0)
                return res.First();
            else
                return -1;
        }

    }

}

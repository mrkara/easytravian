using System;
using System.Threading;
using System.Windows.Forms;

namespace EasyTravian
{
    /// <summary>
    /// Summary description for Class1
    /// </summary>
    public partial class TravianBase
    {
        public void SendMail(string recipient, string subject, string body)
        {
            recipient = recipient.Replace(";", ",");
            string[] traviusers = recipient.Split(',');

            for(int i = 0; i < traviusers.Length; i++)
            {
                Navigate("nachrichten.php?t=1");
                // címzett "id('lmid2')/form/table/tbody/tr[2]/td[3]/input"
                // tárgy: "id('lmid2')/form/table/tbody/tr[3]/td[2]/input"
                // body: id('igm')
                if (xpath.ElementExists("id('lmid2')/form/table/tbody/tr[2]/td[3]/input")) 
                {
                    if (xpath.SetAttribute("id('lmid2')/form/table/tbody/tr[2]/td[3]/input", "value", traviusers[i])
                       &&
                       xpath.SetAttribute("id('lmid2')/form/table/tbody/tr[3]/td[2]/input", "value", subject)
                       &&
                       xpath.SetAttribute("id('igm')", "value", body))
                    {
                        HtmlElement el = xpath.SelectElement("id('lmid2')/form/table/tbody/tr[6]/td/input[2]");
                        el.InvokeMember("Click");

                        Application.DoEvents();
                        Thread.Sleep(3000);
                        //Navigate("dorf1.php");
                        // be kell várni ???
                        //pageLoaded = false;
                        /*
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
                        */
                    }

                }
            }

       }
    }
}
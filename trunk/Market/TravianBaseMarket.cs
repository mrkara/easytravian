using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace EasyTravian
{
    public partial class TravianBase
    {
        public void SendResource(int _lumber, int _clay, int _iron, int _crop, int _x, int _y)
        {
            if (_lumber == 0 && _clay == 0 && _iron == 0 && _crop == 0)
                return;

            // belépés a piacra már a koordinátákkal
            // http://s6.travian.hu/build.php?gid=17&x=-38&y=-143
            StringBuilder sb = new StringBuilder();
            sb.Append("build.php?gid=17");
            sb.Append("&x=");
            sb.Append(_x.ToString());
            sb.Append("&y=");
            sb.Append(_y.ToString());
            // Navigate("http://s6.travian.hu/build.php?gid=17");
            Navigate(sb.ToString());

            // Fa:    id('r1')
            if (_lumber != 0)
                if (!xpath.SetAttribute("id('r1')", "value", _lumber.ToString()))
                    return;
            if (_clay != 0)
                if (!xpath.SetAttribute("id('r2')", "value", _clay.ToString()))
                    return;
            if (_iron != 0)
                if (!xpath.SetAttribute("id('r3')", "value", _iron.ToString()))
                    return;
            if (_crop != 0)
                if (!xpath.SetAttribute("id('r4')", "value", _crop.ToString()))
                    return;

            // X
            // id('lmid2')/form/table[1]/tbody/tr/td[2]/table/tbody/tr[4]/td/span/input[1]
            // Y
            // id('lmid2')/form/table[1]/tbody/tr/td[2]/table/tbody/tr[4]/td/span/input[2]

            // gex rex1 = new Regex(@"dorf[1-2]\.php\?a=\d+&id=\d+&c=(.{3})");
            // MatchCollection matches1 = rex1.Matches(Globals.Web.Document.GetElementById("lmid2").InnerHtml.Replace("&amp;", "&"));

            //if (!xpath.SetAttribute("id('lmid2')/form/table[1]/tbody/tr/td[2]/table/tbody/tr[4]/td/span/input[1]", "value", _x.ToString()))
            //        return;
            //if (!xpath.SetAttribute("id('lmid2')/form/table[1]/tr/td[2]/table/tr[4]/td/span/input[2]", "value", _y.ToString()))
            //        return;
            
            // If curElement.GetAttribute("name").Equals("vb_login_username") Then
            //        curElement.SetAttribute("Value", "My Username")

            // TravianPlus esetén az OK gomb előtt van egy checkbox (Kétszer tegye meg az utat)
            // egyenlőre ezt átugorjuk
            // id('lmid2')/form/p[1]/input[1]
            HtmlElement el = xpath.SelectElement("id('lmid2')/form/p[1]/input[1]");
            if (el.GetAttribute("type") == "checkbox")
            {
                plusEnabled = true;
                el = xpath.SelectElement("id('lmid2')/form/p[2]/input[1]");
            }
            el.InvokeMember("Click");
            Application.DoEvents();

            WaitForBrowser();

            // confirm
            el = xpath.SelectElement("id('lmid2')/form/p[1]/input[1]");
            el.InvokeMember("Click");
            Application.DoEvents();
            // confirm
            el = xpath.SelectElement("id('lmid2')/form/p[1]/input[1]");
            el.InvokeMember("Click");
            Application.DoEvents();

            //Globals.Web.
        }
    }
}

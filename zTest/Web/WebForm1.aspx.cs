using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace zTest.Web
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        //原来代码是读取数据库的，不是这种实现，按你要求写了一个简单的
        protected void Page_Load(object sender, EventArgs e)
        {
            string user_IP = System.Web.HttpContext.Current.Request.UserHostAddress;
            if (user_IP == "::1") user_IP = "127.0.0.1";
            //IP检测
            if (!checkIP(user_IP))
            {
                Response.Write("IP“" + user_IP + "”为非合法IP禁止访问，如有需要，请联系广铁公安局网安处。");
                Response.End();
            }
        }

        bool checkIP(string ip)
        {
            System.Net.IPAddress ipa;
            if (!System.Net.IPAddress.TryParse(ip, out ipa))
                return false;//不是合法的IP地址字符串
            string[] sIP = ip.Split('.');
            if (sIP.Length != 4)
                return false;
            //10.202.196.51这种形式
            if (sIP[0] != "10" && sIP[1] != "202" && sIP[2] != "196")
                return false;
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace xDM.xCommon.xExtensions
{
    public static class MyHttpRequestExt
    {
        /// <summary>
        /// 是否是IE浏览器,返回IE版本号,不是则返回-1
        /// </summary>
        /// <param name="Request"></param>
        /// <param name="browser"></param>
        /// <param name="strVersion"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static int GetIEVersion(this HttpRequest Request, out string browser, out string strVersion, out Double version)
        {
            browser = Request.Browser.Browser;
            strVersion = Request.Browser.Version;
            var bw = browser.ToLower();
            Double.TryParse(strVersion,out version);
            var ver = -1;
            if (bw == "ie" || bw == "internetexplorer")
            {
                var sVer = strVersion.Split('.')[0];
                int.TryParse(sVer, out ver);
            }
            return ver;
        }

        /// <summary>
        /// 获取Request的参数字符串
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public static string GetRequestParametersString(this HttpRequest Request)
        {
            System.IO.Stream sm = Request.InputStream;
            System.IO.StreamReader sr = new System.IO.StreamReader(sm);

            string opration = sr.ReadToEnd();
            return System.Web.HttpUtility.UrlDecode(opration);
        }

        /// <summary>
        /// 获取用户IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIPAddress(this HttpRequest Request)
        {

            string user_IP = string.Empty;
            //if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
            //{
            //    if (System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null)
            //    {
            //        user_IP = System.Web.HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            //    }
            //    else
            //    {
            //        user_IP = System.Web.HttpContext.Current.Request.UserHostAddress;
            //    }
            //}
            //else
            //{
            //    user_IP = System.Web.HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"].ToString();
            //}
            //x-forwarded-for
            user_IP = Request.UserHostAddress;
            return user_IP;
        }
    }
}

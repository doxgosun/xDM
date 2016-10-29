using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace xDM.xCommon
{
    public static class MyPath
    {

        static Regex rWin1 = new Regex(@"\\");
        static Regex rWin2 = new Regex(@"([^\:])/+");

        static Regex rLin1 = new Regex(@"/");
        static Regex rLin2 = new Regex(@"([^\:])\\+");

        /// <summary>
        /// 获取标准Unix格式路径格式,以"/"分割.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetUnixFormatPath(string path)
        {
            if (path == null) return null;
            path = rWin1.Replace(path, @"/");
            path = rWin2.Replace(path, @"$1/");
            return path.Trim();
        }

        /// <summary>
        /// 获取标准windows路径格式,以"\"分割.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetWindowsFormatPath(string path)
        {
            if (path == null) return null;
            path = rLin1.Replace(path, @"\");
            path = rLin2.Replace(path, @"$1\");
            return path.Trim();
        }
    }
}
 
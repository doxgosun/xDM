using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon.xExtensions
{
    public static class MyTimeSpanExt
    {
        /// <summary>
        /// 把时间间隔TimeSpan转换成X天X时X分X秒的形式
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string ToChineseTimeString(this TimeSpan ts)
        {
            if (ts == null) return "null 秒";
            double ss = ts.TotalSeconds;
            if (ss == 0) return "0 秒";
            string time = "";
            double totalTime = ss;
            int DD = (int)(ss / 3600 / 24);
            ss = ss % (3600 * 24);
            int HH = (int)(ss / 3600);
            ss = ss % 3600;
            int MM = (int)(ss / 60);
            ss = ss % 60;
            time = ss.ToString("0.0") + "秒";
            if (MM > 0 || HH > 0 || DD > 0)
            {
                time = MM.ToString() + "分" + time;
            }
            if (HH > 0 || DD > 0)
            {
                time = HH.ToString() + "小时" + time;
            }
            if (DD > 0)
            {
                time = DD.ToString() + "天" + time;
            }
            return time;
        }
    }
}

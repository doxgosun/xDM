using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon.xExtensions
{
    public static class MyLongExt
    {
        /// <summary>
        /// 数值转换成1.04GB格式
        /// </summary>
        /// <param name="l"></param>
        /// <returns></returns>
        public static string ToFormatString(this long l)
        {
            string[] format = new string[] { "B", "KB" ,"MB","GB","TB","PB" };
            int index = 0;
            double db = l;
            while (db > 1024 && index < format.Length)
            {
                db = db / 1024;
                index++;
            }
            return $"{db.ToString("0.00")} {format[index]}";
        }
    }
}

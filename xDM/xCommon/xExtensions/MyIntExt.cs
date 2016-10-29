using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon.xExtensions
{
    public static class MyIntExt
    {
        /// <summary>
        /// int[] 转换成字符串，用指定符号分割
        /// </summary>
        /// <param name="ss"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ToStr(this int[] ss, object c)
        {
            if (ss == null) return null;
            if (ss.Length == 0) return "";
            string re = "";
            foreach (int str in ss)
            {
                re += string.Format("{0}{1}",str,c);
            }
            return re.Substring(0, re.Length - 1);
        }

        /// <summary>
        /// 数值转换成xxPxxGxxMxxKxxB格式
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string ToFormatString(this int i)
        {
            return ((long)i).ToFormatString();
        }
    }
}

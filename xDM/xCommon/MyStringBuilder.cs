using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon
{
    public static class StringBuilderPro
    {
        #region StringBuilderPro
        /// <summary>
        /// 删除前空格
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static StringBuilder TrimStart(System.Text.StringBuilder sb)
        {
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] != ' ')
                {
                    return sb.Remove(0, i);
                }
            }
            return sb;
        }

        /// <summary>
        /// 删除后空格
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static StringBuilder TrimEnd(System.Text.StringBuilder sb)
        {
            for (int i = sb.Length - 1; i > 0; i--)
            {
                if (sb[i] != ' ')
                {
                    return Substring(sb, 0, i);
                }
            }
            return sb;
        }

        /// <summary>
        /// 删除前、后空格
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public static StringBuilder Trim(System.Text.StringBuilder sb)
        {
            return TrimEnd(TrimStart(sb));
        }

        /// <summary>
        /// 返回从左到右第一个指定字符的索引
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int IndexOf(System.Text.StringBuilder sb, char c)
        {
            for (int i = 0; i < sb.Length; i++)
            {
                if (sb[i] == c)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 截取字符串，从开始位置到结束位置并包含结束位置
        /// </summary>
        /// <param name="sb">SB变量</param>
        /// <param name="intStart">开始位置</param>
        /// <param name="intEnd">结束位置</param>
        /// <returns></returns>
        public static StringBuilder Substring(System.Text.StringBuilder sb, int intStart, int intEnd)
        {

            string s = sb.ToString();
            StringBuilder sbT = new StringBuilder();
            sbT.Length = intEnd - intStart + 1;
            for (int i = intStart; i <= intEnd; i++)
            {
                sbT[i - intStart] = sb[i];
            }
            return sbT;
        }

        /// <summary>
        /// 以指定字符分割数组
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static StringBuilder[] Split(System.Text.StringBuilder sb, char c)
        {

            ArrayList al = new ArrayList();
            int intStart = 0, intEnd = 0;
            for (int i = 0; i < sb.Length; i++)
            {
                char cc = sb[sb.Length - 1];
                char ccc = sb[i];
                if (sb[i] == c)
                {
                    intEnd = i - 1;
                    al.Add(Substring(sb, intStart, intEnd));
                    intStart = i + 1;
                }
            }
            if (intStart < sb.Length - 1)
            {
                al.Add(Substring(sb, intStart, sb.Length - 1));
            }
            else if (intStart == sb.Length)
            {
                al.Add(new StringBuilder(""));
            }
            StringBuilder[] sbT = new StringBuilder[al.Count];
            for (int i = 0; i < al.Count; i++)
            {
                sbT[i] = (StringBuilder)al[i];
            }
            return sbT;

        }

        /// <summary>
        /// 以指定字符分割数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static StringBuilder[] Split(string str, char c)
        {

            string[] s = str.Split(c);
            StringBuilder[] sbT = new StringBuilder[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                sbT[i] = new StringBuilder(s[i]);
            }
            return sbT;

        }

        /// <summary>
        /// 把字符串形式的ArrayList转换成字符串数组
        /// </summary>
        /// <param name="al"></param>
        /// <returns></returns>
        public static string[] toStrings(ArrayList al)
        {
            string[] strs = new string[al.Count];
            for (int i = 0; i < al.Count; i++)
            {
                strs[i] = al[i].ToString();
            }
            return strs;
        }


        /// <summary> 
        /// 获取字符串长度，一个汉字算两个字节 
        /// </summary> 
        /// <param name="str"></param> 
        /// <returns></returns> 
        public static int GetLength(string str)
        {
            if (str.Length == 0) return 0;
            ASCIIEncoding ascii = new ASCIIEncoding();
            int tempLen = 0; byte[] s = ascii.GetBytes(str);
            for (int i = 0; i < s.Length; i++)
            {
                if ((int)s[i] == 63)
                {
                    tempLen += 2;
                }
                else
                {
                    tempLen += 1;
                }
            }
            return tempLen;
        }
        #endregion
    }
}

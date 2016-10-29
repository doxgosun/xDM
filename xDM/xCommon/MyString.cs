using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;

namespace xDM.xCommon
{
    public class MyString
    {
        #region 获取标准路径格式,以"/"分割.
        /// <summary>
        /// 获取标准路径格式,以"/"分割.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFormatPath(string path)
        {
            if (path == null) return null;
            path = new Regex(@"\\").Replace(path, @"/");
            path = new Regex(@"([^\:])/+").Replace(path, @"$1/");
            return path.Trim();
        }
        #endregion

        #region 转换汉字为拼音
        /// <summary>
        /// 转换成拼音
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPinyin(string input)
        {
            return PinYin.ToPinyin(input);
        }
        public static string ToPinyin(string input, string split)
        {
            return PinYin.ToPinyin(input, split);
        }
        /// <summary>
        /// 转换成带音调的拼音
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPin1Yin1(string input)
        {
            return PinYin.ToPin1Yin1(input);
        }
        public static string ToPin1Yin1(string input, string split)
        {
            return PinYin.ToPin1Yin1(input, split);
        }
        /// <summary>
        /// 转换成声母缩写
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToPinyinInitials(string input)
        {
            return PinYin.ToPinyinInitials(input);
        }
        public static string ToPinyinInitials(string input, string split)
        {
            return PinYin.ToPinyinInitials(input, split);
        }
        /// <summary>
        /// 转换成拼音缩写
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToShortPinyinInitials(string input)
        {
            return PinYin.ToShortPinyinInitials(input);
        }
        public static string ToShortPinyinInitials(string input, string split)
        {
            return PinYin.ToShortPinyinInitials(input, split);
        }
        #endregion

        #region 用指定字符分割指定数组
        public static string ToStr(StringBuilder[] sb, char c)
        {
            return ToStr(sb, c.ToString());
        }
        public static string ToStr(StringBuilder[] sb, string c)
        {
            if (sb == null) return null;
            string re = "";
            foreach (StringBuilder str in sb)
            {
                re += str + c;
            }
            return re.Substring(0, re.Length - 1);
        }
        public static string ToStr(string[] s, char c)
        {
            return ToStr(s, c.ToString());
        }
        public static string ToStr(string[] s, string c)
        {
            if (s == null) return null;
            string re = "";
            foreach (string str in s)
            {
                re += str + c;
            }
            return re.Substring(0, re.Length - 1);
        }

        public static string ToStr(int[] ss, char c)
        {
            if (ss == null) return null;
            if (ss.Length == 0) return "";
            string re = "";
            foreach (int str in ss)
            {
                re += str.ToString() + c.ToString();
            }
            return re.Substring(0, re.Length - 1);
        }
        #endregion

        #region 把时间间隔TimeSpan转换成X天X时X分X秒的形式
        public static string GetTimeString(TimeSpan ts)
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
        #endregion

        #region 序列化和反序列化对象
        /// <summary>
        /// 序列化类对象
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        /// <param name="t">类实例</param>
        /// <returns>类的序列化字符串</returns>
        public static string Serializable<T>(T t)
        {
            if (t == null) return "";
            IFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            byte[] b;
            formatter.Serialize(ms, t);
            ms.Position = 0;
            b = new byte[ms.Length];
            ms.Read(b, 0, b.Length);
            ms.Close();
            ms.Dispose();
            return Convert.ToBase64String(b);
        }

        /// <summary>
        /// 反序列化类对象
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        /// <param name="t">类的序列化字符串</param>
        /// <returns>类实例</returns>
        public static T DeDeserialize<T>(string t)
        {
            byte[] BytArray = Convert.FromBase64String(t);
            IFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            ms.Write(BytArray, 0, BytArray.Length);
            ms.Position = 0;
            T obj = (T)formatter.Deserialize(ms);
            return obj;
        }
        #endregion
    }
}

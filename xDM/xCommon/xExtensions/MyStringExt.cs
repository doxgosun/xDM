using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace xDM.xCommon.xExtensions
{
    public static class MyStringExt
    {
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="s"></param>
        /// <returns></returns>
        public static T DeDeserialize<T>(this string t)
        {
            byte[] BytArray = Convert.FromBase64String(t);
            IFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            ms.Write(BytArray, 0, BytArray.Length);
            ms.Position = 0;
            T obj = (T)formatter.Deserialize(ms);
            return obj;
        }

        public static T JsonDeserialize<T>(this string s)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(s));
            T obj = (T)ser.ReadObject(ms);
            return obj;
        }

        /// <summary>
        /// Json 字符串 转换为 DataTable数据集合
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this string json)
        {
            DataTable dataTable = new DataTable();  //实例化
            DataTable result;
            try
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue; //取得最大数值
                ArrayList arrayList = javaScriptSerializer.Deserialize<ArrayList>(json);
                if (arrayList.Count > 0)
                {
                    foreach (Dictionary<string, object> dictionary in arrayList)
                    {
                        if (dictionary.Keys.Count<string>() == 0)
                        {
                            result = dataTable;
                            return result;
                        }
                        if (dataTable.Columns.Count == 0)
                        {
                            foreach (string current in dictionary.Keys)
                            {
                                dataTable.Columns.Add(current, dictionary[current].GetType());
                            }
                        }
                        DataRow dataRow = dataTable.NewRow();
                        foreach (string current in dictionary.Keys)
                        {
                            dataRow[current] = dictionary[current];
                        }

                        dataTable.Rows.Add(dataRow); //循环添加行到DataTable中
                    }
                }
            }
            catch
            {
            }
            result = dataTable;
            return result;
        }


        public static string[] Distinct(this string[] a)
        {
            return a.Distinct();
        }

        static Regex regNum = new Regex(@"\d+");
        public static bool IsNumbers(this string s)
        {
            var m = regNum.Match(s);
            if (m.Success)
            {
                if (m.Value == s)
                    return true;
            }
            return false;
        }

        public static bool HasNumbers(this string s)
        {
            var m = regNum.Match(s);
            return m.Success;
        }

        /// <summary>
        /// 以指定字符分割数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static StringBuilder[] SplitToStringBuilders(this string str, char c)
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
        /// 获取字符串长度，一个汉字算两个字节 
        /// </summary> 
        /// <param name="str"></param> 
        /// <returns></returns> 
        public static int GetLength(this string str)
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

        #region 用指定字符分割指定数组

        /// <summary>
        /// 用指定字符组合string[]
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string ToStr(this string[] s, object c)
        {
            if (s == null) return null;
            string ss = c + "";
            return s.ToStr(ss);
        }
        public static string ToStr(this string[] s, string ss)
        {
            if (s == null) return null;
            StringBuilder sb = new StringBuilder();
            foreach (string str in s)
            {
                sb.AppendFormat(str);
                sb.Append(ss);
            }
            if (sb.Length > 1)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xData.xClient.Extensions
{
    public static class MyExtensions
    {
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
            for (int i = 0; i < s.Length; i++)
            {
                var str = s[i];
                if (str + "" != "")
                {
                    sb.Append(str);
                    sb.Append(ss);
                }
            }
            //foreach (string str in s)
            //{
            //    sb.AppendFormat(str);
            //    sb.Append(ss);
            //}
            if (sb.Length > 1)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }
        #endregion

        /// <summary>
        /// 获取参数类型列表
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static Type[] GetDelegateParameterTypes(this Type d)
        {
            if (d.BaseType != typeof(MulticastDelegate))
            {
                throw new InvalidOperationException("Not a delegate.");
            }

            MethodInfo invoke = d.GetMethod("Invoke");
            if (invoke == null)
            {
                throw new InvalidOperationException("Not a delegate.");
            }

            ParameterInfo[] parameters = invoke.GetParameters();
            Type[] typeParameters = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                typeParameters[i] = parameters[i].ParameterType;
            }

            return typeParameters;
        }

        /// <summary>
        /// 获取字符的转义字符串形式
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static string GetString(this char c)
        {
            switch (c)
            {
                case '\0': return null;
                case '\a': return @"\a";
                case '\b': return @"\b";
                case '\f': return @"\f";
                case '\r': return @"\r";
                case '\n': return @"\n";
                case '\t': return @"\t";
                case '\v': return @"\v";
                default:
                    return c.ToString();
            }
        }
        /// <summary>
        /// 获取转义字符串的字符，如：字符串"\t" 返回 字符'\t'
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static char GetChar(this string str)
        {
            if (str == null || str == "") return '\0';
            switch (str)
            {
                case @"\a": return '\a';
                case @"\b": return '\b';
                case @"\f": return '\f';
                case @"\r": return '\r';
                case @"\n": return '\n';
                case @"\t": return '\t';
                case @"\v": return '\v';
                default:
                    if (str.Length > 1)
                    {
                        return str[1];
                    }
                    else
                    {
                        return str[0];
                    }
            }
        }

        /// <summary>
        /// 通过给定的文件流，判断文件的编码类型
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static System.Text.Encoding GetEncoding(this FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
            System.Text.Encoding reVal = System.Text.Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i = 100000;
            if (fs.Length < 100000)
            {
                if (!int.TryParse(fs.Length.ToString(), out i))
                {
                    i = 0;
                }
            }
            byte[] ss = r.ReadBytes(i);
            if (ss.IsUTF8Bytes())
            {
                reVal = System.Text.Encoding.UTF8;
            }
            else if (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
            {
                reVal = new UTF8Encoding(false);
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = System.Text.Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = System.Text.Encoding.Unicode;
            }
            r.Close();
            return reVal;

        }
        /// <summary> 
        /// 判断是否是不带 BOM 的 UTF8 格式 
        /// </summary> 
        /// <param name="data"></param> 
        /// <returns></returns> 
        public static bool IsUTF8Bytes(this byte[] data)
        {
            int charByteCounter = 1; //计算当前正分析的字符应还有的字节数 
            byte curByte; //当前分析的字节. 
            for (int i = 0; i < data.Length; i++)
            {
                curByte = data[i];
                if (charByteCounter == 1)
                {
                    if (curByte >= 0x80)
                    {
                        //判断当前 
                        while (((curByte <<= 1) & 0x80) != 0)
                        {
                            charByteCounter++;
                        }
                        //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X 
                        if (charByteCounter == 1 || charByteCounter > 6)
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    //若是UTF-8 此时第一位必须为1 
                    if ((curByte & 0xC0) != 0x80)
                    {
                        return false;
                    }
                    charByteCounter--;
                }
            }
            if (charByteCounter > 1)
            {
                //throw new Exception("非预期的byte格式");
                return false;
            }
            return true;
        }


    }
}

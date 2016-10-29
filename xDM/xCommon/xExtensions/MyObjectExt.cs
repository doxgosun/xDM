using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;

namespace xDM.xCommon.xExtensions
{
    public static class MyObjectExt
    {
        /// <summary>
        /// 序列化类对象
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        /// <param name="t">类实例</param>
        /// <returns>类的序列化字符串</returns>
        public static string Serializable(this object t)
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

        public static byte[] SerializeToByte(this object obj)
        {
            if (obj == null)
                return null;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;
            byte[] bytes = new byte[ms.Length];
            ms.Read(bytes, 0, bytes.Length);
            ms.Close();
            return bytes;
        }

        public static string JsonSerializer(this object t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(t.GetType());
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string json = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            ms.Dispose();
            return json;
        }

        #region Clone() 克隆对象
        public static T RealCopy<T>(this T t)
        {
            return (T)CloneObj(t);
        }

        private static object CloneObj(object t)
        {
            if (t is ICloneable)
            {
                //return ((ICloneable)t).Clone();
            }
            if (t == null) return null;
            object obj;
            Type type = t.GetType();
            if (type.IsValueType)
            {
                obj = t;
            }
            else if (type == typeof(string))
            {
                obj = t;
            }
            else if (type == typeof(DataTable))
            {
                DataTable dt = t as DataTable;
                obj = dt.Copy();
            }
            else if (type.Namespace == typeof(System.Text.Encoding).Namespace)
            {
                obj = t;
            }
            else if (type.Namespace == typeof(System.Globalization.Calendar).Namespace)
            {
                obj = t;
            }
            else if (type.IsArray)
            {
                Array at = t as Array;
                Array array = Array.CreateInstance(type.GetElementType(), at.Length);
                for (int i = 0; i < at.Length; i++)
                {
                    object o = at.GetValue(i);
                    array.SetValue(CloneObj(o), i);
                }
                //at.CopyTo(obj as Array,0);
                obj = array;
            }
            else
            {
                //obj = type.InvokeMember("", System.Reflection.BindingFlags.CreateInstance, null, t, null);
                obj = System.Activator.CreateInstance(type);
                MemberInfo[] memberInfos = type.GetMembers();
                int len = memberInfos.Length;
                if (len > 0)
                {
                    for (int i = 0; i < len; i++)
                    {
                        MemberInfo memberInfo = memberInfos[i];
                        if (memberInfo.MemberType == MemberTypes.Field)
                        {
                            FieldInfo fi = memberInfo as FieldInfo;
                            if (!fi.IsInitOnly)
                            {
                                try
                                {
                                    object value = fi.GetValue(t);
                                    value = CloneObj(value);
                                    fi.SetValue(obj, value);
                                }
                                catch { }
                            }
                        }
                        else if (memberInfo.MemberType == MemberTypes.Property)
                        {
                            PropertyInfo pi = memberInfo as PropertyInfo;
                            try
                            {
                                ParameterInfo[] p = pi.GetIndexParameters();
                                object value = null;
                                if (p.Length == 0)
                                {
                                    if (pi.CanRead && pi.CanWrite)
                                    {
                                        try
                                        {
                                            value = pi.GetValue(t, null);
                                            value = CloneObj(value);
                                        }
                                        catch { }
                                        pi.SetValue(obj, value, null);
                                    }
                                }
                                else
                                {
                                    IList tl = t as IList;
                                    IList ol = obj as IList;
                                    for (int x = 0; x < tl.Count; x++)
                                    {
                                        if (pi.CanRead && pi.CanWrite)
                                        {
                                            value = pi.GetValue(t, new object[] { x });
                                            value = CloneObj(value);
                                            ol.Insert(x, value);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                }
            }
            return obj;
        }
        #endregion
    }
}

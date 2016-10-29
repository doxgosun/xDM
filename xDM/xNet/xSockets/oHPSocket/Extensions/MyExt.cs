using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xNet.xSockets.oHPSocket.Extensions
{
    public static class MyExt
    {
        public static Array CopyTo(this Array sourceArray, Array targetArray, int index, int length)
        {
            for (int i = 0; i < length; i++)
            {
                targetArray.SetValue(sourceArray.GetValue(i + index), i);
            }
            return targetArray;
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static T DeDeserialize<T>(this string t)
        {
            return Convert.FromBase64String(t).DeDeserialize<T>();
        }
        public static T DeDeserialize<T>(this byte[] bytes)
        {
            MemoryStream ms = new MemoryStream(bytes);
            ms.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            T obj = (T)formatter.Deserialize(ms);
            ms.Close();
            return obj;
        }
    }
}

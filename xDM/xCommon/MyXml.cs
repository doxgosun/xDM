using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace xDM.xCommon
{
    public class MyXml
    {

        #region 静态方法
        public static string XmlSerialize<T>(T t)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(ms, new UTF8Encoding(false));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            ser.Serialize(writer, t, ns);
            writer.Close();
            return System.Text.Encoding.UTF8.GetString(ms.ToArray());
        }
        public static T XmlDeserialize<T>(string s)
        {
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(s);
            XmlNodeReader reader = new XmlNodeReader(xdoc.DocumentElement);
            XmlSerializer ser = new XmlSerializer(typeof(T));
            return (T)ser.Deserialize(reader);
        }
        #endregion
    }
}

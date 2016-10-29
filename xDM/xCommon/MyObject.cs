using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using xDM.xCommon.xExtensions;

namespace xDM.xCommon
{
    public class MyObject
    {
        /// <summary>
        /// 反序列化类对象
        /// </summary>
        /// <typeparam name="T">类名</typeparam>
        /// <param name="t">类的序列化字符串</param>
        /// <returns>类实例</returns>
        public static T DeDeserialize<T>(string t)
        {
            return t.DeDeserialize<T>();
        }
    }
}

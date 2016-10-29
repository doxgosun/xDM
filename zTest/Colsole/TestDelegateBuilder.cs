using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDM.xReflection;
using System.Data.SqlClient;
using System.Diagnostics;
using xDM.xCommon.xExtensions;

namespace zTest.Colsole
{
    class TestDelegateBuilder
    {
        public static void Test()
        {
            Stopwatch sw = new Stopwatch();
            var c = typeof(SqlCommand).GetConstructors()[3];
            var loop = 1000 * 1000 * 1;
            string tmp;
            sw.Restart();
            for (int i = 0; i < loop; i++)
            {
                KeyValuePair<Type, string> kv = new KeyValuePair<Type, string>(c.ReflectedType, c + false.ToString());
               // var key = new KeyValuePair<Type, string>(field.ReflectedType, field + isSetter.ToString())
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            var cotrs = typeof(SqlCommand).GetConstructors();
            cotrs = typeof(SqlCommand).GetConstructors();
            Console.WriteLine();
            var type = typeof(test);
            var l = type.SerializeToByte();
            Console.WriteLine(type.GetConstructors()[0].GetType());
            Console.WriteLine();
            Console.WriteLine(type.GetHashCode());
            Console.WriteLine(typeof(test).GetHashCode());
            Console.WriteLine();
            var t1 = new test();
            var t2 = new test();
            Console.WriteLine(t1.GetHashCode());
            Console.WriteLine(t2.GetHashCode());

            var isSetter = false;

            var field = type.GetField("field");
            var len = field.SerializeToByte();
            len = (field + isSetter.ToString()).SerializeToByte();
            sw.Restart();
            for (int i = 0; i < loop; i++)
            {
                //KeyValuePair<Type, string> kv = new KeyValuePair<Type, string>(c.ReflectedType, c + false.ToString());
                var key = new KeyValuePair<Type, string>(field.ReflectedType, field + isSetter.ToString());
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            var property = type.GetProperty("property");
            var method = type.GetMethod("method");
            var menbers = type.GetMembers();
            var setField = DelegateBuilder.GetSetterDelegate<Action<test,string>>(field);
            var setProperty = DelegateBuilder.GetSetterDelegate<Action<test,string>>(property);
            var delMethod = DelegateBuilder.GetMethodDelegate(method);
            var ccccc = DelegateBuilder.GetCreateDelegate(typeof(SqlCommand));
            var cmd = ccccc.DynamicInvoke();
            var creater = DelegateBuilder.GetCreateDelegate<Func<test>>(typeof(test));
            test t = creater();
            var getField = DelegateBuilder.GetGetterDelegate<Func<test,string>>(field);

            sw.Restart();
            for (int i = 0; i < loop; i++)
            {
                getField = DelegateBuilder.GetGetterDelegate(field) as Func<test,string>;
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            //field.SetValue(null, "qqqqqqqqqqqq");
            var getProperty = DelegateBuilder.GetGetterDelegate(property);
            sw.Restart();
            for (int i = 0; i < loop; i++)
            {
                getProperty = DelegateBuilder.GetGetterDelegate(property);
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            var ff = getField(t);
            //    var pp = getProperty();
            var pp = getProperty.DynamicInvoke(t);
            setField(t,"ffff");
            setProperty(t,"dddddd");
        }

        public class test
        {
            public test() { }
            public  string field = "ddddddddd";
            public  string property { get; set; } = "fffffffff";
            public string method()
            {
                return "";
            }
        }
    }
}

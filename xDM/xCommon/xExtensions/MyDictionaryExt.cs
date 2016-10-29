using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon.xExtensions
{ 
    public static class MyDictionaryExt
    {
        public static Dictionary<T, U> Copy<T, U>(this Dictionary<T, U> source)
        {
            if (source == null) return null;
            Dictionary<T, U> target = new Dictionary<T, U>();
            List<T> Keys = new List<T>(source.Keys);
            foreach (T key in Keys)
            {
                if (source.ContainsKey(key))
                {
                    T key2;
                    if (key is ICloneable) key2 = (T)((ICloneable)key).Clone();
                    else key2 = key;
                    U value2;
                    if (source[key] is ICloneable) value2 = (U)((ICloneable)source[key]).Clone();
                    else value2 = source[key];
                    target.Add(key2, value2);
                }
            }
            return target;
        }
    }
}

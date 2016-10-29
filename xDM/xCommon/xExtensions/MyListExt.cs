using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon.xExtensions
{
    public static class MyListExt
    {
        /// <summary>
        /// 从a中移除b
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static string[] Except(this List<string> a, List<string> b)
        {
            List<string> list = a.ToArray().ToList();
            for (int i = 0; i < list.Count; i++)
            {
                bool bolExcept = false;
                for (int j = 0; j < b.Count; j++)
                {
                    if (list[i] == b[j])
                    {
                        bolExcept = true;
                        b.RemoveAt(j--);
                    }
                }
                if (bolExcept)
                {
                    list.RemoveAt(i--);
                }
            }
            return list.ToArray();
        }
        /// <summary>
        /// 去除重复项
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static string[] Distinct(this List<string> a)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            for (int i = 0; i < a.Count; i++)
            {
                if (!list.ContainsKey(a[i]))
                {
                    list.Add(a[i], a[i]);
                }
            }
            return list.Keys.ToArray();
        }
    }
}

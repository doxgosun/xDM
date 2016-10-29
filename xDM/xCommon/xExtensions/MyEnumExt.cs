using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon.xExtensions
{
    public static class MyEnumExt
    {
        public static string GetName(this Enum en)
        {
            return Enum.GetName(en.GetType(), en);
        }

        public static string[] GetAllNames(this Enum en)
        {
            return Enum.GetNames(en.GetType());
        }
    }
}

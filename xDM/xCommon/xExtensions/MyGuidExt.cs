using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xDM.xCommon.xExtensions
{
    public static class MyGuidExt
    {
        public static string ToStringN(this Guid guid)
        {
            return guid.ToString("N");
        }
    }
}

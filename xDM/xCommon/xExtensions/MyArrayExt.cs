using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xDM.xCommon.xExtensions;

namespace xDM.xCommon.xExtensions
{
    public static class MyArrayExt
    {
        public static Array CopyTo(this Array sourceArray, Array targetArray, int index, int length)
        {
            for (int i = 0; i < length ; i++) 
            {
                targetArray.SetValue(sourceArray.GetValue(i + index),i);
            }
            return targetArray;
        }
    }
}

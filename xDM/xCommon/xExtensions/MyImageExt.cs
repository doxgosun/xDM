using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace xDM.xCommon.xExtensions
{
    static public class MyImageExt
    {
        /// <summary>
        /// 转换成指定格式的二进制
        /// </summary>
        /// <param name="img"></param>
        /// <param name="format">为null则默认为jpeg</param>
        /// <returns></returns>
        public static byte[] ToByteArray(this Image img, System.Drawing.Imaging.ImageFormat format)
        {
            byte[] bt = null;
            if (img == null) return bt;
            if (format == null) format = System.Drawing.Imaging.ImageFormat.Jpeg;
            using (MemoryStream ms = new MemoryStream())
            {
                Bitmap bmp = new Bitmap(img);
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                bt = new byte[ms.Length];
                ms.Read(bt,0,Convert.ToInt32(bt.Length));
                return bt;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace xDM.xCommon.xExtensions
{
    public static class MyFileStreamExt
    {
        /// <summary>
        /// 通过给定的文件流，判断文件的编码类型
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static System.Text.Encoding GetEncoding(this FileStream fs)
        {
            byte[] Unicode = new byte[] { 0xFF, 0xFE, 0x41 };
            byte[] UnicodeBIG = new byte[] { 0xFE, 0xFF, 0x00 };
            byte[] UTF8 = new byte[] { 0xEF, 0xBB, 0xBF }; //带BOM 
            System.Text.Encoding reVal = System.Text.Encoding.Default;

            BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
            int i = 100000;
            if (fs.Length < 100000)
            {
                if (!int.TryParse(fs.Length.ToString(), out i))
                {
                    i = 0;
                }
            }
            byte[] ss = r.ReadBytes(i);
            if (ss.IsUTF8Bytes())
            {
                reVal = System.Text.Encoding.UTF8;
            }
            else if (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
            {
                reVal = new UTF8Encoding(false);
            }
            else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
            {
                reVal = System.Text.Encoding.BigEndianUnicode;
            }
            else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
            {
                reVal = System.Text.Encoding.Unicode;
            }
            r.Close();
            return reVal;

        }
    }
}

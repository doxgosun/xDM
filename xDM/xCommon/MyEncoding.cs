using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using xDM.xCommon.xExtensions;

namespace xDM.xCommon
{
    public class MyEncoding
    {
        /// <summary> 
        /// 给定文件的路径，读取文件的二进制数据，判断文件的编码类型 
        /// </summary> 
        /// <param name="FILE_NAME">文件路径</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetEncoding(string FILE_NAME)
        {
            using (FileStream fs = new FileStream(FILE_NAME, FileMode.Open, FileAccess.Read))
            {
                System.Text.Encoding encoding = GetEncoding(fs);
                fs.Close();
                return encoding;
            }
        }

        /// <summary> 
        /// 通过给定的文件流，判断文件的编码类型 
        /// </summary> 
        /// <param name="fs">文件流</param> 
        /// <returns>文件的编码类型</returns> 
        public static System.Text.Encoding GetEncoding(FileStream fs)
        {
            return fs.GetEncoding();
        }


        /// <summary> 
        /// 判断是否是不带 BOM 的 UTF8 格式 
        /// </summary> 
        /// <param name="data"></param> 
        /// <returns></returns> 
        public static bool IsUTF8Bytes(byte[] data)
        {
            return data.IsUTF8Bytes();
        }
    }
}

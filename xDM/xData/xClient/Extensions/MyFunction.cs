using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xData.xClient.Extensions
{
    public class MyFunction
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
                System.Text.Encoding encoding = fs.GetEncoding();
                fs.Close();
                return encoding;
            }
        }
    }

}

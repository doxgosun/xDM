using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace xDM.xCommon.oFiles
{
    public class Text
    {
        #region "GetFileCount()------ 取得文本文件行数"
        /// <summary>
        /// 取得文本文件行数
        /// </summary>
        /// <param name="strFilePath">文件路径</param>
        /// <returns></returns>
        public static long GetFileCount(string strFilePath)
        {
            try
            {
                if (!System.IO.File.Exists(strFilePath)) return -1;
                long lngFileCount = 0;

                using (FileStream fs = new FileStream(strFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (sr.ReadLine() != null) lngFileCount++;
                    }
                }

                return lngFileCount;
            }
            catch (Exception err) { throw err; }
        }

        /// <summary>
        /// 取得文本文件行数
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <param name="notNullCount">非空白行数</param>
        /// <returns></returns>
        public static long GetFileCount(string strFilePath, out long notNullCount)
        {
            try
            {
                notNullCount = -1;
                if (!System.IO.File.Exists(strFilePath)) return -1;
                long lngFileCount = 0;
                notNullCount = 0;
                string line = "";
                using (FileStream fs = new FileStream(strFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line != "") notNullCount++;
                            lngFileCount++;
                        }
                    }
                }

                return lngFileCount;
            }
            catch (Exception err) { throw err; }
        }
        #endregion  


        /// <summary>
        /// 保存字符到Utf8文件
        /// </summary>
        /// <param name="filePath">文件及完整路径</param>
        /// <param name="content">要保存的内容</param>
        /// <param name="append">false覆盖文件，true添加到文件末尾</param>
        /// <param name="encoding">文件编码，默认为无 bom UTF8</param>
        /// <returns></returns>
        public static void SaveStringToFile(string filePath, string content, bool append, Encoding encoding)
        {
            if (encoding == null)
            {
                encoding = new UTF8Encoding(false);
            }
            using (StreamWriter sw = new StreamWriter(filePath, append, encoding))
            {
                sw.WriteLine(content);
            }
        }
    }
}

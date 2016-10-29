using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using xDM.xCommon.xExtensions;
using System.Security.Cryptography;

namespace xDM.xCommon
{
    public class MyFile
    {
        public static string GetMD5(string fileName)
        {
            var md5 = new MD5CryptoServiceProvider();
            StringBuilder sb = new StringBuilder();
            byte[] retVal = null;
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                retVal = md5.ComputeHash(fs);
            }
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static string GetMD5WithBuff(string filePath)
        {
            int bufferSize = 1024 * 1024 * 1; // 缓冲区大小，1MB
            byte[] buff = new byte[bufferSize];
            var md5 = new MD5CryptoServiceProvider();
            md5.Initialize();
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long offset = 0;
                while (offset < fs.Length)
                {
                    long readSize = bufferSize;
                    if (offset + readSize > fs.Length)
                    {
                        readSize = fs.Length - offset;
                    }
                    fs.Read(buff, 0, Convert.ToInt32(readSize)); // 读取一段数据到缓冲区
                    if (offset + readSize < fs.Length) // 不是最后一块
                    {
                        md5.TransformBlock(buff, 0, Convert.ToInt32(readSize), buff, 0);
                    }
                    else // 最后一块
                    {
                        md5.TransformFinalBlock(buff, 0, Convert.ToInt32(readSize));
                    }
                    offset += bufferSize;
                }
            }
            byte[] result = md5.Hash;
            md5.Clear();
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < result.Length; i++)
            {
                sb.Append(result[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}

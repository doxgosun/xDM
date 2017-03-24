using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xCommon.oFiles
{
    static class Extension
    {
        public static byte[] ToByteArray(this Stream sem)
        {
            long position = sem.Position;
            byte[] bytes = new byte[sem.Length];
            sem.Seek(0, SeekOrigin.Begin);
            sem.Read(bytes, 0, bytes.Length);
            sem.Position = position;
            return bytes;
        }
    }
}

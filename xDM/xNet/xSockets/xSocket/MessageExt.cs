using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using xDM.xNet.xSockets.xSocket.Extensions;

namespace xDM.xNet.xSockets.xSocket
{
    public static class MessageExt
    {

        public static byte[] ToSendByte(this Message msg)
        {
            if (msg == null) return null;
            var bytes = msg.SerializeToByte();
            var len = bytes.Length;
            if (len > Math.Pow(2, 31))
                throw new Exception("对象大小超过2G!");
            byte[] sendBytes = new byte[len + 4];
            sendBytes[0] = (Byte)((len & 0x7f000000) >> 24);
            sendBytes[1] = (Byte)((len & 0xff0000) >> 16);
            sendBytes[2] = (Byte)((len & 0xff00) >> 8);
            sendBytes[3] = (Byte)(len & 0xff);
            Array.Copy(bytes, 0, sendBytes, 4, len);
            return sendBytes;
        }

        //以下静态方法
        #region 静态方法

        public static Message GetMessage(byte[] SerializableBytes)
        {
            if (SerializableBytes == null || SerializableBytes.Length == 0) return null;
            try
            {
                return SerializableBytes.DeDeserialize<Message>();
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}

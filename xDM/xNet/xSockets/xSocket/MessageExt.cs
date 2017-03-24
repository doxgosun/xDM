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
            //包格式：（包头-6字节48位）（包体）
            //包格式：（1111 1111）（00）（10 1111 ... 1111）（1011）（11。。00011）
            //包说明：开始标志0xff 包类型       包长度         校验       数据体  
            //其中：开始标志 8位：固定为0xff
            //      包类型   2位：00 Message 01文件 10数据 11心跳,心跳包包长度为0，并且没有数据体
            //      包长度  30位：最大为1G 数据类型的包包长度表示数据大小，文件包表示文件编号
            //      校验     8位：b[5] = (b[1]+b[2]+b[3]+b[4])/4
            if (msg == null) return null;
            var bytes = msg.SerializeToByte();
            var len = bytes.Length;
            if (len > Math.Pow(2, 30))
                throw new Exception("对象大小超过1G!");
            byte[] sendBytes = new byte[len + 6];
            sendBytes[0] = 0xff;
            sendBytes[1] = (byte)((len & 0x3f000000) >> 24);
            sendBytes[2] = (byte)((len & 0xff0000) >> 16);
            sendBytes[3] = (byte)((len & 0xff00) >> 8);
            sendBytes[4] = (byte)(len & 0xff);
            sendBytes[5] = (byte)((sendBytes[1] + sendBytes[2] + sendBytes[3] + sendBytes[4]) / 4);
            Array.Copy(bytes, 0, sendBytes, 6, len);
            return sendBytes;
        }

        //以下静态方法
        #region 静态方法

        public static Message GetMessage(byte[] SerializableBytes)
        {
            if (SerializableBytes == null || SerializableBytes.Length == 0)
                return null;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDM.xNet.xSockets.oHPSocket.Extensions;

namespace xDM.xNet.xSockets.oHPSocket
{
    public static class MessageExt
    {
        public static string ToSendString(this Message msg)
        {
            return msg.Serializable();
        }

        public static byte[] ToSendByte(this Message msg)
        {
            return msg.SerializeToByte();
        }

        //以下静态方法
        #region 静态方法
        public static Message GetMessage(string SerializableString)
        {
            if (SerializableString == null || SerializableString == "") return null;
            try
            {
                return SerializableString.DeDeserialize<Message>();
            }
            catch
            {
                return null;
            }
        }
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

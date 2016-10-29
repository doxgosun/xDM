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
        public static string ToSendString(this Message msg)
        {
            return $"<Message>{msg.Serializable()}</Message>";
        }

        public static byte[] ToSendByte(this Message msg)
        {
            return Encoding.UTF8.GetBytes(msg.ToSendString());
        }

        //以下静态方法
        #region 静态方法
        private static Regex regMsg = new Regex(@"<Message>(.*?)</Message>");
        private static Regex regNotMsg = new Regex(@"<Message>.*</Message>(.*)");

        private static Regex regMsgStart = new Regex(@"<Message>(.*)");
        private static Regex regMsgEnd = new Regex(@"(.*)</Message>");
        private static string strMsgStart = "<Message>";
        private static string strMsgEnd = "</Message>";
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
        /// <summary>
        /// 这里会丢弃 sender,action,value,guid 同时为空的值
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static Message[] GetMessages(string strMsg)
        {
            List<Message> kvList = new List<Message>();
            StringBuilder sb = new StringBuilder();
            var matchs = regMsg.Matches(strMsg);
            int count = matchs.Count;
            for (int i = 0; i < count; i++)
            {
                var m = matchs[i];
                string strTmp = m.Groups[1].Value;
                int iStart = strTmp.LastIndexOf(strMsgStart);
                int iEnd = strTmp.IndexOf(strMsgEnd);
                iStart = iStart == -1 ? 0 : iStart + strMsgStart.Length;
                iEnd = iEnd == -1 ? strTmp.Length : iEnd;
                strTmp = strTmp.Substring(iStart, iEnd - iStart);
                sb.Clear();
                //sb.Append(strMsgStart);
                sb.Append(strTmp);
                //sb.Append(strMsgEnd);
                try
                {
                    Message msg = GetMessage(sb.ToString());
                    if (msg.Action == "" && msg.Value == "" && msg.Sender == "" && msg.MessageGuid == "")
                        continue;
                    kvList.Add(msg);
                }
                catch { }
            }
            return kvList.ToArray();
        }
        /// <summary>
        /// 获取message格式的string中不是完成message格式的部分
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string GetStringNotMessage(string msg)
        {
            if (msg == null || msg == "") return "";
            var ms = regNotMsg.Matches(msg);
            try
            {
                msg = ms?[0]?.Groups?[1]?.Value;
            }
            catch { }
            return msg;
        }
        #endregion
    }
}

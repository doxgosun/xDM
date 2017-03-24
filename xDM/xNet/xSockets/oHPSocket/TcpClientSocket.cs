using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HPSocketCS;
using xDM.xNet.xSockets.oHPSocket.Extensions;

namespace xDM.xNet.xSockets.oHPSocket
{
    public class TcpClientSocket : IDisposable
    {
        private TcpPackClient client { get; set; } = null;
        public event Action<string> eShowMsg;
        /// <summary>
        /// 接收信息BufferSize
        /// </summary>
        public uint SocketBufferSize { get { return client.SocketBufferSize; } set { client.SocketBufferSize = value; } }


        private DateTime _lastReceiveTime { get; set; } = DateTime.Now;
        public DateTime LastReceiveTime { get { return _lastReceiveTime; } }
        private bool _ConnectCompleted { get; set; } = false;
        public bool ConnectCompleted { get { return _ConnectCompleted; } }
        public delegate void delHdMsg(TcpClientSocket sender, Message message);
        /// <summary>
        /// 处理信息 sender, message
        /// </summary>
        public event delHdMsg HandleMessage;
        /// <summary>
        /// 处理错误
        /// </summary>
        public event Action<TcpClientSocket, Exception> onError;
        private ConcurrentDictionary<string, Message> dicSendedMessages { get; set; } = new ConcurrentDictionary<string, Message>();
        private ConcurrentDictionary<string, Message> dicRevivedMessages { get; set; } = new ConcurrentDictionary<string, Message>();

        public TcpClientSocket()
        {
            client = new TcpPackClient();
            client.SocketBufferSize = 1024 * 2;
            client.OnReceive += Client_OnReceive;
            client.OnConnect += Client_OnConnect;
        }

        private HandleResult Client_OnConnect(TcpClient sender)
        {
            _ConnectCompleted = true;
            return HandleResult.Ok;
        }

        private HandleResult Client_OnReceive(TcpClient sender, byte[] bytes)
        {
            _lastReceiveTime = DateTime.Now;
            if (bytes == null || bytes.Length == 0)
                return HandleResult.Ok;
            var msg = bytes.DeDeserialize<Message>();
            if (msg != null)
            {
                if (msg.MessageGuid != null && msg.MessageGuid != "")
                {
                    if (dicSendedMessages.Count > 0)
                    {
                        Message tmpMsg;
                        if (dicSendedMessages.TryRemove(msg.MessageGuid, out tmpMsg))
                        {
                            dicRevivedMessages.TryAdd(msg.MessageGuid, msg);
                        }
                    }
                }
                this.HandleMessage?.BeginInvoke(this, msg, null, null);
            }
            return HandleResult.Ok;
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Connect(string ip, ushort port)
        {
            return client.Connetion(ip, port, false);
        }

        /// <summary>
        /// 异步发送信息
        /// </summary>
        /// <param name="msg"></param>
        public bool SendMessageAsync(Message msg)
        {
            return SendByteAsync(msg.ToSendByte());
        }
        /// <summary>
        /// 送消息并等待返回，注意：传输过去的GUID可能与发送的不一样！！！！方法返回的和发送的会一致！
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="timeOutMillionSecond"></param>
        /// <returns></returns>
        public Message SendMessage(Message msg, int timeOutMillionSecond)
        {
            if (msg == null)
            {
                return msg;
            }
            if (timeOutMillionSecond < 0)
            {
                timeOutMillionSecond = 10000;
            }
            var startTime = DateTime.Now;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, timeOutMillionSecond);
            var guid = msg.MessageGuid;
            if (guid + "" == "")
                guid = Guid.NewGuid().ToString("N");
            while (!dicSendedMessages.TryAdd(guid, msg))
            {
                guid = Guid.NewGuid().ToString("N");
                if ((DateTime.Now - startTime) > ts)
                {
                    return null;
                }
                Thread.Sleep(10);
            }
            var oGuid = msg.MessageGuid;
            msg.MessageGuid = guid;
            SendMessageAsync(msg);
            msg = null;
            while (!dicRevivedMessages.ContainsKey(guid))
            {
                if ((DateTime.Now - startTime) > ts)
                {
                    dicSendedMessages.TryRemove(guid, out msg);
                    msg = null;
                    return null;
                }
                Thread.Sleep(10);
            }
            dicRevivedMessages.TryRemove(guid, out msg);
            if (msg != null)
            {
                msg.MessageGuid = oGuid;
            }
            return msg;
        }

        private bool SendByteAsync(byte[] data)
        {
            return client.Send(data, data.Length);
        }

        public void Dispose()
        {
            dicRevivedMessages = null;
            dicSendedMessages = null;
            client.Stop();
        }
        public bool Stop()
        {
            return client.Stop();
        }
    }
}

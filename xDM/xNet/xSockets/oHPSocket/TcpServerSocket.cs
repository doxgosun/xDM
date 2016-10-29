using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HPSocketCS;
using xDM.xNet.xSockets.oHPSocket.Extensions;

namespace xDM.xNet.xSockets.oHPSocket
{
    public class TcpServerSocket : IDisposable
    {

        TcpPackServer server { get; set; } = null;

        /// <summary>
        /// 显示信息
        /// </summary>
        public event Action<string> eShowMsg;

        public delegate void delHdMsg(TcpServerSocket sender, IntPtr connID, Message message);
        /// <summary>
        /// 处理接收到的信息sender,remoteEndPoint,Messgae
        /// </summary>
        public event delHdMsg HandleMessage;
        /// <summary>
        /// 处理发生错误
        /// </summary>
        public event Action<TcpServerSocket, Exception> onError;
        /// <summary>
        /// 发送BufferSize
        /// </summary>
        public uint SocketBufferSize { get { return server.SocketBufferSize; } set { server.SocketBufferSize = value; } }
        /// <summary>
        /// 客户端空闲时间，默认一分钟（60000）,超过则断开连接
        /// </summary>
        public int ClientTimeOutMillionSecond { get; set; } = 60000;
        /// <summary>
        /// 连接数
        /// </summary>
        public uint ConnectionCount { get { return server.ConnectionCount; } }

        /// <summary>
        /// 发送信息缓存
        /// </summary>
        private ConcurrentDictionary<string, Message> dicSendedMessages { get; set; } = new ConcurrentDictionary<string, Message>();
        /// <summary>
        /// 接收信息缓存
        /// </summary>
        private ConcurrentDictionary<string, Message> dicRevivedMessages { get; set; } = new ConcurrentDictionary<string, Message>();

        public TcpServerSocket()
        {
            server = new TcpPackServer();
            server.SocketBufferSize = 1024 * 1;
            server.KeepAliveTime = 20000;
            server.KeepAliveInterval = 2000;
            Timer disconnectLilenceConnections = new Timer(new TimerCallback((obj) =>
            {
                server.DisconnectSilenceConnections(30000);
            }), null, 30000, 1000);
            //ThreadPool.SetMinThreads(100,100);
            //ThreadPool.SetMaxThreads(655350,655350);
            server.OnReceive += Server_OnReceive;
            server.OnAccept += Server_OnAccept;
            server.OnClose += Server_OnClose;
        }

        private HandleResult Server_OnClose(IntPtr connId, SocketOperation enOperation, int errorCode)
        {
            return HandleResult.Ok;
        }

        private HandleResult Server_OnAccept(IntPtr connId, IntPtr pClient)
        {
            return HandleResult.Ok;
        }

        private HandleResult Server_OnReceive(IntPtr connId, byte[] bytes)
        {
            //args ar = new args();
            //ar.bytes = bytes;
            //ar.connID = connId;
            //ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(hdMsgBytes), ar);
            //return HandleResult.Ok;
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
                this.HandleMessage?.BeginInvoke(this, connId, msg, null, null);
            }
            return HandleResult.Ok;
        }
        private class args
        {
            public byte[] bytes;
            public IntPtr connID;
        }
        private void hdMsgBytes(object oArgs)
        {
            args ar = oArgs as args;
            var msg = MessageExt.GetMessage(ar.bytes);
            if (msg != null)
            {
                if (msg.MessageGuid != null && msg.MessageGuid != "")
                {
                    Message tmpMsg;
                    if (dicSendedMessages.TryRemove(msg.MessageGuid, out tmpMsg))
                    {
                        dicRevivedMessages.TryAdd(msg.MessageGuid, msg);
                    }
                }
                this.HandleMessage?.BeginInvoke(this, ar.connID, msg, null, null);
            }
        }
        /// <summary>
        /// 绑定IP 端口号
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Bind(string ip, ushort port)
        {
            server.IpAddress = ip;
            server.Port = port;
            var result = server.Start();
            if (result)
            {
                server.Stop();
            }
            return result;
        }
        /// <summary>
        /// 绑定端口号到本机ip
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Bind(ushort port)
        {
            return Bind(IPAddress.Any, port);
        }
        /// <summary>
        /// 绑定IP 端口号
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Bind(IPAddress ip, ushort port)
        {
            return Bind(ip + "", port);
        }
        /// <summary>
        /// 绑定 30000-40000间的随机端口，返回绑定的端口
        /// </summary>
        /// <returns></returns>
        public int BindRandomPort()
        {
            return BindRandomPort(IPAddress.Any);
        }
        /// <summary>
        /// 绑定30000-40000间的随机端口，返回绑定的端口
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public int BindRandomPort(string ip)
        {
            return BindRandomPort(ip, 30000, 40000);
        }
        public int BindRandomPort(IPAddress ip)
        {
            return BindRandomPort(ip + "");
        }
        /// <summary>
        /// 绑定指定范围的随机商品，重试10000次后失败返回0；
        /// </summary>
        /// <param name="minPort"></param>
        /// <param name="maxPort"></param>
        /// <returns></returns>
        public int BindRandomPort(string ip, ushort minPort, ushort maxPort)
        {
            Random rd = new Random();
            bool sucess = false;
            ushort port = 0;
            int tryCount = 10000;
            while (!sucess && --tryCount > 0)
            {
                port = (ushort)rd.Next(minPort, maxPort);
                Bind(ip, port);
                sucess = true;
            }
            return sucess ? port : 0;
        }
        /// <summary>
        /// 开始监听
        /// </summary>
        public bool Listen()
        {
            return server.Start();
        }

        /// <summary>
        /// 异步给所有客户端发送信息
        /// </summary>
        /// <param name="msg"></param>
        public int SendMessageAsync(Message msg)
        {
            int sended = 0;
            var connIDs = server.GetAllConnectionIDs().ToArray();
            foreach (var item in connIDs)
            {
                if (SendMessageAsync(item, msg))
                {
                    sended++;
                }
            }
            return sended;
        }

        /// <summary>
        /// 给指定客户端发送消息，并等待返回，注意：传输过去的GUID可能与发送的不一样！！！！方法返回的和发送的会一致！
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="msg"></param>
        /// <param name="timeOutMillionSecond"></param>
        /// <returns></returns>
        public Message SendMessage(IntPtr connID, Message msg, int timeOutMillionSecond)
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
            {
                guid = Guid.NewGuid().ToString("N");
            }
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
            SendMessageAsync(connID, msg);
            msg = null;
            while (!dicRevivedMessages.TryRemove(guid, out msg))
            {
                if ((DateTime.Now - startTime) > ts)
                {
                    if (dicSendedMessages.TryRemove(guid, out msg))
                        msg = null;
                    return null;
                }
                Thread.Sleep(10);
            }
            if (msg != null)
            {
                msg.MessageGuid = oGuid;
            }
            return msg;
        }

        /// <summary>
        /// 异步给指定的客户端发送信息
        /// </summary>
        /// <param name="remoteEndPoint">客户端的remoteEndPoint</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool SendMessageAsync(IntPtr connID, Message msg)
        {
            return SendByteAsync(connID, msg?.ToSendByte());
        }

        private bool SendByteAsync(IntPtr connID, byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return false;
            }
            return server.Send(connID, data, data.Length);
        }

        public void Dispose()
        {
            dicRevivedMessages = null;
            dicSendedMessages = null;
            server.Stop();
        }
        public bool Stop()
        {
            return server.Stop();
        }
    }
}

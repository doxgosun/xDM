using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xDM.xNet.xSockets.xSocket
{
    public class TcpClientSocket : IDisposable
    {
        public event Action<string> eShowMsg;
        /// <summary>
        /// 接收信息BufferSize
        /// </summary>
        public int ReciveBufferSize { get; set; } = 1024 * 1;

        private Socket socket { get; set; } = null;

        public EndPoint RemoteEndPoint { get { return socket?.RemoteEndPoint; } }

        private DateTime _lastReceiveTime { get; set; } = DateTime.Now;
        public DateTime LastReceiveTime { get { return _lastReceiveTime; } }
        private bool _ConnectCompleted { get; set; } = false;
        public bool ConnectCompleted { get { return _ConnectCompleted; } }
        /// <summary>
        /// 处理信息 sender, message
        /// </summary>
        public event Action<TcpClientSocket, Message> HandleMessage;
        /// <summary>
        /// 处理错误
        /// </summary>
        public event Action<TcpClientSocket, Exception> onError;
        ConcurrentDictionary<string, Message> dicSendedMessages { get; set; } = new ConcurrentDictionary<string, Message>();
        ConcurrentDictionary<string, Message> dicRevivedMessages { get; set; } = new ConcurrentDictionary<string, Message>();

        public TcpClientSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Connect(string ip, int port)
        {
            string tmp;
            return Connect(ip, port, out tmp);
        }
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Connect(string ip, int port, out string msg)
        {
            try
            {
                this._ConnectCompleted = false;
                IPEndPoint serverIp = new IPEndPoint(IPAddress.Parse(ip), port);
                SocketAsyncEventArgs saea = new SocketAsyncEventArgs();
                saea.Completed += Accept_Completed;
                saea.RemoteEndPoint = serverIp;
                msg = "连接成功！";
                return socket.ConnectAsync(saea);
            }
            catch (Exception err)
            {
                msg = err.Message;
                return false;
            }
        }
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool ConnectAsync(string ip, int port)
        {
            string msg;
            return Connect(ip, port, out msg);
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                Socket server = e.AcceptSocket;
                Socket client = sender as Socket;
                var t = new Thread(new ParameterizedThreadStart((obj) =>
                {
                    var cliectSocket = obj as Socket;
                    SocketAsyncEventArgs receciveArg = new SocketAsyncEventArgs();
                    byte[] data = new byte[ReciveBufferSize];
                    receciveArg.SetBuffer(data, 0, data.Length);
                    receciveArg.Completed += Rececive_Completed;
                    cliectSocket.ReceiveAsync(receciveArg);

                }));
                t.Priority = ThreadPriority.Highest;
                t.IsBackground = true;
                t.Start(client);

                eShowMsg?.BeginInvoke("Socket连接成功", null, null);
                this._ConnectCompleted = true;
            }
            catch
            { }
        }
        private void Rececive_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                _lastReceiveTime = DateTime.Now;
                Socket socket = sender as Socket;
                if (e.SocketError == SocketError.Success)
                {
                    int rec = e.BytesTransferred;
                    if (rec > 0)
                    {
                        var data = e.Buffer.ToArray();
                        string reMsg = Encoding.UTF8.GetString(data, 0, rec);
                        string msgTmp = e.UserToken + "" + reMsg;
                        //msgTmp = msgTmp + reMsg;
                        Message[] msgs = MessageExt.GetMessages(msgTmp);
                        if (msgs.Length > 0)
                        {
                            new Action<Message[]>((ms) =>
                            {
                                foreach (Message msg in ms)
                                {
                                    if (msg.MessageGuid + "" != "")
                                    {
                                        Message tmpMsg;
                                        if (dicSendedMessages.TryRemove(msg.MessageGuid, out tmpMsg))
                                        {
                                            dicRevivedMessages.TryAdd(msg.MessageGuid, msg);
                                        }
                                    }
                                    this.HandleMessage?.BeginInvoke(this, msg, null, null);
                                }
                            }).BeginInvoke(msgs, null, null);

                            msgTmp = MessageExt.GetStringNotMessage(msgTmp);
                        }
                        e.UserToken = msgTmp;
                    }
                    else
                    {
                        int av = socket.Available;
                        socket.Close();
                        socket.Dispose();
                        e.Dispose();
                    }
                }
                socket.ReceiveAsync(e);
            }
            catch { }
        }
        /// <summary>
        /// 异步发送信息
        /// </summary>
        /// <param name="msg"></param>
        public void SendMessageAsync(Message msg)
        {
            try
            {
                SendAsync(msg.ToSendString());
            }
            catch (Exception err)
            {
                onError?.BeginInvoke(this, err, null, null);
            }
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

        private void SendAsync(string msgPro)
        {
            byte[] data = Encoding.UTF8.GetBytes(msgPro);
            SendByteAsync(data);
        }
        private void SendByteAsync(byte[] data)
        {
            try
            {
                SocketAsyncEventArgs sendArg = SocketAsyncEventArgsPool.SendPool.Get();
                sendArg.SetBuffer(data, 0, data.Length);
                socket.SendAsync(sendArg);
            }
            catch (Exception err)
            {
                onError?.BeginInvoke(this, err, null, null);
                if (err.GetType() == typeof(SocketException))
                {

                }
            }
        }

        public void Dispose()
        {
            socket.Dispose();
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using xDM.xNet.xSockets.xSocket.Extensions;

namespace xDM.xNet.xSockets.xSocket
{
    public class TcpServerSocket : IDisposable
    {

        Socket socket { get; set; } = null;
        public EndPoint RemoteEndPoint { get { return socket?.RemoteEndPoint; } }

        /// <summary>
        /// 显示信息
        /// </summary>
        public event Action<string> eShowMsg;

        /// <summary>
        /// 处理接收到的信息sender,remoteEndPoint,Messgae
        /// </summary>
        public event Action<TcpServerSocket, string, Message> HandleMessage;
        /// <summary>
        /// 处理发生错误
        /// </summary>
        public event Action<object, Exception> onError;
        /// <summary>
        /// 发送BufferSize
        /// </summary>
        public int SendBufferSize { get; set; } = 1024;
        /// <summary>
        /// 接收BufferSize
        /// </summary>
        public int ReciveBufferSize { get; set; } = 1024 * 2;
        /// <summary>
        /// 客户端空闲时间，默认一分钟（60000）,超过则断开连接
        /// </summary>
        public int ClientTimeOutMillionSecond { get; set; } = 60000;
        /// <summary>
        /// 连接数
        /// </summary>
        public int ClientCount { get { return ClientSocketDic.Count; } }
        /// <summary>
        /// 所有连接的客户端，RemoteEndPoint，Socket,DateTime
        /// </summary>
        protected ConcurrentDictionary<string, KeyValuePair<Socket, DateTime>> ClientSocketDic { get; set; } = new ConcurrentDictionary<string, KeyValuePair<Socket, DateTime>>();

        protected ConcurrentDictionary<string, TcpServerSocketRecivedDataHandler> ClientRecHandlers { get; set; } = new ConcurrentDictionary<string, TcpServerSocketRecivedDataHandler>();
        /// <summary>
        /// 发送信息缓存
        /// </summary>
        ConcurrentDictionary<string, Message> dicSendedMessages { get; set; } = new ConcurrentDictionary<string, Message>();
        /// <summary>
        /// 接收信息缓存
        /// </summary>
        ConcurrentDictionary<string, Message> dicRevivedMessages { get; set; } = new ConcurrentDictionary<string, Message>();

        public TcpServerSocket()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        /// <summary>
        /// 绑定端口号
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Bind(int port)
        {
            try
            {
                IPEndPoint serverIp = new IPEndPoint(IPAddress.Any, port);
                socket.Bind(serverIp);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }
        /// <summary>
        /// 绑定30000-40000间的随机端口，返回绑定的端口
        /// </summary>
        /// <returns></returns>
        public int BindRandomPort()
        {
            return BindRandomPort(30000, 40000);
        }
        /// <summary>
        /// 绑定指定范围的随机商品，重试10000次后失败返回-1；
        /// </summary>
        /// <param name="minPort"></param>
        /// <param name="maxPort"></param>
        /// <returns></returns>
        public int BindRandomPort(int minPort, int maxPort)
        {
            Random rd = new Random();
            bool sucess = false;
            int port = -1;
            int tryCount = 10000;
            while (!sucess && --tryCount > 0)
            {
                try
                {
                    port = rd.Next(minPort, maxPort);
                    IPEndPoint serverIp = new IPEndPoint(IPAddress.Any, port);
                    socket.Bind(serverIp);
                    sucess = true;
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
            return -1;
        }
        /// <summary>
        /// 开始监听
        /// </summary>
        public void Listen(int backlog)
        {
            socket.Listen(backlog);
            Thread tWatcher = new Thread(Watcher);
            tWatcher.IsBackground = true;
            tWatcher.Start();
            SocketAsyncEventArgs acceptArg = new SocketAsyncEventArgs();
            acceptArg.Completed += AcceptArg_Completed;
            socket.AcceptAsync(acceptArg);
        }
        /// <summary>
        /// 开始监听，backlog=1000；
        /// </summary>
        public void Listen()
        {
            Listen(1000);
        }

        private void NewClientAccept(object oClientSocket)
        {
            Socket clientSocket = oClientSocket as Socket;
            try
            {
                if (ClientSocketDic.TryAdd(clientSocket.RemoteEndPoint.ToString(), new KeyValuePair<Socket, DateTime>(clientSocket, DateTime.Now)))
                {
                    eShowMsg?.BeginInvoke($"客户端上线，RemoteEndPoint：{clientSocket.RemoteEndPoint}", null, null);
                    SocketAsyncEventArgs reciveArgs = new SocketAsyncEventArgs();
                    byte[] data = new byte[ReciveBufferSize];
                    reciveArgs.SetBuffer(data, 0, data.Length);
                    var dataHandler = new TcpServerSocketRecivedDataHandler();
                    dataHandler.server = this;
                    dataHandler.HandleMessage = this.HandleMessage;
                    dataHandler.dicSendedMessages = dicSendedMessages;
                    dataHandler.dicRevivedMessages = dicRevivedMessages;
                    dataHandler.ClientSocketDic = ClientSocketDic;
                    dataHandler.clientSocket = clientSocket;
                    ClientRecHandlers.TryAdd(clientSocket.RemoteEndPoint.ToString(), dataHandler);
                    reciveArgs.UserToken = dataHandler;
                    reciveArgs.Completed += Recive_Completed;
                    clientSocket.ReceiveAsync(reciveArgs);
                }
                else
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    clientSocket.Dispose();
                }
            }
            catch
            {
            }
        }

        private void AcceptArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket socket = sender as Socket;
            Socket clientSocket = e.AcceptSocket;
            Thread client = new Thread(new ParameterizedThreadStart(NewClientAccept));
            client.IsBackground = true;
            client.Start(clientSocket);
            SocketAsyncEventArgs acceptArg = new SocketAsyncEventArgs();
            acceptArg.Completed += AcceptArg_Completed;
            socket.AcceptAsync(acceptArg);
            eShowMsg?.BeginInvoke("AcceptArg_Completed", null, null);
        }
        private void Recive_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                Socket clientSocket = sender as Socket;
                if (e.SocketError == SocketError.Success)
                {
                    int rec = e.BytesTransferred;
                    if (rec > 0)
                    {
                        var data = e.Buffer.ToArray();
                        byte[] tmp = new byte[rec];
                        data.CopyTo(tmp, 0, rec);
                        var dataHandler = e.UserToken as TcpServerSocketRecivedDataHandler;
                        dataHandler.dataQueue.Enqueue(tmp);
                    }
                }
                clientSocket.ReceiveAsync(e);
            }
            catch
            {
            }
        }
        /// <summary>
        /// 异步给所有客户端发送信息
        /// </summary>
        /// <param name="msg"></param>
        public int SendMessageAsync(Message msg)
        {
            new Action<Message>((m) =>
            {
                var reps = ClientSocketDic.Keys.ToArray();
                foreach (var item in reps)
                {
                    SendMessageAsync(item, msg);
                }
            }).BeginInvoke(msg, null, null);
            return ClientSocketDic.Count;
        }

        /// <summary>
        /// 给指定客户端发送消息，并等待返回，注意：传输过去的GUID可能与发送的不一样！！！！方法返回的和发送的会一致！
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="msg"></param>
        /// <param name="timeOutMillionSecond"></param>
        /// <returns></returns>
        public Message SendMessage(string remoteEndPoint, Message msg, int timeOutMillionSecond)
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
            SendMessageAsync(remoteEndPoint, msg);
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
        public void SendMessageAsync(string remoteEndPoint, Message msg)
        {
            KeyValuePair<Socket, DateTime> kv;
            try
            {
                if (ClientSocketDic.TryGetValue(remoteEndPoint, out kv))
                {
                    SendByteAsync(kv.Key, msg.ToSendByte());
                    Thread.Sleep(1);
                }
            }
            catch (Exception err)
            {
                onError?.BeginInvoke(this, err, null, null);
                ClientSocketDic.TryRemove(remoteEndPoint, out kv);
                TcpServerSocketRecivedDataHandler hder;
                if (ClientRecHandlers.TryRemove(remoteEndPoint, out hder))
                {
                    hder.Quit = true;
                }
                eShowMsg?.BeginInvoke($"SendMessage错误！RemoteEndPoint:{remoteEndPoint},Error:{err.Message}", null, null);
            }
        }

        private void SendAsync(Socket worksocket, string message)
        {
            if (worksocket == null)
            {
                return;
            }
            byte[] data = Encoding.UTF8.GetBytes(message);
            SendByteAsync(worksocket, data);
        }
        private void SendByteAsync(Socket worksocket, byte[] data)
        {
            try
            {
                SocketAsyncEventArgs sendArg = SocketAsyncEventArgsPool.SendPool.Get();
                sendArg.SetBuffer(data, 0, data.Length);
                worksocket.SendAsync(sendArg);
            }
            catch (Exception err)
            {
                onError?.BeginInvoke(this, err, null, null);
                if (err.GetType() == typeof(SocketException))
                {

                }
            }
        }


        private void Watcher()
        {
            while (true)
            {
                try
                {
                    var remoteEndPoints = ClientSocketDic.Keys.ToArray();
                    foreach (var remoteEndPoint in remoteEndPoints)
                    {
                        KeyValuePair<Socket, DateTime> kv;
                        if (ClientSocketDic.TryGetValue(remoteEndPoint, out kv))
                        {
                            DateTime lastReceiveTime = kv.Value;
                            if ((DateTime.Now - lastReceiveTime).TotalMilliseconds > 60000)
                            {
                                try
                                {
                                    if (ClientSocketDic.TryRemove(remoteEndPoint, out kv))
                                    {
                                        eShowMsg?.BeginInvoke($"强行中止长时间无响应连接：{kv.Key.RemoteEndPoint}", null, null);
                                        kv.Key.Shutdown(SocketShutdown.Both);
                                        kv.Key.Close();
                                        kv.Key.Dispose();
                                    }
                                }
                                catch { }
                            }
                        }
                    }

                }
                catch { }
                Thread.Sleep(100);
            }
        }

        public void Dispose()
        {
            socket.Dispose();
        }
    }
}

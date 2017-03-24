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
    public class TcpServerSocket :TcpBaseSocket, IDisposable
    {
        /// <summary>
        /// 显示信息
        /// </summary>
        public event showMsg eShowMsg;
        /// <summary>
        /// 处理发生错误
        /// </summary>
        public event onError OnError;
        /// <summary>
        /// 处理接收到的信息sender,remoteEndPoint,Messgae
        /// </summary>
        public event serverHandlePackage HandleMessage;
        /// <summary>
        /// 客户端空闲时间(毫秒)，默认一分钟（60000）,超过则断开连接
        /// </summary>
        public int ClientTimeOutMillionSecond { get; set; } = 20000;
        /// <summary>
        /// 连接数
        /// </summary>
        public int ClientCount { get { return ClientInfosDic.Count; } }
        /// <summary>
        /// 所有连接的客户端，RemoteEndPoint，Socket,DateTime
        /// </summary>
        protected ConcurrentDictionary<Socket, UserToken> ClientInfosDic { get; set; } = new ConcurrentDictionary<Socket, UserToken>();
        protected ConcurrentDictionary<string, Socket> ClientSocketsDic { get; set; } = new ConcurrentDictionary<string, Socket>();

        private bool _exit = false;

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
            catch
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
        /// 开始监听.
        /// </summary>
        /// <param name="backlog">Backlog.</param>/
        public void Listen(int backlog)
        {
            socket.Listen(backlog);
            Thread tDaemon = new Thread(DaemonThreadStart);
            tDaemon.IsBackground = true;
            tDaemon.Start();
            SocketAsyncEventArgs acceptArg = new SocketAsyncEventArgs();
            acceptArg.Completed += AcceptArg_Completed;
            socket.AcceptAsync(acceptArg);
        }
        /// <summary>
        /// 开始监听，backlog=1024；
        /// </summary>
        public void Listen()
        {
            Listen(65534);
        }

        private Action<TcpServerSocket, string, byte[]> _HandlePackage;

        private void NewClientAccept(object oClientSocket)
        {
            Socket clientSocket = oClientSocket as Socket;
            try
            {
                UserToken userToken = UserToken.Pop();
                if (ClientInfosDic.TryAdd(clientSocket, userToken) && ClientSocketsDic.TryAdd(clientSocket.RemoteEndPoint + "",clientSocket))
                {
                    eShowMsg?.Invoke($"客户端上线，RemoteEndPoint：{clientSocket.RemoteEndPoint}");
                    userToken.socket = clientSocket;
                    userToken.LastWorkingTime = DateTime.Now;
                    if (userToken.ReciveArgs == null)
                    {
                        userToken.ReciveArgs = new SocketAsyncEventArgs();
                        userToken.ReciveArgs.Completed += ReciveArgs_Completed;
                        userToken.ReciveHandler.server = this;
                        if (_HandlePackage == null)
                            _HandlePackage = new Action<TcpServerSocket, string, byte[]>((sender, remoteEndPoint, msg) =>
                            {
                                HandleMessage.Invoke(sender, remoteEndPoint, msg);
                            });
                        userToken.ReciveHandler.HandlePackage = _HandlePackage;
                        userToken.ReciveHandler.dicSendedMessages = dicSendedPackages;
                        userToken.ReciveHandler.dicRevivedMessages = dicRevivedPackages;
                        userToken.ReciveHandler.ClientSocketDic = ClientInfosDic;
                        userToken.ReciveArgs.UserToken = userToken;
                    }

                    userToken.ReciveHandler.clientSocket = clientSocket;
                    userToken.ReciveHandler.Start();
                    byte[] data = new byte[ReciveBufferSize];
                    userToken.ReciveArgs.SetBuffer(data, 0, data.Length);
                    userToken.ReciveArgs.AcceptSocket = clientSocket;
                    clientSocket.ReceiveAsync(userToken.ReciveArgs);
                }
                else
                {
                    DisposeClient(clientSocket,"加入客户端队列失败！");
                }
            }
            catch(Exception err)
            {
                DisposeClient(clientSocket,err.Message);
                OnError?.Invoke(this,err);
            }
        }

        private void AcceptArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket socket = sender as Socket;
            Socket clientSocket = e.AcceptSocket;
            e.AcceptSocket = null;
            try
            {
                socket.AcceptAsync(e);
            }
            catch (Exception err)
            {
                SocketAsyncEventArgs acceptArg = new SocketAsyncEventArgs();
                acceptArg.Completed += AcceptArg_Completed;
                socket.AcceptAsync(acceptArg);
                OnError?.Invoke(this, err);
            }
            NewClientAccept(clientSocket);
        }

        private void ReciveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket clientSocket = sender as Socket;
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    int rec = e.BytesTransferred;
                    if (rec > 0)
                    {
                        byte[] d = new byte[rec];
                        Array.Copy(e.Buffer, 0, d,0, rec);
                        var userToken = e.UserToken as UserToken;
                        userToken.ReciveHandler.hdData(d);
                    }
                    if (!clientSocket.Connected || !clientSocket.ReceiveAsync(e))
                    {
                        e.AcceptSocket = null;
                        DisposeClient(clientSocket, "连接已断开");
                    }
                }
            }
            catch(Exception err)
            {
                DisposeClient(clientSocket,err.Message);
                OnError?.Invoke(this, err);
            }
        }
        private Socket GetClientSocket(string remoteEndPoint)
        {
            Socket clientSocket;
            ClientSocketsDic.TryGetValue(remoteEndPoint, out clientSocket);
            return clientSocket;
        }

        private UserToken GetClientInfo(string remoteEndPoint)
        {
            Socket sk = GetClientSocket(remoteEndPoint);
            if (sk == null)
                return null;
            UserToken ci;
            ClientInfosDic.TryGetValue(sk, out ci);
            return ci;
        }

        /// <summary>
        /// 异步给所有客户端发送信息
        /// </summary>
        /// <param name="msg"></param>
        public int SendMessageAsync(Message msg)
        {
            if (msg == null)
                return 0;
            int sendedCount = 0;
            var reps = ClientInfosDic.Keys.ToArray();
            foreach (var item in reps)
            {
                if (SendByteAsync(item, msg.ToSendByte()))
                    sendedCount++;
            }
            return sendedCount;
        }

        /// <summary>
        /// 给指定客户端发送消息，并等待返回，注意：传输过去的GUID可能与发送的不一样！！！！方法返回的和发送的会一致！
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="msg"></param>
        /// <param name="timeOutMillionSecond"></param>
        /// <returns></returns>
		//public bool SendMessage(string remoteEndPoint, Message msg,out Message resultMsg, int timeOutMillionSecond)
  //      {
		//	resultMsg = null;
  //          if (msg == null)
  //          {
		//		return false;
  //          }
  //          if (timeOutMillionSecond < 0)
  //          {
  //              timeOutMillionSecond = 10000;
  //          }
  //          var startTime = DateTime.Now;
  //          TimeSpan ts = new TimeSpan(0, 0, 0, 0, timeOutMillionSecond);
  //          var guid = msg.GetGuid();
  //          if (guid == Guid.Empty)
  //          {
  //              guid = Guid.NewGuid();
  //              msg.SetGuid(guid);
  //          }
  //          while (!dicSendedPackages.TryAdd(guid, msg))
  //          {
  //              guid = Guid.NewGuid();
  //              if ((DateTime.Now - startTime) > ts)
  //              {
		//			return false;
  //              }
  //              Thread.Sleep(10);
  //          }
  //          var oGuid = msg.GetGuid();
  //          msg.SetGuid(guid);
  //          SendMessageAsync(remoteEndPoint, msg);
		//	while (!dicRevivedPackages.TryRemove(guid, out resultMsg))
  //          {
  //              if ((DateTime.Now - startTime) > ts)
  //              {
		//			if (dicSendedPackages.TryRemove(guid, out resultMsg))
		//				resultMsg = null;
		//			return false;
  //              }
  //              Thread.Sleep(10);
  //          }
		//	if (resultMsg != null)
  //          {
		//		resultMsg.SetGuid(oGuid);
		//		return true;
  //          }
		//	return false;
  //      }

        /// <summary>
        /// 异步给指定的客户端发送信息
        /// </summary>
        /// <param name="remoteEndPoint">客户端的remoteEndPoint</param>
        /// <param name="msg"></param>
        /// <returns></returns>
		public bool SendMessageAsync(string remoteEndPoint, Message msg)
        {
            return SendDataAsync(remoteEndPoint, msg.ToSendByte());
        }

        public bool SendDataAsync(string remoteEndPoint, byte[] data)
        {
            var clientSocket = GetClientSocket(remoteEndPoint);
            if (clientSocket != null && data != null)
                return SendByteAsync(clientSocket, data);
            return false;
        }



        private void DaemonThreadStart()
        {
            while (!_exit)
            {
                try
                {
                    var clientInfos = ClientInfosDic.Values.ToArray();
                    foreach (var ci in clientInfos)
                    {
                        if (_exit)
                            break;
                        if (!ci.socket.Connected)
                            DisposeClient(ci.socket, "连接已断开");
                        if ((DateTime.Now - ci.LastWorkingTime).TotalMilliseconds > this.ClientTimeOutMillionSecond)
                            DisposeClient(ci.socket, "连接超时");
                    }
                }
                catch (Exception err)
                {
                    OnError?.Invoke(this, err);
                }
                Thread.Sleep(100);
            }
        }

        private void DisposeClient(Socket worksocket,string message)
        {
            if (worksocket == null)
                return;
            message = "断开连接：" + message;
            try
            {
                UserToken ci = null;
                if (ClientInfosDic.TryRemove(worksocket, out ci) && ci != null)
                {
                    eShowMsg?.Invoke($"{message}，{ci?.RemoteEndPoint}");
                    ci?.ReciveHandler?.Stop();
                    UserToken.Push(ci);
                    ClientSocketsDic.TryRemove(ci.RemoteEndPoint,out worksocket);
                }
                else
                {
                    try
                    {
                        eShowMsg?.Invoke($"{message}，{worksocket?.RemoteEndPoint}");
                    }
                    catch
                    {
                        eShowMsg?.Invoke($"{message}，{worksocket}");
                    }
                }
                GC.Collect();
                //worksocket.Shutdown(SocketShutdown.Both);
                worksocket?.Close();
                worksocket?.Dispose();
            }
            catch(Exception err)
            {
                OnError?.Invoke(this, err);
            }
        }

        public void Dispose()
        {
            _exit = true;
			socket.Dispose();
        }

		protected override void SendByteAsyncFailed(Socket worksocket, string message)
		{
			DisposeClient(worksocket, message);
		}

        protected override void HandleError(Exception err)
        {
            OnError?.Invoke(this, err);
        }
    }
}

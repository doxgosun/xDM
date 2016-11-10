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
		/// 处理接收到的信息sender,remoteEndPoint,Messgae
		/// </summary>
		public event Action<TcpServerSocket, string, Message> HandleMessage;
        /// <summary>
        /// 客户端空闲时间(毫秒)，默认一分钟（60000）,超过则断开连接
        /// </summary>
        public int ClientTimeOutMillionSecond { get; set; } = 60000;
        /// <summary>
        /// 连接数
        /// </summary>
        public int ClientCount { get { return ClientInfosDic.Count; } }
        /// <summary>
        /// 所有连接的客户端，RemoteEndPoint，Socket,DateTime
        /// </summary>
        protected ConcurrentDictionary<Socket, ClientInfo> ClientInfosDic { get; set; } = new ConcurrentDictionary<Socket, ClientInfo>();
        protected ConcurrentDictionary<string, Socket> ClientSockets { get; set; } = new ConcurrentDictionary<string, Socket>();

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
            Thread tWatcher = new Thread(Watcher);
            tWatcher.IsBackground = true;
            tWatcher.Start();
            SocketAsyncEventArgs acceptArg = new SocketAsyncEventArgs();
            acceptArg.Completed += AcceptArg_Completed;
            socket.AcceptAsync(acceptArg);
        }
        /// <summary>
        /// 开始监听，backlog=1024；
        /// </summary>
        public void Listen()
        {
            Listen(1024);
        }

        private void NewClientAccept(object oClientSocket)
        {
            try
            {
                Socket clientSocket = oClientSocket as Socket;
                ClientInfo client = ClientInfo.Get();
                if (ClientInfosDic.TryAdd(clientSocket, client) && ClientSockets.TryAdd(clientSocket.RemoteEndPoint + "",clientSocket))
                {
                    eShowMsg?.BeginInvoke($"客户端上线，RemoteEndPoint：{clientSocket.RemoteEndPoint}", null, null);
                    client.socket = clientSocket;
                    client.LastWorkingTime = DateTime.Now;
                    if (client.ReciveArgs == null)
                    {
                        client.ReciveArgs = new SocketAsyncEventArgs();
                        client.ReciveArgs.Completed += ReciveArgs_Completed;
                        client.ReciveHandler.server = this;
                        client.ReciveHandler.HandleMessage = this.HandleMessage;
                        client.ReciveHandler.dicSendedMessages = dicSendedMessages;
                        client.ReciveHandler.dicRevivedMessages = dicRevivedMessages;
                        client.ReciveHandler.ClientSocketDic = ClientInfosDic;
                        client.ReciveArgs.UserToken = client.ReciveHandler;
                    }

                    client.ReciveHandler.clientSocket = clientSocket;
                    client.ReciveHandler.Start();
                    byte[] data = new byte[ReciveBufferSize];
                    client.ReciveArgs.SetBuffer(data, 0, data.Length);

                    clientSocket.ReceiveAsync(client.ReciveArgs);
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
            e.AcceptSocket = null;
            socket.AcceptAsync(e);
            new Task((obj)=> { NewClientAccept(clientSocket); },clientSocket, TaskCreationOptions.None).Start();
        }
        private void ReciveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                Socket clientSocket = sender as Socket;
                if (e.SocketError == SocketError.Success)
                {
                    int rec = e.BytesTransferred;
                    if (rec > 0)
                    {
                        byte[] tmp = new byte[rec];
                        Array.Copy(e.Buffer, 0, tmp,0, rec);
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
        private Socket GetClientSocket(string remoteEndPoint)
        {
            Socket clientSocket;
            ClientSockets.TryGetValue(remoteEndPoint, out clientSocket);
            return clientSocket;
        }
        /// <summary>
        /// 异步给所有客户端发送信息
        /// </summary>
        /// <param name="msg"></param>
        public int SendMessageAsync(Message msg)
        {
            var reps = ClientInfosDic.Keys.ToArray();
            foreach (var item in reps)
            {
                SendByteAsync(item, msg.ToSendByte());
            }
            return ClientInfosDic.Count;
        }

        /// <summary>
        /// 给指定客户端发送消息，并等待返回，注意：传输过去的GUID可能与发送的不一样！！！！方法返回的和发送的会一致！
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="msg"></param>
        /// <param name="timeOutMillionSecond"></param>
        /// <returns></returns>
		public bool SendMessage(string remoteEndPoint, Message msg,out Message resultMsg, int timeOutMillionSecond)
        {
			resultMsg = null;
            if (msg == null)
            {
				return false;
            }
            if (timeOutMillionSecond < 0)
            {
                timeOutMillionSecond = 10000;
            }
            var startTime = DateTime.Now;
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, timeOutMillionSecond);
            var guid = msg.GetGuid();
            if (guid == Guid.Empty)
            {
                guid = Guid.NewGuid();
                msg.SetGuid(guid);
            }
            while (!dicSendedMessages.TryAdd(guid, msg))
            {
                guid = Guid.NewGuid();
                if ((DateTime.Now - startTime) > ts)
                {
					return false;
                }
                Thread.Sleep(10);
            }
            var oGuid = msg.GetGuid();
            msg.SetGuid(guid);
            SendMessageAsync(remoteEndPoint, msg);
			while (!dicRevivedMessages.TryRemove(guid, out resultMsg))
            {
                if ((DateTime.Now - startTime) > ts)
                {
					if (dicSendedMessages.TryRemove(guid, out resultMsg))
						resultMsg = null;
					return false;
                }
                Thread.Sleep(10);
            }
			if (resultMsg != null)
            {
				resultMsg.SetGuid(oGuid);
				return true;
            }
			return false;
        }

        /// <summary>
        /// 异步给指定的客户端发送信息
        /// </summary>
        /// <param name="remoteEndPoint">客户端的remoteEndPoint</param>
        /// <param name="msg"></param>
        /// <returns></returns>
		public bool SendMessageAsync(string remoteEndPoint, Message msg)
        {
            var clientSocket = GetClientSocket(remoteEndPoint);
            if (clientSocket != null)
				return SendByteAsync(clientSocket, msg.ToSendByte());
			return false;
        }




        private void Watcher()
        {
            while (!_exit)
            {
                try
                {
                    var clientInfos = ClientInfosDic.Values.ToArray();
                    foreach (var ci in clientInfos)
                        if ((DateTime.Now - ci.LastWorkingTime).TotalMilliseconds > this.ClientTimeOutMillionSecond)
                            DisposeClient(ci.socket,"关闭超时了连接");
                }
                catch { }
                Thread.Sleep(100);
            }
        }

        private void DisposeClient(Socket worksocket,string message)
        {
            if (worksocket == null)
                return;
            try
            {
                ClientInfo ci = null;
                if (ClientInfosDic.TryRemove(worksocket, out ci))
                {
                    eShowMsg?.BeginInvoke($"{message}：{ci.RemoteEndPoint}", null, null);
                    ci.ReciveHandler.Stop();
                    ClientSockets.TryRemove(ci.RemoteEndPoint,out worksocket);
                }
                worksocket.Shutdown(SocketShutdown.Both);
                worksocket.Close();
                worksocket.Dispose();
            }
            catch { }
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
	}
}

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
    public class TcpClientSocket :TcpBaseSocket, IDisposable
    {
        private DateTime _lastReceiveTime { get; set; } = DateTime.Now;
        public DateTime LastReceiveTime { get { return _lastReceiveTime; } }
        private bool _ConnectCompleted { get; set; } = false;
        public bool ConnectCompleted { get { return _ConnectCompleted; } }
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
        public event clientHandlePackage HandleMessage;
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
        private Action<TcpClientSocket, Message> _HandleMessage;
        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                //Socket server = e.AcceptSocket;
                //Socket client = sender as Socket;
                //var t = new Thread(new ParameterizedThreadStart((obj) =>
                //{
                //    var cliectSocket = obj as Socket;
                //    SocketAsyncEventArgs receciveArg = new SocketAsyncEventArgs();
                //    byte[] data = new byte[ReciveBufferSize];
                //    receciveArg.SetBuffer(data, 0, data.Length);

                //    var dataHandler = new TcpClientSocketRecivedDataHandler();
                //    dataHandler.client = this;
                //    dataHandler.server = server;
                //    if (_HandleMessage == null)
                //        _HandleMessage = new Action<TcpClientSocket, Message>((s, msg) =>
                //        {
                //            HandleMessage.Invoke(s,msg);
                //        });
                //    dataHandler.HandleMessage = _HandleMessage;
                //    dataHandler.dicSendedMessages = dicSendedPackages;
                //    dataHandler.dicRevivedMessages = dicRevivedPackages;
                //    dataHandler.Start();

                //    receciveArg.UserToken = dataHandler;

                //    receciveArg.Completed += Rececive_Completed;
                //    cliectSocket.ReceiveAsync(receciveArg);

                //}));
                //t.IsBackground = true;
                //t.Start(client);

                //eShowMsg?.BeginInvoke("Socket连接成功", null, null);
                //this._ConnectCompleted = true;
            }
            catch
            { }
        }
        private void Rececive_Completed(object sender, SocketAsyncEventArgs e)
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
                        Array.Copy(e.Buffer, 0, tmp, 0, rec);
                        var dataHandler = e.UserToken as TcpClientSocketRecivedDataHandler;
                        dataHandler.hdData(tmp);
                    }
                }
                clientSocket.ReceiveAsync(e);
            }
            catch
            {
            }
        }
        /// <summary>
        /// 异步发送信息
        /// </summary>
        /// <param name="msg"></param>
        public bool SendMessageAsync(Message msg)
        {
            try
            {
                return SendByteAsync(msg.ToSendByte());
            }
            catch (Exception err)
            {
                HandleError(err);
                return false;
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
            //if (msg == null)
            //{
            //    return msg;
            //}
            //if (timeOutMillionSecond < 0)
            //{
            //    timeOutMillionSecond = 10000;
            //}
            //var startTime = DateTime.Now;
            //TimeSpan ts = new TimeSpan(0, 0, 0, 0, timeOutMillionSecond);
            //var guid = msg.GetGuid();
            //if (guid == Guid.Empty)
            //    guid = Guid.NewGuid();
            //while (!dicSendedPackages.TryAdd(guid, msg))
            //{
            //    guid = Guid.NewGuid();
            //    if ((DateTime.Now - startTime) > ts)
            //    {
            //        return null;
            //    }
            //    Thread.Sleep(10);
            //}
            //var oGuid = msg.GetGuid();
            //msg.SetGuid(guid);
            //SendMessageAsync(msg);
            //msg = null;
            //while (!dicRevivedPackages.ContainsKey(guid))
            //{
            //    if ((DateTime.Now - startTime) > ts)
            //    {
            //        dicSendedPackages.TryRemove(guid, out msg);
            //        msg = null;
            //        return null;
            //    }
            //    Thread.Sleep(10);
            //}
            //dicRevivedPackages.TryRemove(guid, out msg);
            //if (msg != null)
            //{
            //    msg.SetGuid(oGuid);
            //}
            return msg;
        }

        private bool SendByteAsync(byte[] data)
        {
            try
            {
                SocketAsyncEventArgs sendArg = SocketAsyncEventArgsPool.SendPool.Pop();
                sendArg.SetBuffer(data, 0, data.Length);
                return socket.SendAsync(sendArg);
            }
            catch (Exception err)
            {
                HandleError(err);
                if (err.GetType() == typeof(SocketException))
                {

                }
                return false;
            }
        }

        public void Dispose()
        {
            socket.Dispose();
        }

		protected override void SendByteAsyncFailed(Socket worksocket, string message)
		{
			
		}

        protected override void HandleError(Exception err)
        {
            OnError?.BeginInvoke(this, err, null, null);
        }
    }
}

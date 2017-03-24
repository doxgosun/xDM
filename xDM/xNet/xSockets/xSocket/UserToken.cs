using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;

namespace xDM.xNet.xSockets.xSocket
{
	public class UserToken
	{
        public byte[] tmpBytes = new byte[0];
        public byte[] msgObj = null;
        public int reciveObjLength;
        public ConcurrentQueue<byte[]> dataQueue = new ConcurrentQueue<byte[]>();
		private Socket _socket { get; set; }
		public Socket socket { get { return _socket; } set { _socket = value; RemoteEndPoint = _socket?.RemoteEndPoint + ""; } }
		public string RemoteEndPoint { get; set; }
		public SocketAsyncEventArgs ReciveArgs { get; set; }
		public TcpServerSocketRecivedDataHandler ReciveHandler { get; set; }
        public ConcurrentQueue<byte[]> SendDataQueue { get; set; }
		public DateTime LastWorkingTime { get; set; }

        protected bool SendByteAsync(Socket worksocket, byte[] data,Action<Socket,string> SendByteAsyncFailed)
        {
            if (worksocket == null)
                return false;
            if (!worksocket.Connected)
                return false;
            try
            {
                SocketAsyncEventArgs sendArg = SocketAsyncEventArgsPool.SendPool.Pop();
                var dataList = new List<byte[]>();
                sendArg.BufferList = new List<ArraySegment<byte>>();
                sendArg.BufferList.Add(new ArraySegment<byte>(data));
                //sendArg.SetBuffer(data, 0, data.Length);
                var result = worksocket.SendAsync(sendArg);
                if (!result)
                    SendByteAsyncFailed?.Invoke(worksocket, "发送信息失败");
                return result;
            }
            catch (Exception err)
            {
                SendByteAsyncFailed.Invoke(worksocket, err.Message);
                return false;
            }
        }
        private UserToken()
		{
			ReciveHandler = new TcpServerSocketRecivedDataHandler();
            SendDataQueue = new ConcurrentQueue<byte[]>();
		}


        public static int MaxPoolSize = 100;
		private static readonly ConcurrentBag<UserToken> tokenPool = new ConcurrentBag<UserToken>();
		public static UserToken Pop()
		{
			UserToken info = null;
			if (tokenPool.Count == 0 || !tokenPool.TryTake(out info))
			{
				info = new UserToken();
			}
			if (tokenPool.Count < 2)
				new Action(() =>
				{
					var count = 8;
					while (--count > 0)
					{
						tokenPool.Add(new UserToken());
					}
				}).BeginInvoke(null, null);
			return info;
		}
		public static void Push(UserToken ut)
		{
            if (ut != null)
            {
                ut.ReciveHandler.Stop();
                ut.ReciveArgs.AcceptSocket = null;
                if (MaxPoolSize < 1)
                    MaxPoolSize = 128;
                if(tokenPool.Count < MaxPoolSize)
                    tokenPool.Add(ut);
            }
		}
	}
}

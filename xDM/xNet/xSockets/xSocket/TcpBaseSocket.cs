using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace xDM.xNet.xSockets.xSocket
{
    public abstract class TcpBaseSocket
    {
		
		protected Socket socket { get; set; } = null;
		public EndPoint RemoteEndPoint { get { return socket?.RemoteEndPoint; } }

		/// <summary>
		/// 显示信息
		/// </summary>
		public delegate void showMsg(string message);
		/// <summary>
		/// 处理发生错误
		/// </summary>
		public delegate void onError(object sender, Exception err);

        public delegate void serverHandlePackage(TcpServerSocket sender, string clientRemoteEndPoint, byte[] package);

        public delegate void clientHandlePackage(TcpClientSocket sender, byte[] package);
        /// <summary>
        /// 发送BufferSize
        /// </summary>
        public int SendBufferSize { get; set; } = 1024 * 64;
		/// <summary>
		/// 接收BufferSize
		/// </summary>
		public int ReciveBufferSize { get; set; } = 1024 * 64;

		/// <summary>
		/// 发送信息缓存
		/// </summary>
		protected ConcurrentDictionary<Guid, byte[]> dicSendedPackages { get; set; } = new ConcurrentDictionary<Guid, byte[]>();
		/// <summary>
		/// 接收信息缓存
		/// </summary>
		protected ConcurrentDictionary<Guid, byte[]> dicRevivedPackages { get; set; } = new ConcurrentDictionary<Guid, byte[]>();

		protected bool SendByteAsync(Socket worksocket, byte[] data)
		{
			if (worksocket == null)
				return false;
            if (!worksocket.Connected)
                return false;
			try
			{
				SocketAsyncEventArgs sendArg = SocketAsyncEventArgsPool.SendPool.Pop();
                //var dataList = new List<byte[]>();
				sendArg.SetBuffer(data, 0, data.Length);
				var result = worksocket.SendAsync(sendArg);
				if (!result)
					SendByteAsyncFailed(worksocket, "发送信息失败");
				return result;
			}
			catch (Exception err)
			{
				SendByteAsyncFailed(worksocket, err.Message);
				HandleError(err);
				return false;
			}
		}

		protected abstract void SendByteAsyncFailed(Socket worksocket,string message);
        protected abstract void HandleError(Exception err);
    }
}

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
		public Action<string> eShowMsg;
		/// <summary>
		/// 处理发生错误
		/// </summary>
		public Action<object, Exception> onError;
		/// <summary>
		/// 发送BufferSize
		/// </summary>
		public int SendBufferSize { get; set; } = 1024;
		/// <summary>
		/// 接收BufferSize
		/// </summary>
		public int ReciveBufferSize { get; set; } = 1024 * 2;

		/// <summary>
		/// 发送信息缓存
		/// </summary>
		protected ConcurrentDictionary<Guid, Message> dicSendedMessages { get; set; } = new ConcurrentDictionary<Guid, Message>();
		/// <summary>
		/// 接收信息缓存
		/// </summary>
		protected ConcurrentDictionary<Guid, Message> dicRevivedMessages { get; set; } = new ConcurrentDictionary<Guid, Message>();

		protected bool SendByteAsync(Socket worksocket, byte[] data)
		{
			if (worksocket == null)
				return false;
			try
			{
				SocketAsyncEventArgs sendArg = SocketAsyncEventArgsPool.SendPool.Get();
				sendArg.SetBuffer(data, 0, data.Length);
				var result = worksocket.SendAsync(sendArg);
				if (!result)
					SendByteAsyncFailed(worksocket, "发送信息失败");
				return result;
			}
			catch (Exception err)
			{
				SendByteAsyncFailed(worksocket, err.Message);
				onError?.BeginInvoke(this, err, null, null);
				if (err.GetType() == typeof(SocketException))
				{

				}
				return false;
			}
		}

		protected abstract void SendByteAsyncFailed(Socket worksocket,string message);
    }
}

using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace xDM.xNet.xSockets.xSocket
{
	public class ClientInfo
	{
		private Socket _socket { get; set; }
		public Socket socket { get { return _socket; } set { _socket = value; RemoteEndPoint = _socket?.RemoteEndPoint + ""; } }
		public string RemoteEndPoint { get; set; }
		public SocketAsyncEventArgs ReciveArgs { get; set; }
		public TcpServerSocketRecivedDataHandler ReciveHandler { get; set; }
		public DateTime LastWorkingTime { get; set; }

		private ClientInfo()
		{
			ReciveHandler = new TcpServerSocketRecivedDataHandler();
		}

		private static readonly ConcurrentQueue<ClientInfo> infoQueue = new ConcurrentQueue<ClientInfo>();
		public static ClientInfo Get()
		{
			ClientInfo info = null;
			if (infoQueue.Count == 0 || !infoQueue.TryDequeue(out info))
			{
				info = new ClientInfo();
			}
			if (infoQueue.Count < 2)
				new Action(() =>
				{
					var count = 8;
					while (--count > 0)
					{
						infoQueue.Enqueue(new ClientInfo());
					}
				}).BeginInvoke(null, null);
			return info;
		}
		public static void Push(ClientInfo info)
		{
			if (info != null)
				infoQueue.Enqueue(info);
		}
	}
}

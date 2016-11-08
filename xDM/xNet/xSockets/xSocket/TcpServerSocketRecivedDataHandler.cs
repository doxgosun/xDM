using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xDM.xNet.xSockets.xSocket
{
    public class TcpServerSocketRecivedDataHandler : BaseTcpRecivedDataHandler
    {
        public TcpServerSocket server;
        public Socket clientSocket;
        public Action<TcpServerSocket, string, Message> HandleMessage;

        /// <summary>
        /// 所有连接的客户端，RemoteEndPoint，Socket,DateTime
        /// </summary>
        public ConcurrentDictionary<string, KeyValuePair<Socket, DateTime>> ClientSocketDic { get; set; }

		protected override void hdMsg(Message msg)
		{
			DateTime workTime = DateTime.Now;
			DateTime updateTime = DateTime.MinValue;
			TimeSpan tsUpdate = new TimeSpan(0, 0, 1);
			workTime = DateTime.Now;
			if (DateTime.Now - updateTime > tsUpdate)
			{
				updateTime = DateTime.Now;
				KeyValuePair<Socket, DateTime> newKv = new KeyValuePair<Socket, DateTime>(clientSocket, DateTime.Now);
				KeyValuePair<Socket, DateTime> kv;
				if (ClientSocketDic.TryGetValue(clientSocket.RemoteEndPoint + "", out kv))
				{
					ClientSocketDic.TryUpdate(clientSocket.RemoteEndPoint + "", newKv, kv);
				}
			}
			this.HandleMessage?.BeginInvoke(server, clientSocket.RemoteEndPoint.ToString(), msg, null, null);
		}
	}
}

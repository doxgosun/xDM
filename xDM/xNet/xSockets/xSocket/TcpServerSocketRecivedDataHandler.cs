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
        public TcpServerSocket server { get; set; }
        public Socket clientSocket { get { return _clientSocket; }set { _clientSocket = value;RemoteEndPoint = _clientSocket?.RemoteEndPoint + ""; } }
        private Socket _clientSocket { get; set; }
        public Action<TcpServerSocket, string, Message> HandleMessage;
        private string RemoteEndPoint { get; set; }

        /// <summary>
        /// 所有连接的客户端，RemoteEndPoint，Socket,DateTime
        /// </summary>
        public ConcurrentDictionary<Socket, TcpServerSocket.ClientInfo> ClientSocketDic { get; set; }

		protected override void hdMsg(Message msg)
		{
            this.HandleMessage?.Invoke(server, RemoteEndPoint, msg);
		}

        protected override void HeartBeat()
        {
            TcpServerSocket.ClientInfo clientInfo = null;
            if (ClientSocketDic.TryGetValue(clientSocket, out clientInfo))
            {
                clientInfo.LastWorkingTime = DateTime.Now;
            }
        }
    }
}

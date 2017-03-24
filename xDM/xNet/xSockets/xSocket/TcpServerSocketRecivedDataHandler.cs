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
        public Action<TcpServerSocket, string, byte[]> HandlePackage;
        private string RemoteEndPoint { get; set; }

        /// <summary>
        /// 所有连接的客户端，RemoteEndPoint，Socket,DateTime
        /// </summary>
        public ConcurrentDictionary<Socket, UserToken> ClientSocketDic { get; set; }

		//protected override void hdPackage(byte[] package)
		//{
  //          this.HandlePackage?.Invoke(server, RemoteEndPoint, package);
  //          if (DateTime.Now - lastReviedTime > updateTimeSpan)
  //          {
  //              HeartBeat();
  //              lastReviedTime = DateTime.Now;
  //          }
		//}
        private DateTime lastReviedTime = DateTime.Now;
        private TimeSpan updateTimeSpan = new TimeSpan(0, 0, 2);

        protected override void HeartBeat()
        {
            UserToken clientInfo = null;
            if (ClientSocketDic.TryGetValue(clientSocket, out clientInfo))
            {
                clientInfo.LastWorkingTime = DateTime.Now;
            }
            var e = SocketAsyncEventArgsPool.SendPool.Pop();
            var heartbeat = new byte[6];
            heartbeat[0] = 0xff;
            heartbeat[1] = 0xc0;//1100 0000
            heartbeat[2] = 0;
            heartbeat[3] = 0;
            heartbeat[4] = 0;
            heartbeat[5] = (byte)((heartbeat[1] + heartbeat[2] + heartbeat[3] + heartbeat[4]) / 4);
            e.SetBuffer(heartbeat, 0, heartbeat.Length);
            server.SendDataAsync(RemoteEndPoint, heartbeat);
        }

        protected override Task hdRequest(byte[] package)
        {
            throw new NotImplementedException();
        }

        protected override Task hdResponse(byte[] package)
        {
            throw new NotImplementedException();
        }

        protected override Task hdPackage(byte[] package)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xNet.xSockets.xSocket
{
    class TcpClientSocketRecivedDataHandler : BaseTcpRecivedDataHandler
    {

        public TcpClientSocket client;
        public Socket server;
        public Action<TcpClientSocket, Message> HandleMessage;


        //protected override void hdMsg(Message msg)
        //{
        //    DateTime workTime = DateTime.Now;
        //    DateTime updateTime = DateTime.MinValue;
        //    TimeSpan tsUpdate = new TimeSpan(0, 0, 1);
        //    workTime = DateTime.Now;
        //    if (DateTime.Now - updateTime > tsUpdate)
        //    {
        //        updateTime = DateTime.Now;
        //    }
        //    this.HandleMessage?.Invoke(client, msg);
        //}

        protected override void HeartBeat()
        {
            
        }

        protected override Task hdPackage(byte[] package)
        {
            throw new NotImplementedException();
        }

        protected override Task hdRequest(byte[] package)
        {
            throw new NotImplementedException();
        }

        protected override Task hdResponse(byte[] package)
        {
            throw new NotImplementedException();
        }
    }
}

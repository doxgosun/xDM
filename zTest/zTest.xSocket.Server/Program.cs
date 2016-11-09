using System;
using System.Net;
using xDM.xNet.xSockets.xSocket;
using xDM.xNet.xSockets;
using xDM.xNet.xSockets.xSocket.Extensions;
using System.Threading;
using System.Data;


namespace zTest.xSocket.Server
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			DataTable dt = new DataTable();
			var dddd = dt.SerializeToByte();
            int len = 1234445566;
            byte[] d = new byte[4];
            d[0] = (Byte)((len & 0x7f000000) >> 24);
            d[1] = (Byte)((len & 0xff0000) >> 16);
            d[2] = (Byte)((len & 0xff00) >> 8);
            d[3] = (Byte)(len & 0xff);

            int reciveObjLength = d[3];
            reciveObjLength += (d[2] << 8);
            reciveObjLength += d[1] << 16;
            reciveObjLength += (d[0] & 0x7f) << 24;



            Message msg = new Message();

            var server = new TcpServerSocket();
			server.HandleMessage += Server_HandleMessage;
            server.eShowMsg += (s) => { Console.WriteLine(s); };
			if (server.Bind(23456))
				server.Listen();
			while (true)
			{
              //  if (rp != null)
                {
             //       server.SendMessageAsync(rp,msg);
                }
        //        else
                {
                    Thread.Sleep(1000);
                    Console.WriteLine($"收到：{reCount}  发出：{sendCount}");
                }
                //reCount = 0;
            }
		}
        static string rp = null;
		static int reCount = 0;
        static int sendCount = 0;
        static object lck = new object();
        static object lck2 = new object();
		static void Server_HandleMessage(TcpServerSocket sender, string repoint, Message msg)
		{
       //     if (rp == null)
        //        rp = repoint;
       //     lock(lck)
			    reCount++;
            //return;
			//Console.WriteLine($"收到的信息为：{msg.Value}");
		//	if (reCount % 2 == 0)
			{
				//msg.Value = $"{reCount}Server:SendMessageAsync:{DateTime.Now}";
				sender.SendMessageAsync(repoint, msg);
             //   lock(lck2)
                    sendCount++;
			}
	//		else
			{
			//	msg.Value = $"{reCount}Server:SendMessage:{DateTime.Now}";
				//sender.SendMessage(repoint, msg, 100000);
			}
		}
	}
}

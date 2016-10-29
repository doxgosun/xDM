using System;
using System.Net;
using xDM.xNet.xSockets.xSocket;
using xDM.xNet.xSockets;
using System.Threading;

namespace zTest.xSocket.Server
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var server = new TcpServerSocket();
			server.HandleMessage += Server_HandleMessage;
			if (server.Bind(23456))
				server.Listen();
			while (true)
			{
				Thread.Sleep(10);
			}
		}
		static int reCount = 0;
		static void Server_HandleMessage(TcpServerSocket sender, string repoint, Message msg)
		{
			reCount++;
			Console.WriteLine($"收到的信息为：{msg.Value}");
			if (reCount % 2 == 0)
			{
				msg.Value = $"{reCount}Server:SendMessageAsync:{DateTime.Now}";
				sender.SendMessageAsync(repoint, msg);
			}
			else
			{
				msg.Value = $"{reCount}Server:SendMessage:{DateTime.Now}";
				sender.SendMessage(repoint, msg, 100000);
			}
		}
	}
}

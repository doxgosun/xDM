using System;
using xDM.xNet.xSockets;
using xDM.xNet.xSockets.xSocket;
using System.Threading;

namespace zTest.xSocket.Client
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var client = new TcpClientSocket();
			client.HandleMessage += Client_HandleMessage;
			if (client.Connect("127.0.0.1", 23456))
			{				Message msg = new Message();
				while (true)
				{
					sendCount++;
					Thread.Sleep(1000);
					if (sendCount % 2 == 0)
					{
						msg.Value = $"{sendCount}Client: SendMessgae:{DateTime.Now}";
						client.SendMessage(msg, 10000);
					}
					else
					{
						msg.Value = $"{sendCount}Client: SendMessgaeSync:{DateTime.Now}";
						client.SendMessageAsync(msg);
					}
				}
			}
		}
		static int sendCount = 0;
		static void Client_HandleMessage(TcpClientSocket sender, Message msg)
		{
			Console.WriteLine($"收到的信息为：{msg.Value}");
		}
	}
}

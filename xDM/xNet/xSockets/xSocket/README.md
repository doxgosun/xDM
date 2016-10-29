# xDM.xNet.xSockets.xSocket

tcp socket 的一个简单封装，原来用于公司生产环境，一直稳定，但并发等没测试过，有心人可以测试一下，并将结果告知一声。

可用于windows,mac,(Linux暂时未测试，理论上mac的mono可以运行，linux也是可以的)

项目总目录：https://github.com/doxgosun/xDM

DLL下载：https://github.com/doxgosun/xDM/tree/master/xBuild

使用方法：

引用xDM.xNet.xSockets.xSocket.dll

using xDM.xNet.xSockets;
using xDM.xNet.xSockets.xSocket;

//服务器端示例代码

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

//客户端示例代码

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


已在 Windows10 + MacOS10.12.1 测试通过

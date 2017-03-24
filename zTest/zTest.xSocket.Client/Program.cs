using System;
using xDM.xNet.xSockets;
using xDM.xNet.xSockets.xSocket;
using System.Threading;
using xDM.xCommon.xExtensions;
using System.Threading.Tasks;

namespace zTest.xSocket.Client
{
	class MainClass
	{
		public static void Main(string[] args)
		{
            object tLck = new object();
            var tCount = 0;
            int lastSended = 0, lastRec = 0;

            while (true)
            {
                if (tCount < 1)
                {
                    var action = new Action(() =>
                    {
                        var client = new TcpClientSocket();
                     //   client.HandleMessage += Client_HandleMessage;
                        if (client.Connect("10.202.196.51", 23456))
                        {
                            lock (tLck)
                                tCount++;
                            Message msg = new Message();
                            //client.SendMessageAsync(msg);
                            var loop = 100000000;
                            Thread.Sleep(1000);
                            while (loop-- > 0)
                            {

                              //  Thread.Sleep(300);
                                //  if (sendCount % 2 == 0)
                                {
                                    msg.Value = $"{sendCount}Client: SendMessgae:{DateTime.Now}";
                                    //client.SendMessage(msg, 10000);
                                }
                                //    	else
                                {
                                    msg.Value = $"{sendCount}Client: SendMessgaeSync:{DateTime.Now}";
                                    if (client.SendMessageAsync(msg))
                                        lock (lck2)
                                            sendCount++;
                                    else
                                    {
                                        lock (tLck)
                                            tCount--;
                                        break;
                                    }
                                }
                            }
                        }
                        client.Dispose();
                    });
                    Task task = new Task(action);
                    task.Start();
                }
                Console.WriteLine($"发出：{sendCount}  收到：{recCount}  线程数：{tCount}");
                Console.Title = $"发出：{sendCount - lastSended}  接收：{recCount - lastRec}  线程数：{tCount}";
                lastSended = sendCount;
                lastRec = recCount;
                //sendCount = 0;
                Thread.Sleep(1000);
            }
		}
		static int sendCount = 0;
        static int recCount = 0;
        static object lck = new object();
        static object lck2 = new object();
		static void Client_HandleMessage(TcpClientSocket sender, Message msg)
		{
            lock(lck)
                recCount++;
			//Console.WriteLine($"收到的信息为：{msg.Value}");
		}
	}
}

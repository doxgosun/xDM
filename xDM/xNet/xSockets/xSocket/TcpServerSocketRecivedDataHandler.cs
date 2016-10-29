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
    public class TcpServerSocketRecivedDataHandler
    {
        public readonly ConcurrentQueue<byte[]> dataQueue = new ConcurrentQueue<byte[]>();
        private readonly ConcurrentQueue<Message> msgQueue = new ConcurrentQueue<Message>();
        public TcpServerSocket server;
        public Socket clientSocket;
        public Action<TcpServerSocket, string, Message> HandleMessage;
        public bool Quit = false;
        /// <summary>
        /// 发送信息缓存
        /// </summary>
        public ConcurrentDictionary<string, Message> dicSendedMessages { get; set; }
        /// <summary>
        /// 接收信息缓存
        /// </summary>
        public ConcurrentDictionary<string, Message> dicRevivedMessages { get; set; }
        /// <summary>
        /// 所有连接的客户端，RemoteEndPoint，Socket,DateTime
        /// </summary>
        public ConcurrentDictionary<string, KeyValuePair<Socket, DateTime>> ClientSocketDic { get; set; }
        public TcpServerSocketRecivedDataHandler()
        {
            Thread thd = new Thread(hdData);
            thd.IsBackground = true;
            thd.Start();
            Thread thdMsg = new Thread(hdMsg);
            thdMsg.IsBackground = true;
            thdMsg.Start();
        }

        private void hdData()
        {
            string msgTmp = "";
            DateTime workTime = DateTime.Now;
            TimeSpan ts = new TimeSpan(0, 1, 0);
            byte[] d;
            Message[] msgs;
            Message baseMsg = new Message();
            var baseLength = baseMsg.ToSendString().Length;
            while (!Quit || DateTime.Now - workTime > ts)
            {
                while (dataQueue.Count > 0)
                {
                    workTime = DateTime.Now;
                    if (dataQueue.TryDequeue(out d))
                    {
                        string reMsg = Encoding.UTF8.GetString(d, 0, d.Length);
                        msgTmp += reMsg;
                        if (msgTmp.Length >= baseLength)
                        {
                            msgs = MessageExt.GetMessages(msgTmp);
                            if (msgs.Length > 0)
                            {
                                foreach (Message msg in msgs)
                                {
                                    if (msg.MessageGuid + "" != "")
                                    {
                                        Message tmpMsg;
                                        if (dicSendedMessages.TryRemove(msg.MessageGuid, out tmpMsg))
                                        {
                                            dicRevivedMessages.TryAdd(msg.MessageGuid, msg);
                                        }
                                    }
                                    msgQueue.Enqueue(msg);
                                }
                                msgTmp = MessageExt.GetStringNotMessage(msgTmp);
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
                Thread.Sleep(10);
            }
        }

        private void hdMsg()
        {
            DateTime workTime = DateTime.Now;
            DateTime updateTime = DateTime.MinValue;
            TimeSpan ts = new TimeSpan(0, 1, 0);
            TimeSpan tsUpdate = new TimeSpan(0, 0, 1);
            Message msg;
            while (!Quit || DateTime.Now - workTime > ts)
            {
                while (msgQueue.Count > 0)
                {
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
                    if (msgQueue.TryDequeue(out msg))
                    {
                        this.HandleMessage?.Invoke(server, clientSocket.RemoteEndPoint.ToString(), msg);
                    }
                }
                Thread.Sleep(10);
            }
        }
    }
}

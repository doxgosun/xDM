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
        private readonly ConcurrentQueue<byte[]> filesQueue = new ConcurrentQueue<byte[]>();
        public TcpServerSocket server;
        public Socket clientSocket;
        public Action<TcpServerSocket, string, Message> HandleMessage;
        public bool Quit = false;
        /// <summary>
        /// 发送信息缓存
        /// </summary>
        public ConcurrentDictionary<Guid, Message> dicSendedMessages { get; set; }
        /// <summary>
        /// 接收信息缓存
        /// </summary>
        public ConcurrentDictionary<Guid, Message> dicRevivedMessages { get; set; }
        /// <summary>
        /// 所有连接的客户端，RemoteEndPoint，Socket,DateTime
        /// </summary>
        public ConcurrentDictionary<string, KeyValuePair<Socket, DateTime>> ClientSocketDic { get; set; }
        public TcpServerSocketRecivedDataHandler()
        {
            Thread thd = new Thread(hdBytes);
            thd.IsBackground = true;
            thd.Start();
            Thread thdMsg = new Thread(hdMsg);
            thdMsg.IsBackground = true;
            thdMsg.Start();
        }

        private void hdBytes()
        {
            string msgTmp = "";
            DateTime workTime = DateTime.Now;
            TimeSpan ts = new TimeSpan(0, 1, 0);
            byte[] d = null;
            byte[] msgObj = null;
            byte[] tmpByte = new byte[0];
            int reciveObjLength = -1;
            int fileIndex = -1;
            while (!Quit || DateTime.Now - workTime > ts)
            {
                while (dataQueue.Count > 0)
                {
                    workTime = DateTime.Now;
                    if (dataQueue.TryDequeue(out d))
                    {
                        var tmp = d;
                        if (tmpByte.Length > 0)
                        {
                            tmp = new byte[tmpByte.Length + d.Length];
                            Array.Copy(tmpByte, 0, tmp, 0, tmpByte.Length);
                            Array.Copy(d, 0, tmp, tmpByte.Length, d.Length);
                        }
                        if (msgObj == null) //上个对象已传送完毕对象
                        {
                            tmpByte = hdNewBytes(tmp,0, out msgObj, out reciveObjLength);
                        }
                        else //继续接收上个对象
                        {

                            tmpByte = addMsgObjBytes(tmp,0, ref msgObj, ref reciveObjLength);
                        }
                    }
                   // Thread.Sleep(1);
                }
                Thread.Sleep(1);
            }
        }

        private byte[] addMsgObjBytes(byte[] d, int index, ref byte[] msgObj, ref int reciveObjLength)
        {
            byte[] tmpByte = new byte[0];
            var len = msgObj.Length - reciveObjLength;
            if (len <= d.Length - index) //接收的数据超过对象大小
            {
                Array.Copy(d, index, msgObj, reciveObjLength, len);
                var msg = MessageExt.GetMessage(msgObj);
                if (msg != null)
                {
                    if (msg.GetGuid() != Guid.Empty)
                    {
                        Message tmpMsg;
                        if (dicSendedMessages.TryRemove(msg.GetGuid(), out tmpMsg))
                        {
                            dicRevivedMessages.TryAdd(msg.GetGuid(), msg);
                        }
                    }
                    msgQueue.Enqueue(msg);
                }
                msgObj = null;
                len = d.Length - len - index;
                if (len > 0)
                {
                    if (len < 4)
                    {
                        tmpByte = new byte[len];
                        Array.Copy(d, d.Length - len, tmpByte, 0, len);
                    }
                    else
                    {
                        return hdNewBytes(d,d.Length - len,out msgObj,out reciveObjLength);
                    }
                }
            }
            else
            {
                len = d.Length - index;
                Array.Copy(d, index, msgObj, reciveObjLength, len);
                reciveObjLength += len;
            }
            return tmpByte;
        }

        private byte[] hdNewBytes(byte[] d, int index, out byte[] msgObj,out int reciveObjLength)
        {
            msgObj = null;
            reciveObjLength = 0;
            if (d.Length - index < 4)
            {
                var tmpByte = new byte[d.Length - index];
                Array.Copy(d, index, tmpByte, 0, tmpByte.Length);
                return tmpByte;
            }           
            if (d[index] >> 7 == 0) //是对象
            {
                reciveObjLength = d[index + 3];
                reciveObjLength += (d[index + 2] << 8);
                reciveObjLength += d[index + 1] << 16;
                reciveObjLength += (d[index] & 0x7f) << 24;
                msgObj = new byte[reciveObjLength];
                reciveObjLength = 0;
                return addMsgObjBytes(d,index + 4,ref msgObj,ref reciveObjLength);
            }
            else // 是文件
            {
                return new byte[0];
            }
        }

        private void dhFiles()
        {

        }

        private void hdMsg()
        {
            DateTime workTime = DateTime.Now;
            DateTime updateTime = DateTime.MinValue;
            TimeSpan ts = new TimeSpan(1, 1, 0);
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
                        this.HandleMessage?.BeginInvoke(server, clientSocket.RemoteEndPoint.ToString(), msg,null,null);
                    }
                }
                Thread.Sleep(1);
            }
        }
    }
}

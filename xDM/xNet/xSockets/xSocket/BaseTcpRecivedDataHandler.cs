using System;
using System.Collections.Concurrent;
using System.Threading;

namespace xDM.xNet.xSockets.xSocket
{
	public abstract class BaseTcpRecivedDataHandler
	{
		public readonly ConcurrentQueue<byte[]> dataQueue = new ConcurrentQueue<byte[]>();
		private readonly ConcurrentQueue<byte[]> msgBytesQueue = new ConcurrentQueue<byte[]>();
		private readonly ConcurrentQueue<byte[]> filesQueue = new ConcurrentQueue<byte[]>();
		/// <summary>
		/// 发送信息缓存
		/// </summary>
		public ConcurrentDictionary<Guid, Message> dicSendedMessages { get; set; }
		/// <summary>
		/// 接收信息缓存
		/// </summary>
		public ConcurrentDictionary<Guid, Message> dicRevivedMessages { get; set; }


		private bool _isWorking = false;

        public void Stop()
        {
            _isWorking = false;
            byte[] tmp;
            while (!dataQueue.IsEmpty)
                dataQueue.TryDequeue(out tmp);
            while (!msgBytesQueue.IsEmpty)
                msgBytesQueue.TryDequeue(out tmp);
            while (!filesQueue.IsEmpty)
                filesQueue.TryDequeue(out tmp);
            tmp = null;
            GC.Collect();
        }

        public void Start()
        {
            if (_isWorking)
                return;
            _isWorking = true;
            StartHandleMsgBytesThread();
            Thread thd = new Thread(hdData);
            thd.IsBackground = true;
            thd.Start();
        }

		public BaseTcpRecivedDataHandler()
		{

		}

        private int _maxHdMsgBytesThreadCount = 32;
		private int hdMsgBytesThredCount = 0;
		private object hdMsgBytesThreadLock = new object();
		private void StartHandleMsgBytesThread()
		{
			if (hdMsgBytesThredCount > _maxHdMsgBytesThreadCount)
				return;
			lock (hdMsgBytesThreadLock)
			{
                if (hdMsgBytesThredCount > _maxHdMsgBytesThreadCount)
                    return;
                hdMsgBytesThredCount++;
			    Thread thdMsg = new Thread(hdMsgBytes);
			    thdMsg.IsBackground = true;
			    thdMsg.Start();
			}
		}

		private void hdData()
		{
			DateTime workTime = DateTime.Now;
			TimeSpan ts = new TimeSpan(0, 1, 0);
			byte[] d = null;
			byte[] msgObj = null;
			byte[] tmpByte = new byte[0];
			int reciveObjLength = -1;
			int fileIndex = -1;
			while (_isWorking || DateTime.Now - workTime > ts)
			{
				while (_isWorking && dataQueue.Count > 0)
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
							tmpByte = hdNewBytes(tmp, 0, out msgObj, out reciveObjLength);
						}
						else //继续接收上个对象
						{

							tmpByte = addMsgObjBytes(tmp, 0, ref msgObj, ref reciveObjLength);
						}
					}
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
				msgBytesQueue.Enqueue(msgObj);
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
						return hdNewBytes(d, d.Length - len, out msgObj, out reciveObjLength);
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

		private byte[] hdNewBytes(byte[] d, int index, out byte[] msgObj, out int reciveObjLength)
		{
			msgObj = null;
			reciveObjLength = 0;
			if (d.Length - index < 6)
			{
				var tmpByte = new byte[d.Length - index];
				Array.Copy(d, index, tmpByte, 0, tmpByte.Length);
				return tmpByte;
			}
            var check = d[index + 1] + d[index + 2] + d[index + 3] + d[index + 4];
            check /= 4;
            if (d[index] != 0xff || d[index + 5] != check)
            {
                return hdNewBytes(d, index++, out msgObj, out reciveObjLength);
            }
			if (d[index + 1] >> 6 == 0) //是对象
			{
				reciveObjLength = d[index + 4];
				reciveObjLength += (d[index + 3] << 8);
				reciveObjLength += d[index + 2] << 16;
				reciveObjLength += (d[index + 1] & 0x3f) << 24;
				msgObj = new byte[reciveObjLength];
				reciveObjLength = 0;
				return addMsgObjBytes(d, index + 6, ref msgObj, ref reciveObjLength);
			}
			else if(d[index + 1] >> 6 == 3) // 是心跳
			{
                HeartBeat();
                return hdNewBytes(d, index + 6, out msgObj, out reciveObjLength);
            }
            return new byte[0];
		}

		private void dhFiles()
		{

		}

		private void hdMsgBytes()
		{
			while (_isWorking)
			{
				if (msgBytesQueue.Count < 100 & hdMsgBytesThredCount > 1)
				{
					lock (hdMsgBytesThreadLock)
					{
						if (msgBytesQueue.Count < 100 & hdMsgBytesThredCount > 1)
						{
							hdMsgBytesThredCount--;
							return;
						}
					}
				}
				while (_isWorking && msgBytesQueue.Count > 0)
				{
                    if (hdMsgBytesThredCount < _maxHdMsgBytesThreadCount && msgBytesQueue.Count > 1000)
                    {
                        new Action(() =>
                        {
                            Thread.Sleep(2000);
                            if (hdMsgBytesThredCount < _maxHdMsgBytesThreadCount && msgBytesQueue.Count > 1000)
                                StartHandleMsgBytesThread();
                        }).BeginInvoke(null, null);
                    }
                    byte[] msgBytes;
                    if (msgBytesQueue.TryDequeue(out msgBytes))
                    {
                        var msg = MessageExt.GetMessage(msgBytes);
                        if (msg != null)
                        {
                            if (msg.GetGuid() != Guid.Empty)
                            {
                                Message tmpMsg;
                                if (dicSendedMessages.TryRemove(msg.GetGuid(), out tmpMsg))
                                    dicRevivedMessages.TryAdd(msg.GetGuid(), msg);
                            }
                            hdMsg(msg);
                        }
                    }
				}
				Thread.Sleep(1);
			}
		}
		protected abstract void hdMsg(Message msg);
        protected abstract void HeartBeat();
	}
}
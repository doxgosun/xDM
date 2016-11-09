using System;
using System.Collections.Concurrent;
using System.Threading;

namespace xDM.xNet.xSockets.xSocket
{
	public abstract class BaseTcpRecivedDataHandler
	{
		public readonly ConcurrentQueue<byte[]> dataQueue = new ConcurrentQueue<byte[]>();
		protected readonly ConcurrentQueue<byte[]> msgBytesQueue = new ConcurrentQueue<byte[]>();
		protected readonly ConcurrentQueue<byte[]> filesQueue = new ConcurrentQueue<byte[]>();
		/// <summary>
		/// 发送信息缓存
		/// </summary>
		public ConcurrentDictionary<Guid, Message> dicSendedMessages { get; set; }
		/// <summary>
		/// 接收信息缓存
		/// </summary>
		public ConcurrentDictionary<Guid, Message> dicRevivedMessages { get; set; }


		public bool Quit = false;

		public BaseTcpRecivedDataHandler()
		{
			StartHandleMsgBytesThread();
			Thread thd = new Thread(hdData);
			thd.IsBackground = true;
			thd.Start();
		}

        private int _maxHdMsgThreadCount = 4;
		private int hdMsgThredCount = 0;
		private object hdMsgThreadLock = new object();
		private void StartHandleMsgBytesThread()
		{
			if (hdMsgThredCount > _maxHdMsgThreadCount)
				return;
			lock (hdMsgThreadLock)
			{
				hdMsgThredCount++;
			}
			Thread thdMsg = new Thread(hdMsgBytes);
			thdMsg.IsBackground = true;
			thdMsg.Start();
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
				return addMsgObjBytes(d, index + 4, ref msgObj, ref reciveObjLength);
			}
			else // 是文件
			{
				return new byte[0];
			}
		}

		private void dhFiles()
		{

		}

		private void hdMsgBytes()
		{
			while (!Quit)
			{
				if (msgBytesQueue.Count < 100 & hdMsgThredCount > 1)
				{
					lock (hdMsgThreadLock)
					{
						if (msgBytesQueue.Count < 100 & hdMsgThredCount > 1)
						{
							hdMsgThredCount--;
							return;
						}
					}
				}
				while (msgBytesQueue.Count > 0)
				{
                    if (hdMsgThredCount < _maxHdMsgThreadCount && msgBytesQueue.Count > 1000)
                    {
                        new Action(() =>
                        {
                            Thread.Sleep(2000);
                            if (hdMsgThredCount < _maxHdMsgThreadCount && msgBytesQueue.Count > 1000)
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
						}
						hdMsg(msg);
					}
				}
				Thread.Sleep(1);
			}
		}
		protected abstract void hdMsg(Message msg);
	}
}
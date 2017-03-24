using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace xDM.xNet.xSockets.xSocket
{
	public abstract class BaseTcpRecivedDataHandler
	{
        public readonly ConcurrentQueue<KeyValuePair<Guid, byte[]>> recivePackages = new ConcurrentQueue<KeyValuePair<Guid, byte[]>>();
        /// <summary>
        /// 请求包
        /// </summary>
        private readonly ConcurrentDictionary<Guid, byte[]> requestPackages = new ConcurrentDictionary<Guid, byte[]>();
        /// <summary>
        /// 回复包
        /// </summary>
        private readonly ConcurrentDictionary<Guid, byte[]> responsePackages = new ConcurrentDictionary<Guid, byte[]>();
		private readonly ConcurrentQueue<byte[]> filesQueue = new ConcurrentQueue<byte[]>();

		/// <summary>
		/// 发送信息缓存
		/// </summary>
		public ConcurrentDictionary<Guid, byte[]> dicSendedMessages { get; set; }
		/// <summary>
		/// 接收信息缓存
		/// </summary>
		public ConcurrentDictionary<Guid, byte[]> dicRevivedMessages { get; set; }


		private bool _isWorking = false;

        public void Stop()
        {
            _isWorking = false;
        }

        public void Start()
        {
            if (_isWorking)
                return;
            _isWorking = true;
            StartHandleMsgBytesThread();
        }

		public BaseTcpRecivedDataHandler()
		{

		}

        private int _maxHdMsgBytesThreadCount = 16;
		private int hdMsgBytesThredCount = 0;
		private object hdMsgBytesThreadLock = new object();
		private void StartHandleMsgBytesThread()
		{
			//if (hdMsgBytesThredCount > _maxHdMsgBytesThreadCount)
			//	return;
			//lock (hdMsgBytesThreadLock)
			//{
   //             if (hdMsgBytesThredCount > _maxHdMsgBytesThreadCount)
   //                 return;
   //             hdMsgBytesThredCount++;
   //             Thread thdMsg = new Thread(hdMsgBytes);
   //             thdMsg.IsBackground = true;
   //             thdMsg.Start();
   //         }
   //         var kv = new KeyValuePair<Guid, byte[]>(null,null);

        }

        byte[] tmp = null;
		byte[] msgObj = null;
		byte[] tmpByte = new byte[0];
		int reciveObjLength = -1;
		int fileIndex = -1;
        int packageType = 0;
        List<byte> guid = new List<byte>();
        public void hdData(byte[] d)
        {
            tmp = d;
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
                tmpByte = addMsgObjBytes(tmp, 0, ref msgObj, ref reciveObjLength,packageType);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="index"></param>
        /// <param name="msgObj"></param>
        /// <param name="reciveObjLength"></param>
        /// <param name="packageType">000 请求 001回复 010（2）普通 011（3）。。。 （4、5）未使用 110（6）文件 111（7）心跳,心跳包没有数据体</param>
        /// <returns></returns>
		private byte[] addMsgObjBytes(byte[] d, int index, ref byte[] msgObj, ref int reciveObjLength, int packageType)
		{
			byte[] tmpByte = new byte[0];
   //         if (packageType <= 2 && guid.Count < 16)
   //         {
   //             if (d.Length - index < 16)
   //             {
   //                 for (int i = index;  i < d.Length; i++)
   //                     guid.Add(d[i]);
   //                 return tmpByte;
   //             }
   //             else
   //             {
   //                 index = index + 16;
   //                 for (int i = index - 16; i < index; i++)
   //                     guid.Add(d[i]);
   //             }
   //         }
			//var len = msgObj.Length - reciveObjLength;
			//if (len <= d.Length - index) //接收的数据等于或超过对象大小
			//{
			//	Array.Copy(d, index, msgObj, reciveObjLength, len);
   //             if (packageType == 0)
   //                 requestPackages.TryAdd(new Guid(guid.ToArray()), msgObj);
   //             else if (packageType == 1)
   //                 responsePackages.TryUpdate(new Guid(guid.ToArray()), msgObj, null);
   //             else if(packageType == 2)
			//	    recivedPackages.Enqueue(msgObj);
			//	msgObj = null;
   //             guid.Clear();
			//	len = d.Length - len - index;
			//	if (len > 0)
			//	{
			//		if (len < 6)
			//		{
			//			tmpByte = new byte[len];
			//			Array.Copy(d, d.Length - len, tmpByte, 0, len);
			//		}
			//		else
			//		{
			//			return hdNewBytes(d, d.Length - len, out msgObj, out reciveObjLength);
			//		}
			//	}
			//}
			//else
			//{
			//	len = d.Length - index;
			//	Array.Copy(d, index, msgObj, reciveObjLength, len);
			//	reciveObjLength += len;
			//}
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
            if (check == 0 || d[index] != 0xff || d[index + 5] != check)
            {
                return hdNewBytes(d, index + 1, out msgObj, out reciveObjLength);
            }
            var type = d[index + 1] >> 6;
            //000 请求 001回复 010（2）普通 011（3）。。。 （4、5）未使用 110（6）文件 111（7）心跳,心跳包没有数据体
            if (type < 3) //是Message对象
			{
				reciveObjLength = d[index + 4];
				reciveObjLength += (d[index + 3] << 8);
				reciveObjLength += d[index + 2] << 16;
				reciveObjLength += (d[index + 1] & 0x1f) << 24;
				msgObj = new byte[reciveObjLength];
				reciveObjLength = 0;
                return addMsgObjBytes(d, index + 6, ref msgObj, ref reciveObjLength, type);
			}
            else if (type == 6) //是文件
            {

            }
			else if(type == 7) // 是心跳
			{
                HeartBeat();
                return hdNewBytes(d, index + 6, out msgObj, out reciveObjLength);
            }
            return new byte[0];
		}

		private void dhFiles()
		{

		}

		private async Task hdMsgBytes(byte[] package)
		{
            lock (hdMsgBytesThreadLock)
            {
                hdMsgBytesThredCount++;
            }
            await hdPackage(package);
            lock (hdMsgBytesThreadLock)
            {
                hdMsgBytesThredCount--;
            }
        }

		protected abstract Task hdPackage(byte[] package);
        protected abstract Task hdRequest(byte[] package);
        protected abstract Task hdResponse(byte[] package);
        protected abstract void HeartBeat();
	}
}
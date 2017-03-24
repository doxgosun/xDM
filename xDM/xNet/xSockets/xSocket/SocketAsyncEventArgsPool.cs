using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace xDM.xNet.xSockets.xSocket
{
    public class SocketAsyncEventArgsPool
    {
        public class Pool
        {
            private ConcurrentBag<SocketAsyncEventArgs> m_completed_pool = new ConcurrentBag<SocketAsyncEventArgs>();
            public SocketAsyncEventArgs Pop()
            {
                SocketAsyncEventArgs saea = null;
                if (m_completed_pool.IsEmpty || !m_completed_pool.TryTake(out saea))
                {
                    saea = new SocketAsyncEventArgs();
                    saea.Completed += Saea_Completed;
                }
                return saea;
            }

			private void Saea_Completed(object sender, SocketAsyncEventArgs e)
			{
				e.SetBuffer(new byte[0], 0, 0);
				e.AcceptSocket = null;
                Push(e);
            }

            public void Push(SocketAsyncEventArgs saea)
            {
                m_completed_pool.Add(saea);
            }
        }

        private static Pool m_send_pool = new Pool();
        public static Pool SendPool { get { return m_send_pool; } }
    }
}

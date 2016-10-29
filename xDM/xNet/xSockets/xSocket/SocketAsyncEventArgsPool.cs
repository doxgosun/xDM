using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xNet.xSockets.xSocket
{
    public class SocketAsyncEventArgsPool
    {
        public class Pool
        {
            private ConcurrentQueue<SocketAsyncEventArgs> m_completed_queue = new ConcurrentQueue<SocketAsyncEventArgs>();
            public SocketAsyncEventArgs Get()
            {
                SocketAsyncEventArgs saea = null;
                if (!m_completed_queue.TryDequeue(out saea))
                {
                    saea = new SocketAsyncEventArgs();
                    saea.Completed += Saea_Completed;
                }
                return saea;
            }
            //public SocketAsyncEventArgs Get(EventHandler handler)
            //{
            //    SocketAsyncEventArgs saea = null;
            //    if (!m_completed_queue.TryDequeue(out saea))
            //    {
            //        saea = new SocketAsyncEventArgs();
            //        saea.Completed += Saea_Completed;
            //    }
            //    return saea;
            //}

            private void Saea_Completed(object sender, SocketAsyncEventArgs e)
            {
                Push(e);
            }

            public void Push(SocketAsyncEventArgs saea)
            {
                m_completed_queue.Enqueue(saea);
            }
        }

        private static Pool m_send_pool = new Pool();
        public static Pool SendPool { get { return m_send_pool; } }

        //private static Pool m_recive_pool = new Pool();
        //public static Pool RecivePool { get { return m_recive_pool; } }

        //private static Pool m_Accept_pool = new Pool();
        //public static Pool AcceptPool { get { return m_Accept_pool; } }
    }
}

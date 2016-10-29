using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDM.xNet.xSockets;
using System.Data.SqlClient;
using xDM.xData.xClient;
using xDM.xNet.xSockets.xSocket;

namespace zTest.Colsole
{
    class Program
    {
        static void Main(string[] args)
        {

			TcpServerSocket server = new TcpServerSocket();
			if(server.Bind(8876))
				server.Listen(100);

            //TestDelegateBuilder.Test();
            Console.ReadKey();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDM.xNet.xSockets;
using System.Data.SqlClient;
using xDM.xData.xClient;

namespace zTest.Colsole
{
    class Program
    {
        static void Main(string[] args)
        {

            var client = new DataClient(ClientType.SQLServer);

            TestDelegateBuilder.Test();
            Console.ReadKey();
        }
    }
}

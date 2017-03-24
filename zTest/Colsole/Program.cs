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

            using (DataClient client = new DataClient(ClientType.Vertica))
            {
                client.SetConnectionString("10.202.196.52", -1, "dbadmin", "gtgaj", "smz");
            }
            Console.ReadKey();
        }
    }
}

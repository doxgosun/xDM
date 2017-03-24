using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace xDM.xData.xClient
{
    public class ClientPool
    {
        /// <summary>
        /// key: $"{dbType}{connectionString}" value:ConcurrentBag<DataClient>
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConcurrentBag<DataClient>> dicConnectionStringClient = new ConcurrentDictionary<string, ConcurrentBag<DataClient>>();
        /// <summary>
        /// 池中保持连接的最大数值，超过此值则Push时直接丢弃，默认为10；
        /// </summary>
        public static int MaxCount { get; set; } = 10;
        /// <summary>
        /// 保持连接的最小连接数，此数目的client不会自动断开连接,直到服务器超时；
        /// </summary>
        //public static int MinCount { get; set; } = 2;
        /// <summary>
        /// 超时则维持最小连接数
        /// </summary>
        //public static int TimeOutSeconds { get; set; } = 300;

        public static DataClient Get(string dbType, string connectionString)
        {
            DataClient client = null;
            ConcurrentBag<DataClient> clientBag = null;
            var key = $"{dbType}{connectionString}";
            if (dicConnectionStringClient.TryGetValue(key, out clientBag))
                clientBag.TryTake(out client);
            else
                dicConnectionStringClient.TryAdd(key, new ConcurrentBag<DataClient>());
            if(client == null)
                client = new DataClient(dbType, connectionString);
            client.Open();
            return client;
        }
        public static DataClient Get(string dbType, string ip, int port, string user, string password,string database)
        {
            return Get(dbType,Common.GetConnectionString(dbType, ip, port, user, password, database));
        }

        public static void Push(DataClient client)
        {
            ConcurrentBag<DataClient> clientBag = null;
            var key = $"{client.DbType}{client.ConnectionString}";
            if (dicConnectionStringClient.TryGetValue(key, out clientBag))
            {
                if (clientBag.Count < MaxCount)
                    clientBag.Add(client);
                else
                    client.Dispose();
            }
            else
            {
                dicConnectionStringClient.TryAdd(key, new ConcurrentBag<DataClient>());
                Push(client);
            }
        }

        public static void Clear(string dbType, string connectionString)
        {
            DataClient client = null;
            ConcurrentBag<DataClient> clientBag = null;
            var key = $"{dbType}{connectionString}";
            while (dicConnectionStringClient.TryRemove(key, out clientBag))
            {
                while (clientBag.TryTake(out client))
                    client.Dispose();
                clientBag = null;
            }
        }

        public static void Clear(string dbType, string ip, int port, string user, string password, string database)
        {
            Clear(dbType, Common.GetConnectionString(dbType, ip, port, user, password, database));
        }

        public static void ClearAll()
        {
            DataClient client = null;
            ConcurrentBag<DataClient> clientBag = null;
            foreach (var kv in dicConnectionStringClient)
            {
                client = null;
                clientBag = kv.Value;
                while (clientBag.TryTake(out client))
                    client.Dispose();
            }
            client = null;
            clientBag = null;
            dicConnectionStringClient.Clear();
        }
    }
}

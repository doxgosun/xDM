using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xData.xClient
{
    public static class Common
    {
        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="type">数据库类型</param>
        /// <param name="ipORfilePath">服务器地址或文件地址</param>
        /// <param name="port">access 2007格式请用7或者2007,03格式请用2003或3,默认为03格式. 其他数据库如果小于等于0，则按默认值：SqlServer:1433  MySql:3306  Oracle:1521  Vertica:5433</param>
        /// <param name="user">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="database">数据库名称</param>
        /// <returns></returns>
        public static string GetConnectionString(ClientType type, string ipORfilePath, int port, string user, string password, string database)
        {
            string strConn = "";
            switch (type)
            {
                case ClientType.MySql:
                    {
                        if (port <= 0) port = DBPort.MySql;
                        strConn = string.Format("Persist Security Info=False;server={0};Port={4};user id={1};pwd='{2}';database={3};charset=utf8;",
                            ipORfilePath, user, password, database, port);
                    }
                    break;
                case ClientType.SQLServer:
                    {
                        if (port <= 0) port = DBPort.SQLServer;
                        strConn = string.Format("Server={0},{4};Database={1};Uid={2};PWD='{3}';",
                            ipORfilePath, database, user, password, port);
                    }
                    break;
                case ClientType.SQLite:
                    {
                        strConn = string.Format("Data Source={0};Password='{1}';Pooling=true;FailIfMissing=false",
                            ipORfilePath, password);
                    }
                    break;
                case ClientType.Vertica:
                    {
                        if (port <= 0) port = DBPort.Vertica;
                        strConn = string.Format("Host={0};Port={1};Database={2};User={3};Password='{4}';",
                            ipORfilePath, port, database, user, password); //Port:5433
                    }
                    break;
                case ClientType.Oracle:
                    {
                        //这里没测试,不知道对不对
                        if (port <= 0) port = DBPort.Oracle;
                        //strConn = string.Format("Data Source={0} {1};Intergrated Security=yes;User ID={2};Password='{3}';", ipORfilePath, port, user, password);
                        strConn = string.Format("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SID={4})));User Id={2};Password={3};", ipORfilePath, port, user, password, database);
                    }
                    break;
                case ClientType.Access:
                    {
                        //这里注意07版本的Access和03版本的连接字符串不同。不找了，很少用到，到时再说吧。
                        if (port == 7 || port == 2007)
                        {
                            strConn = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Jet OleDb:DataBase Password='{1}';",
                                ipORfilePath, password);
                        }
                        else
                        {
                            strConn = string.Format("Provider=Microsoft.Ace.OLEDB.12.0;Data Source={0};Jet OleDb:DataBase Password='{1}';",
                                ipORfilePath, password);
                        }
                    }
                    break;
                case ClientType.PostgreSQL:
                    {
                        if (port <= 0) port = DBPort.PostgreSQL;
                        strConn = string.Format("Server={0};Port={4};Database={1};Userid={2};Password={3};",
                            ipORfilePath, database, user, password, port);
                    }
                    break;
                case ClientType.Sybase:
                    {
                        if (port <= 0) port = DBPort.Sybase;
                        strConn = string.Format("Data Source={0};Port={4};UID={1};PWD='{2}';database={3};charset=utf8;",
                            ipORfilePath, user, password, database, port);
                    }
                    break;
                case ClientType.DB2:
                    {
                        if (port <= 0) port = DBPort.DB2;
                        strConn = $"Database={database};UserID={user}; Password={password};Server={ipORfilePath}";
                    }
                    break;
            }
            return strConn;
        }

        /// <summary>
        /// 获取连接字符串
        /// </summary>
        /// <param name="type">数据库类型</param>
        /// <param name="ipORfilePath">服务器地址或文件地址</param>
        /// <param name="port">access 2007格式请用7或者2007,03格式请用2003或3,默认为03格式. 其他数据库如果小于等于0，则按默认值：SqlServer:1433  MySql:3306  Oracle:1521  Vertica:5433</param>
        /// <param name="user">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="database">数据库名称</param>
        /// <returns></returns>
        public static string GetConnectionString(string strType, string ipORfilePath, int port, string user, string password, string database)
        {
            ClientType type = GetClientType(strType);
            return GetConnectionString(type, ipORfilePath, port, user, password, database);
        }
        public static DbType GetDataType(System.Type type)
        {
            IDbDataParameter iDbDataParameter = new OleDbParameter() as IDbDataParameter;
            System.ComponentModel.TypeConverter tc = System.ComponentModel.TypeDescriptor.GetConverter(iDbDataParameter.DbType);
            try
            {
                iDbDataParameter.DbType = (DbType)tc.ConvertFrom(type.Name);
            }
            catch (Exception) { }
            return iDbDataParameter.DbType;
        }
        public static ClientType GetClientType(string type)
        {
            if (xClient.DBType.Access.ToLower() == type.ToLower()) return ClientType.Access;
            if (xClient.DBType.MySql.ToLower() == type.ToLower()) return ClientType.MySql;
            if (xClient.DBType.Oracle.ToLower() == type.ToLower()) return ClientType.Oracle;
            if (xClient.DBType.PostgreSQL.ToLower() == type.ToLower()) return ClientType.PostgreSQL;
            if (xClient.DBType.SQLite.ToLower() == type.ToLower()) return ClientType.SQLite;
            if (xClient.DBType.SQLServer.ToLower() == type.ToLower()) return ClientType.SQLServer;
            if (xClient.DBType.Vertica.ToLower() == type.ToLower()) return ClientType.Vertica;
            if (xClient.DBType.Sybase.ToLower() == type.ToLower()) return ClientType.Sybase;
            if (xClient.DBType.DB2.ToLower() == type.ToLower()) return ClientType.DB2;
            return ClientType.Unknow;
        }

        public static int GetDBProt(string type)
        {
            if (xClient.DBType.Access.ToLower() == type.ToLower()) return DBPort.Access;
            if (xClient.DBType.MySql.ToLower() == type.ToLower()) return DBPort.MySql;
            if (xClient.DBType.Oracle.ToLower() == type.ToLower()) return DBPort.Oracle;
            if (xClient.DBType.PostgreSQL.ToLower() == type.ToLower()) return DBPort.PostgreSQL;
            if (xClient.DBType.SQLite.ToLower() == type.ToLower()) return DBPort.SQLite;
            if (xClient.DBType.SQLServer.ToLower() == type.ToLower()) return DBPort.SQLServer;
            if (xClient.DBType.Vertica.ToLower() == type.ToLower()) return DBPort.Vertica;
            if (xClient.DBType.Sybase.ToLower() == type.ToLower()) return DBPort.Sybase;
            if (xClient.DBType.DB2.ToLower() == type.ToLower()) return DBPort.DB2;
            return 0;
        }
    }
}

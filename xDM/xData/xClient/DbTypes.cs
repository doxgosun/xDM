using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xData.xClient
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public enum ClientType
    {
        Unknow = -1,
        Access = 1,
        MySql = 2,
        Oracle = 3,
        SQLite = 4,
        SQLServer = 5,
        Vertica = 6,
        PostgreSQL = 7,
        Sybase = 8,
        DB2 = 9
    }

    /// <summary>
    /// 数据库类型
    /// </summary>
    public static class DBType
    {
        public static readonly string SQLServer = ClientType.SQLServer.ToString();
        public static readonly string MySql = ClientType.MySql.ToString();
        public static readonly string Oracle = ClientType.Oracle.ToString();
        public static readonly string Access = ClientType.Access.ToString();
        public static readonly string SQLite = ClientType.SQLite.ToString();
        public static readonly string Vertica = ClientType.Vertica.ToString();
        public static readonly string PostgreSQL = ClientType.PostgreSQL.ToString();
        public static readonly string Sybase = ClientType.Sybase.ToString();
        public static readonly string DB2 = ClientType.DB2.ToString();
    }
    /// <summary>
    /// 数据库默认端口
    /// </summary>
    public static class DBPort
    {
        public const int Unknow = 0;
        public const int Access = 2003;
        public const int MySql = 3306;
        public const int Oracle = 1521;
        public const int SQLite = 0;
        public const int SQLServer = 1433;
        public const int Vertica = 5433;
        public const int PostgreSQL = 5432;
        public const int Sybase = 5007;
        public const int DB2 = 50000;
    }
}

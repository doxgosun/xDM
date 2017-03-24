/*
 * 1、这里旨在写一个多功能类,只要说明了连接的数据库类型就可以连接,
 *    不用管如何实现.当时DataProvider设计失败了,才定的这个.
 *    
 * 2、废了旧的DataCliet,想起码那些那时候累的...都废了....好了,现在
 *    这个更合理,更稳定.
 */
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;

namespace xDM.xData.xClient
{
    /// <summary>
    /// 这里提供一个Connection，一个Command，一个DataAdapter，并提供其常用的方法。
    /// 如果需要DataReader，请定义一个： DataReader reader = client.Command.ExcuteReader();
    /// 一般的查改增删应该足够。
    /// </summary>
    public class DataClient : System.IDisposable, System.ICloneable
    {
        #region 属性
        private string _dbType { get; set; }
        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DbType { get { return this._dbType; } }
        private IDbCommand _command { get; set; }
        public IDbCommand Command { get { return this._command; } }
        private IDbConnection _connection { get; set; }
        public IDbConnection Connection { get { return this._connection; } }
        private IDbDataAdapter _dataAdapter { get; set; }
        public IDbDataAdapter DataAdapter { get { return this._dataAdapter; } }

        private bool _isTransactionBegining = false;
        public bool IsTransactionBegining { get { return _isTransactionBegining; } }
        public string CommandText
        {
            get { return this.Command.CommandText; }
            set { this.Command.CommandText = value; }
        }
        public int ConnectionTimeout
        {
            get { return this.Command.CommandTimeout; }
            set { this.Command.CommandTimeout = value; }
        }
        public string ConnectionString
        {
            get { return this.Connection.ConnectionString; }
            set { this.Connection.ConnectionString = value; }
        }
        public ConnectionState State
        {
            get
            {
                return this.Connection.State;
            }
        }
        /// <summary>
        /// 如果为Vertica，请先Open()再操作Parameters
        /// </summary>
        public IDataParameterCollection Parameters
        {
            get { return this.Command.Parameters; }
        }
        public object SyncRoot = new object();

        #endregion

        #region 构造函数
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">数据库类型</param>
        public DataClient(ClientType type)
        {
            _Init(type + "");
        }
        public DataClient(string type)
        {
            _Init(type);
        }
        public DataClient(ClientType type, string connectionString)
        {
            _Init(type + "");
            if (connectionString == null) connectionString = "";
            this.ConnectionString = connectionString;
        }
        public DataClient(string type, string connectionString)
        {
            _Init(type);
            if (connectionString == null) connectionString = "";
            this.ConnectionString = connectionString;
        }
        private void _Init(string type)
        {
            this._dbType = type;
            this._connection = Factory.CreateConnection(this.DbType);
            this._command = Factory.CreateCommand(this.DbType);
            this._dataAdapter = Factory.CreateDataAdapter(this.DbType);
            this.Command.Connection = this.Connection;
            this.DataAdapter.SelectCommand = this.Command;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 读取内容到DataTable, 如果连接处于关闭状态,则尝试自动打开执行完毕自动关闭，如果非select，则可能返回null，无法确认是否执行成功，请用ExceuteNonQuery
        /// </summary>
        /// <returns></returns>
        public DataTable ExecuteDataTable()
        {
            return ExecuteDataTable(0);
        }
        public DataTable ExecuteDataTable(int index)
        {
            if (index < 0)
                throw new System.IndexOutOfRangeException("索引不能小于0！");
            var ds = ExecuteDataSet();
            DataTable dt = null;
            if (ds.Tables.Count > index)
                dt = ds.Tables[index];
            else
                throw new System.IndexOutOfRangeException("索引超出返回DataSet中表数量最大值！");
            return dt;
        }
        public DataSet ExecuteDataSet()
        {
            ConnectionState state = this.State;
            if (state != ConnectionState.Open) this.Open();
            DataSet ds = new DataSet();
            if (this._dataAdapter.SelectCommand == null)
            {
                this._dataAdapter.SelectCommand = this._command;
            }
            int count = this.DataAdapter.Fill(ds);
            if (state == ConnectionState.Closed) this.Close();
            return ds;
        }
        /// <summary>
        /// 返回受影响结果count, 如果连接处于关闭状态,则尝试自动打开执行完毕自动关闭
        /// </summary>
        /// <returns></returns>
        public int ExecuteNonQuery()
        {
            ConnectionState state = this.State;
            if (state != ConnectionState.Open) this.Open();
            int count = this.Command.ExecuteNonQuery();
            if (state == ConnectionState.Closed) this.Close();
            return count;
        }
        /// <summary>
        /// 返回唯一结果 Object, 如果连接处于关闭状态,则尝试自动打开执行完毕自动关闭
        /// </summary>
        /// <returns></returns>
        public object ExecuteScalar()
        {
            ConnectionState state = this.State;
            if (state != ConnectionState.Open) this.Open();
            object obj = this.Command.ExecuteScalar();
            if (state == ConnectionState.Closed) this.Close();
            return obj;
        }

        public IDataReader ExecuteReader()
        {
            ConnectionState state = this.State;
            if (state != ConnectionState.Open) this.Open();
            return this.Command.ExecuteReader();
        }

        /// <summary>
        /// 执行查询,将结果导出到文本文件
        /// </summary>
        /// <param name="fileName">要导出文件的完整路径</param>
        /// <param name="fieldTerminator">字段分割符</param>
        /// <param name="enclosed">字段封闭符号,一般为空(null)或双引号</param>
        /// <param name="firstFieldName">是否保存首行为字段名</param>
        /// <returns>导出文件的记录数,不包括首行字段名,返回-1则导出失败</returns>
        public int ExcuteExport(string fileName, string fieldTerminator, char enclosed, bool firstFieldName)
        {
            if (fieldTerminator == null) fieldTerminator = "\t";
            int count = 0;
            if (!Directory.Exists(Path.GetDirectoryName(fileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
            }
            using (DbDataReader reader = this.Command.ExecuteReader() as DbDataReader)
            {
                using (StreamWriter sw = new StreamWriter(fileName, false, new UTF8Encoding(false)))
                {
                    string line = "";
                    int FieldCount = reader.FieldCount;
                    //保存字段名
                    if (firstFieldName)
                    {
                        for (int iCount = 0; iCount < FieldCount; iCount++)
                        {
                            line += string.Format("{0}{1}{0}", enclosed, reader.GetName(iCount)); ;
                            if (iCount != FieldCount - 1)
                            {
                                line += fieldTerminator;
                            }
                        }
                        if (line != "")
                        {
                            sw.WriteLine(line);
                        }
                    }
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            line = "";
                            for (int iCount = 0; iCount < FieldCount; iCount++)
                            {
                                line += string.Format("{0}{1}{0}", enclosed, reader.GetValue(iCount)); ;
                                if (iCount != FieldCount - 1)
                                {
                                    line += fieldTerminator;
                                }
                            }
                            if (line != "")
                            {
                                sw.WriteLine(line);
                                count++;
                            }
                        }
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// 开始事务,等效:this.Command.Transaction = this.Connection.BeginTransaction();
        /// </summary>
        public void BeginTransaction()
        {
            this._isTransactionBegining = true;
            this.Command.Transaction = this.Connection.BeginTransaction();
        }

        /// <summary>
        /// 提交事务,等效:this.Command.Transaction.Commit();
        /// </summary>
        public void Commit()
        {
            this._isTransactionBegining = false;
            this.Command.Transaction.Commit();
        }

        /// <summary>
        /// 从挂起的状态回滚事务
        /// </summary>
        public void Rollback()
        {
            this.Command.Transaction.Rollback();
        }

        /// <summary>
        /// 打开连接,等效:this.Connection.Open()
        /// </summary>
        public void Open()
        {
            if (this.State != ConnectionState.Open)
                this.Connection.Open();
        }
        /// <summary>
        /// 关闭连接,等效:this.Connection.Close()
        /// </summary>
        public void Close()
        {
            if (this.State != ConnectionState.Closed)
                this.Connection.Close();
        }
        public void Dispose()
        {
            try
            {
                if (this.Connection.State == ConnectionState.Open)
                {
                    this.Close();
                }
                this.Command.Dispose();
                this.Connection.Dispose();
                this.DataAdapter.Dispose();
            }
            catch { }
        }
        public object Clone()
        {
            return this.Copy();
        }

        /// <summary>
        /// 复制当前实例，不包括连接状态等状态值
        /// </summary>
        /// <returns></returns>
        public DataClient Copy()
        {
            DataClient client = new DataClient(this.DbType, this.ConnectionString);
            client.CommandText = this.CommandText;
            client.ConnectionTimeout = this.ConnectionTimeout;
            for (int i = 0; i < this.Parameters.Count; i++)
            {
                var p = this.Parameters[i] as IDataParameter;
                client.Parameters.AddWithValue(p.ParameterName,p.Value);
            }
            return client;
        }

        /// <summary>
        /// 通过IP等信息设置连接字符串
        /// </summary>
        /// <param name="type">数据库类型</param>
        /// <param name="ipORfilePath">服务器地址或文件地址</param>
        /// <param name="port">如果小于等于0，则按默认值：SqlServer:1433  MySql:3306  Oracle:1521  Vertica:5433</param>
        /// <param name="user">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="database">数据库名称</param>
        /// <returns></returns>
        public void SetConnectionString(string ipORfilePath, int port, string user, string password, string database)
        {
            this.ConnectionString = xClient.Common.GetConnectionString(this.DbType, ipORfilePath, port, user, password, database);

        }
        #endregion

        #region 静态方法

        #endregion

    }
}

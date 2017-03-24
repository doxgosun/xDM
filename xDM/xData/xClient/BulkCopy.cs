using System;
/*
 * 1、这里旨在写一个多功能类,只要说明了连接的数据库类型就可以连接,
 *    不用管如何实现.当时DataProvider设计失败了,才定的这个.
 *    
 * 2、这里没有继承System.Data.Common名字空间的抽象类,因为觉得如果全
 *    部实现里面的抽象成员会很痛苦,而且不知如何和具体数据库类型转换。
 * 
 * 3、如果以后对C#的领悟有更一步的提高，这里的所有代码可能会费掉，
 *    一如DataProvider.cs.
 */
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Reflection;
using xDM.xData.xClient.Extensions;

namespace xDM.xData.xClient
{
    public class BulkCopy : System.IDisposable
    {

        #region 属性
        public string DbType { get { return this._client.DbType; } }
        private DataClient _client { get; set; }

        public object UserToken { get; set; }
        
        /// <summary>
        /// 每一批次中的行数。在每一批次结束时，将该批次中的行发送到服务器。如果未设置任何值，则为10000。
        /// </summary>
        public int BatchSize
        {
            get { return _batchSize; }
            set
            {
                if (value <= 0) _batchSize = 10000;
                else _batchSize = value;
            }
        }
        private int _batchSize { get; set; } = 10000;
        
        /// <summary>
        /// 超时之前操作完成所允许的秒数。
        /// </summary>
        public int BulkTimeout
        {
            get { return this._client.ConnectionTimeout; }
            set { this._client.ConnectionTimeout = value; }
        }

        /// <summary>
        /// iReader读取的行数
        /// </summary>
        private long _completedCount { get; set; } = 0;

        /// <summary>
        /// 列映射集合。列映射定义数据源中的列和目标表中的列之间的关系。默认情况下为空集合。
        /// </summary>
        public BulkColumnMappingCollection ColumnMappings { get { return this._ColumnMappings; } }
        private BulkColumnMappingCollection _ColumnMappings = new BulkColumnMappingCollection();
        
        /// <summary>
        /// 服务器上目标表的名称。或者如果未提供任何值，则为  null。
        /// </summary>
        public string DestinationTableName { get; set; }

        private int _notifyAfter = -1;
        /// <summary>
        /// 定义在生成通知事件之前要处理的行数。或者如果未设置该属性，则为1。
        /// </summary>
        public int NotifyAfter { get
            {
                if (_notifyAfter <= 0)
                {
                    return 1;
                }
                return _notifyAfter;
            }
            set { _notifyAfter = value; } }
        #endregion

        /// <summary>
        /// 在每次处理完 System.xClient.BulkCopy.NotifyAfter 属性指定的行数时发生。
        /// </summary> 
        public event RowsCopiedEventHandler RowsCopied;


        #region 构造函数
        public BulkCopy(DataClient client)
        {
            _client = client;
        }

        #endregion


        #region 方法
        public void Dispose()
        {
            this._client.Dispose();
            this._ColumnMappings = null;
            this.RowsCopied = null;
        }

        private void _CopyMaps(object oTargetColumnMappings)
        {
            MethodInfo Add = oTargetColumnMappings.GetType().GetMethod("Add", new Type[] { typeof(string), typeof(string) });
            foreach (BulkColumnMapping map in this.ColumnMappings)
            {
                if (map.SourceColumn != null)
                    if (map.DestinationColumn != null)
                        Add.Invoke(oTargetColumnMappings, new object[] { map.SourceColumn, map.DestinationColumn });
                    else
                        Add.Invoke(oTargetColumnMappings, new object[] { map.SourceColumn, map.DestinationOrdinal });
                else if (map.DestinationColumn != null)
                    Add.Invoke(oTargetColumnMappings, new object[] { map.SourceOrdinal, map.DestinationColumn });
                else
                    Add.Invoke(oTargetColumnMappings, new object[] { map.SourceOrdinal, map.DestinationOrdinal });
            }
        }

        /// <summary>
        /// 获取原生BulkCopy
        /// </summary>
        /// <param name="classBulkCopyName"></param>
        /// <param name="classRowsCopiedName"></param>
        /// <param name="oType"></param>
        /// <returns></returns>
        private long _GetBulkCopyAndWriteToServer(string classBulkCopyName,string classRowsCopiedName,object[] args)
        {
            _completedCount = 0;
            var assembly = Factory.GetAssembly(this.DbType);
            var NameSpace = Factory.GetNamespace(this.DbType + "");
            var copyer = assembly.CreateInstance($"{NameSpace}.{classBulkCopyName}", true, BindingFlags.Default, null, new object[] { this._client.ConnectionString }, null, null);
            var oType = copyer.GetType();
            oType.GetProperty("DestinationTableName")?.SetValue(copyer, this.DestinationTableName, null);
            oType.GetProperty("BulkCopyTimeout")?.SetValue(copyer, this.BulkTimeout, null);
            //oType.GetProperty("BatchSize")?.SetValue(copyer, this.BatchSize, null);
            oType.GetProperty("BatchSize")?.SetValue(copyer, 1, null);
            oType.GetProperty("NotifyAfter")?.SetValue(copyer, this.NotifyAfter, null);
            var ColumnMappings = oType.GetProperty("ColumnMappings");
            _CopyMaps(ColumnMappings);

            EventInfo evt = oType.GetEvent(classRowsCopiedName);
            var delType = evt.EventHandlerType;
            var pts = delType.GetDelegateParameterTypes();
            MethodInfo doEventMethod = this.GetType().GetMethod(nameof(bc_RowsCopied), BindingFlags.NonPublic | BindingFlags.Instance);
            MethodInfo concreteDoEventMethod = doEventMethod.MakeGenericMethod(pts[1]);
            Delegate d = Delegate.CreateDelegate(delType, this, concreteDoEventMethod);
            evt.AddEventHandler(copyer,d);

            var wtServer = oType.GetMethod("WriteToServer",DelegateBuilder.GetParamsTypesFromParameters(args));
            wtServer.Invoke(copyer, args);
            return _completedCount;
        }

        //private long GetNpgsqlCopyInAndWriteToServer()
        //{

        //}

        private void _InitSqlBulkCopy(SqlBulkCopy bc)
        {
            bc.DestinationTableName = this.DestinationTableName;
            bc.BulkCopyTimeout = this._client.ConnectionTimeout;
            bc.BatchSize = this.BatchSize;
            bc.NotifyAfter = this.NotifyAfter;
            _CopyMaps(bc.ColumnMappings);
            bc.SqlRowsCopied += bc_SqlRowsCopied;
        }

        void bc_SqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            RowsCopiedEventArgs re = new RowsCopiedEventArgs(e.RowsCopied);
            re.Abort = e.Abort;
            BulkCopy bc = sender as BulkCopy;
            bc.RowsCopied?.BeginInvoke(sender, re, null, null);
        }
        protected void bc_RowsCopied<T>(object sender, T eventArgs) where T : System.EventArgs
        {
            _completedCount++;
            if (_completedCount % NotifyAfter != 0)
                return;
            var RowsCopied = (long)eventArgs.GetType().GetProperty("RowsCopied").GetValue(eventArgs,null);
            RowsCopiedEventArgs re = new RowsCopiedEventArgs(RowsCopied);
            var Abort = (bool)eventArgs.GetType().GetProperty("Abort").GetValue(eventArgs, null);
            re.Abort = Abort;
            BulkCopy bc = sender as BulkCopy;
            bc.RowsCopied?.BeginInvoke(sender, re, null, null);
        }

        private BulkInsert _GetBulkInsert()
        {
            BulkInsert bi = new BulkInsert(this._client);
            bi.BatchSize = BatchSize;
            foreach (BulkColumnMapping map in ColumnMappings)
            {
                bi.ColumnMappings.Add(map);
            }
            bi.DestinationTableName = DestinationTableName;
            bi.NotifyAfter = NotifyAfter;
            bi.RowsCopied += RowsCopied;
            return bi;
        }


        private long WriteToServer(object[] args)
        {
            long count = -1;
            switch (Common.GetClientType(this.DbType))
            {
                case ClientType.SQLServer:
                    //using (SqlBulkCopy bc = new SqlBulkCopy(this._client.ConnectionString))
                    //{
                    //    InitSqlBulkCopy(bc);
                    //    bc.WriteToServer(reader);
                    //}
                    count = _GetBulkCopyAndWriteToServer("SqlBulkCopy", "SqlRowsCopied",args);
                    break;
                case ClientType.DB2:
                    count = _GetBulkCopyAndWriteToServer("DB2BulkCopy", "DB2RowsCopied", args);
                    break;
                case ClientType.Sybase:
                    count = _GetBulkCopyAndWriteToServer("AseBulkCopy", "AscRowsCopied", args);
                    break;
                case ClientType.Oracle:
                    count = _GetBulkCopyAndWriteToServer("OracleBulkCopy", "OracleRowsCopied", args);
                    break;
            }
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="skipError">忽略错误</param>
        public long WriteToServer(IDataReader reader)
        {
            if (this.DestinationTableName + "" == "")
            {
                throw new Exception("目标表为空！");
            }
            long count = -1;
            switch (Common.GetClientType(this.DbType))
            {
                case ClientType.SQLServer:
                case ClientType.DB2:
                case ClientType.Sybase:
                case ClientType.Oracle:
                    count = WriteToServer(new object[] { reader });
                    break;
                default:
                    count = _GetBulkInsert().InsertToServer(reader);
                    break;
            }
            return count;
        }

        public long WriteToServer(DataTable dt)
        {
            if (this.DestinationTableName + "" == "")
            {
                throw new Exception("目标表为空！");
            }
            long count = -1;
            switch (Common.GetClientType(this.DbType))
            {
                case ClientType.SQLServer:
                case ClientType.DB2:
                case ClientType.Sybase:
                case ClientType.Oracle:
                    count = WriteToServer(new object[] { dt });
                    break;
                default:
                    count = _GetBulkInsert().InsertToServer(dt);
                    break;
            }
            return count;
        }

        #endregion
    }

    public class RowsCopiedEventArgs
    {
        /// <summary>
        /// 创建 RowsCopiedEventArgs 对象的新实例。
        /// </summary>
        /// <param name="rowsCopied">指示在当前的批量复制操作过程中复制的行数。</param>
        public RowsCopiedEventArgs(long rowsCopied)
        {
            this._RowsCopied = rowsCopied;
        }
 
        /// <summary>
        /// 获取或设置指示是否应中止批量复制操作的值。如果应中止批量复制操作，则为 true；否则为 false。
        /// </summary>
        public bool Abort { get; set; }

        /// <summary>
        /// 获取一个值，该值返回在当前批量复制操作期间复制的行数。
        /// </summary>
        public long RowsCopied { get { return this._RowsCopied; } }
        private long _RowsCopied { get; set; }
    }

    /// <summary>
    /// 表示处理 System.xClient.BulkCopy 的 System.xClient.BulkCopy.RowsCopied事件的方法。
    /// </summary>
    /// <param name="sender">事件源</param>
    /// <param name="e">包含事件数据的 System.Data.SqlClient.SqlRowsCopiedEventArgs 对象</param>
    public delegate void RowsCopiedEventHandler(object sender, RowsCopiedEventArgs e);

}

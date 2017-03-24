using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace xDM.xData.xClient
{
    class BulkInsert : System.IDisposable
    {
        #region 属性
        public string DbType { get { return this._client.DbType; } }
        private DataClient _client { get; set; }

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
        /// 列映射集合。列映射定义数据源中的列和目标表中的列之间的关系。默认情况下为空集合。
        /// </summary>
        public BulkColumnMappingCollection ColumnMappings { get { return this._columnMappings; } }
        private BulkColumnMappingCollection _columnMappings = new BulkColumnMappingCollection();

        /// <summary>
        /// 服务器上目标表的名称。或者如果未提供任何值，则为  null。
        /// </summary>
        public string DestinationTableName { get; set; }

        private int _notifyAfter = -1;
        /// <summary>
        /// 定义在生成通知事件之前要处理的行数。或者如果未设置该属性，则为1。
        /// </summary>
        public int NotifyAfter
        {
            get
            {
                if (_notifyAfter <= 0)
                {
                    return 1;
                }
                return _notifyAfter;
            }
            set { _notifyAfter = value; }
        }
        #endregion

        /// <summary>
        /// 在每次处理完 System.xClient.BulkCopy.NotifyAfter 属性指定的行数时发生。
        /// </summary> 
        public event RowsCopiedEventHandler RowsCopied;

        /// <summary>
        /// 异步委托
        /// </summary>
        /// <param name="dt"></param>
        private delegate long write(DataTable dt);

        public BulkInsert(DataClient client)
        {
            this._client = client;
        }


        public long InsertToServer(DataTable dt)
        {
            long count = 0;
            //每次插入的条数
            if (BatchSize <= 0)
            {
                BatchSize = 100;
            }
            DataTable dtServer = this._InitTable(dt);

            char paraChar = '@';

            //处理字段名
            string strNames = "", strParaNames = "";
            for (int i = 0; i < dtServer.Columns.Count; i++)
            {
                var colName = dtServer.Columns[i].ColumnName;
                var para = $"{paraChar}{Regex.Replace(colName, "^\"|\"$|\\s", "")}";
                if (i == 0)
                {
                    strNames = colName;
                    strParaNames = para;
                }
                else
                {
                    strNames = $"{strNames},{colName}";
                    strParaNames = $"{strParaNames},{paraChar}{para}";
                }
            }
            string[] paraNames = strParaNames.Split(',');
            if (this._client.State != ConnectionState.Open)
            {
                this._client.Open();
            }

            bool ifTran = true;
            try
            {
                this._client.BeginTransaction();
            }
            catch
            {
                ifTran = false;
            }
            this._client.CommandText = $" INSERT INTO {DestinationTableName} ({strNames}) VALUES ({strParaNames}) ";
            var colCount = dtServer.Columns.Count;
            for (int iRow = 0; iRow < dtServer.Rows.Count; iRow++)
            {
                if (iRow % this.NotifyAfter == 0)
                {
                    this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(iRow), null, null);
                }
                for (int iCol = 0; iCol < colCount; iCol++)
                {
                    object val = dtServer.Rows[iRow][iCol];
                    if (this._client.Parameters.Count < colCount)
                    {
                        this._client.Parameters.AddWithValue(paraNames[iCol], val);
                    }
                    else
                    {
                        var p = this._client.Parameters[iCol] as IDataParameter;
                        p.Value = val;
                    }
                }
                try
                {
                    this._client.ExecuteNonQuery();
                    count++;
                }
                catch { }
                if (iRow % BatchSize == 0)
                {
                    try
                    {
                        if (ifTran && this._client.Command.Transaction != null && this._client.Command.Transaction.Connection != null)
                        {
                            this._client.Commit();
                        }
                    }
                    catch { }
                }

            }

            this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(dt.Rows.Count), null, null);

            try
            {
                if (ifTran && this._client.Command.Transaction != null && this._client.Command.Transaction.Connection != null)
                {
                    this._client.Commit();
                }
                count += BatchSize;
            }
            catch { }
            return count;
        }

        public long InsertToServer(IDataReader reader)
        {
            if (reader == null) throw new Exception();
            //每次插入的条数
            if (BatchSize <= 0)
            {
                BatchSize = 100;
            }
            string[] desNames;
            var desIndexs = this._InitReader(reader, out desNames);
            char paraChar = '@';
            //处理字段名
            string strNames = "", strParaNames = "", strParaNamesWithOutParaChar = "";
            for (int i = 0; i < desIndexs.Length; i++)
            {
                var colName = desNames[i];
                var para = $"p{i}";//$"{Regex.Replace(colName, "^\"|\"$|\\s", "")}";
                if (i == 0)
                {
                    strNames = colName;
                    strParaNames = paraChar + para;
                    strParaNamesWithOutParaChar = para;
                }
                else
                {
                    strNames = $"{strNames},{colName}";
                    strParaNames = $"{strParaNames},{paraChar}{para}";
                    strParaNamesWithOutParaChar = $"{strParaNamesWithOutParaChar},{para}";
                }
            }
            string[] paraNames = strParaNames.Split(',');
            string[] paraNamesWithOutParaChar = strParaNamesWithOutParaChar.Split(',');

            this._client.Open();
            bool ifTran = true;
            try
            {
                this._client.BeginTransaction();
            }
            catch
            {
                ifTran = false;
            }
            long count = 0;
            int intReaded = 0;
            this._client.CommandText = $" INSERT INTO {DestinationTableName} ({strNames}) VALUES ({strParaNames}) ";
            this._client.Parameters.Clear();
            while (reader.Read())
            {
                intReaded++;
                if (this.RowsCopied != null && this.NotifyAfter != 0)
                {
                    if (this.NotifyAfter > 0 && intReaded % this.NotifyAfter == 0)
                    {
                        this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(intReaded), null, null);
                    }
                }
                for (int iCol = 0; iCol < desIndexs.Length; iCol++)
                {
                    object val = reader[desIndexs[iCol]];
                    if (this._client.Parameters.Count < desIndexs.Length)
                    {
                        this._client.Parameters.AddWithValue(paraNames[iCol], val);
                    }
                    else
                    {
                        var p = this._client.Parameters[iCol] as IDataParameter;
                        p.Value = val;
                    }

                }
                try
                {
                    this._client.ExecuteNonQuery();
                    count++;
                }
                catch { }
                if (intReaded % BatchSize == 0)
                {
                    try
                    {
                        if (ifTran && this._client.Command.Transaction != null && this._client.Command.Transaction.Connection != null)
                        {
                            this._client.Commit();
                            this._client.BeginTransaction();
                        }
                    }
                    catch { }
                }
            }
            this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(intReaded), null, null);
            try
            {
                if (ifTran && this._client.Command.Transaction != null && this._client.Command.Transaction.Connection != null)
                {
                    this._client.Commit();
                }
            }
            catch { }
            return count;
        }

        private DataTable _InitTable(DataTable sourceDataTable)
        {
            #region 获取服务器字段名及顺序
            this._client.CommandText = string.Format(" SELECT * FROM {0} WHERE 1=2 ", this.DestinationTableName);
            DataTable dtServer = this._client.ExecuteDataTable();
            if (dtServer == null) return null;
            Dictionary<string, int> sourceNamesIndex = new Dictionary<string, int>();
            Dictionary<int, string> sourceIndexNames = new Dictionary<int, string>();
            for (int i = 0; i < sourceDataTable.Columns.Count; i++)
            {
                sourceNamesIndex.Add(sourceDataTable.Columns[i].ColumnName, i);
                sourceIndexNames.Add(i, sourceDataTable.Columns[i].ColumnName);
            }
            Dictionary<string, int> destinationNamesIndex = new Dictionary<string, int>();
            Dictionary<int, string> destinationIndexNames = new Dictionary<int, string>();
            for (int i = 0; i < dtServer.Columns.Count; i++)
            {
                destinationNamesIndex.Add(dtServer.Columns[i].ColumnName, i);
                destinationIndexNames.Add(i, dtServer.Columns[i].ColumnName);
            }
            #endregion

            #region 处理 Mapping
            if (this.ColumnMappings.Count > 0)
            {
                try
                {
                    dtServer = new DataTable();
                    dtServer = sourceDataTable.Copy();
                    if (dtServer.Columns.IsReadOnly)
                    {
                        throw new Exception("无法对只读DataTable进行操作!");
                    }
                    string strDelColumnName = "sadfsd6f4w6f1sdf9+87wef1sdaf6w4ef35sa4f9dfg56fsdf34gfbuhjksd4f6dt4df86hj4gf65g4sda6gf4as";
                    for (int i = 0; i < dtServer.Columns.Count; i++)
                    {
                        dtServer.Columns[i].ColumnName = strDelColumnName;
                    }
                    foreach (BulkColumnMapping map in this.ColumnMappings)
                    {
                        if (map.SourceColumn != null)
                        {
                            if (map.DestinationColumn != null)
                            {
                                if (sourceNamesIndex.ContainsKey(map.SourceColumn) && destinationNamesIndex.ContainsKey(map.DestinationColumn))
                                {
                                    dtServer.Columns[sourceNamesIndex[map.SourceColumn]].ColumnName = map.DestinationColumn;
                                }
                            }
                            else
                            {
                                if (sourceNamesIndex.ContainsKey(map.SourceColumn) && destinationIndexNames.ContainsKey(map.DestinationOrdinal))
                                {
                                    dtServer.Columns[sourceNamesIndex[map.SourceColumn]].ColumnName = destinationIndexNames[map.DestinationOrdinal];
                                }
                            }
                        }
                        else
                        {
                            if (map.DestinationColumn != null)
                            {
                                if (sourceIndexNames.ContainsKey(map.SourceOrdinal) && destinationNamesIndex.ContainsKey(map.DestinationColumn))
                                {
                                    dtServer.Columns[map.SourceOrdinal].ColumnName = map.DestinationColumn;
                                }
                            }
                            else
                            {
                                if (sourceIndexNames.ContainsKey(map.SourceOrdinal) && destinationIndexNames.ContainsKey(map.DestinationOrdinal))
                                {
                                    dtServer.Columns[map.SourceOrdinal].ColumnName = destinationIndexNames[map.DestinationOrdinal];
                                }
                            }
                        }
                    }
                    dtServer.Columns.Remove(strDelColumnName);
                }
                catch
                {
                    throw new Exception("目标字段或源字段不存在!");
                }
            }
            else
            {
                dtServer = sourceDataTable;
            }
            #endregion

            return dtServer;
        }

        private Dictionary<string, string> _Mpaaing(string[] columns)
        {
            Dictionary<string, string> kv = new Dictionary<string, string>();
            #region 获取服务器目标表字段名及顺序
            this._client.CommandText = string.Format(" SELECT * FROM {0} WHERE 1=2 ", this.DestinationTableName);
            DataTable dtServer = this._client.ExecuteDataTable();
            if (dtServer == null) return null;
            Dictionary<string, int> sourceNamesIndex = new Dictionary<string, int>();
            Dictionary<int, string> sourceIndexNames = new Dictionary<int, string>();
            for (int i = 0; i < columns.Length; i++)
            {
                sourceNamesIndex.Add(columns[i], i);
                sourceIndexNames.Add(i, columns[i]);
            }
            Dictionary<string, int> destinationNamesIndex = new Dictionary<string, int>();
            Dictionary<int, string> destinationIndexNames = new Dictionary<int, string>();
            for (int i = 0; i < dtServer.Columns.Count; i++)
            {
                destinationNamesIndex.Add(dtServer.Columns[i].ColumnName, i);
                destinationIndexNames.Add(i, dtServer.Columns[i].ColumnName);
            }
            #endregion

            #region 处理 Mapping
            if (this.ColumnMappings.Count > 0)
            {
                try
                {
                    foreach (BulkColumnMapping map in this.ColumnMappings)
                    {
                        if (map.SourceColumn != null)
                        {
                            if (map.DestinationColumn != null)
                            {
                                if (sourceNamesIndex.ContainsKey(map.SourceColumn) && destinationNamesIndex.ContainsKey(map.DestinationColumn))
                                {
                                    kv.Add(dtServer.Columns[sourceNamesIndex[map.SourceColumn]].ColumnName, map.DestinationColumn);
                                }
                            }
                            else
                            {
                                if (sourceNamesIndex.ContainsKey(map.SourceColumn) && destinationIndexNames.ContainsKey(map.DestinationOrdinal))
                                {
                                    kv.Add(dtServer.Columns[sourceNamesIndex[map.SourceColumn]].ColumnName, destinationIndexNames[map.DestinationOrdinal]);
                                }
                            }
                        }
                        else
                        {
                            if (map.DestinationColumn != null)
                            {
                                if (sourceIndexNames.ContainsKey(map.SourceOrdinal) && destinationNamesIndex.ContainsKey(map.DestinationColumn))
                                {
                                    kv.Add(dtServer.Columns[map.SourceOrdinal].ColumnName, map.DestinationColumn);
                                }
                            }
                            else
                            {
                                if (sourceIndexNames.ContainsKey(map.SourceOrdinal) && destinationIndexNames.ContainsKey(map.DestinationOrdinal))
                                {
                                    kv.Add(dtServer.Columns[map.SourceOrdinal].ColumnName, destinationIndexNames[map.DestinationOrdinal]);
                                }
                            }
                        }
                    }
                    if (kv.Count == 0)
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            kv.Add(columns[i], columns[i]);
                        }
                    }
                }
                catch
                {
                    throw new Exception("目标字段或源字段不存在!");
                }
            }
            #endregion
            return kv;
        }

        private int[] _InitReader(IDataReader reader, out string[] desNames)
        {
            List<int> list = new List<int>();
            List<string> lNames = new List<string>();
            desNames = null;
            Dictionary<int, int> kv_S_D = new Dictionary<int, int>();
            Dictionary<int, int> kv_D_S = new Dictionary<int, int>();
            #region 获取服务器目标表字段名及顺序
            this._client.CommandText = string.Format(" SELECT * FROM {0} WHERE 1=2 ", this.DestinationTableName);
            DataTable dtServer = this._client.ExecuteDataTable();
            if (dtServer == null) return null;
            Dictionary<string, int> sourceNamesIndex = new Dictionary<string, int>();
            Dictionary<int, string> sourceIndexNames = new Dictionary<int, string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                sourceNamesIndex.Add(reader.GetName(i), i);
                sourceIndexNames.Add(i, reader.GetName(i));
            }
            Dictionary<string, int> destinationNamesIndex = new Dictionary<string, int>();
            Dictionary<int, string> destinationIndexNames = new Dictionary<int, string>();
            bool eq = true; //是否全匹配
            for (int i = 0; i < dtServer.Columns.Count; i++)
            {
                var colName = dtServer.Columns[i].ColumnName;
                destinationNamesIndex.Add(colName, i);
                destinationIndexNames.Add(i, colName);
                if (!sourceNamesIndex.ContainsKey(colName))
                    eq = false;
            }
            #endregion

            #region 处理 Mapping
            if (this.ColumnMappings.Count == 0)
            {
                if (eq)
                    foreach (var name in destinationNamesIndex.Keys)
                        this.ColumnMappings.Add(name, name);
                else
                    for (int i = 0; i < destinationNamesIndex.Keys.Count; i++)
                        this.ColumnMappings.Add(i, i);
            }
            if (this.ColumnMappings.Count > 0)
            {
                try
                {
                    foreach (BulkColumnMapping map in this.ColumnMappings)
                    {
                        if (map.SourceColumn != null)
                        {
                            if (map.DestinationColumn != null)
                            {
                                if (sourceNamesIndex.ContainsKey(map.SourceColumn) && destinationNamesIndex.ContainsKey(map.DestinationColumn))
                                {
                                    kv_S_D.Add(sourceNamesIndex[map.SourceColumn], destinationNamesIndex[map.DestinationColumn]);
                                    kv_D_S.Add(destinationNamesIndex[map.DestinationColumn], sourceNamesIndex[map.SourceColumn]);
                                }
                            }
                            else
                            {
                                if (sourceNamesIndex.ContainsKey(map.SourceColumn) && destinationIndexNames.ContainsKey(map.DestinationOrdinal))
                                {
                                    kv_S_D.Add(sourceNamesIndex[map.SourceColumn], map.DestinationOrdinal);
                                    kv_D_S.Add(map.DestinationOrdinal, sourceNamesIndex[map.SourceColumn]);
                                }
                            }
                        }
                        else
                        {
                            if (map.DestinationColumn != null)
                            {
                                if (sourceIndexNames.ContainsKey(map.SourceOrdinal) && destinationNamesIndex.ContainsKey(map.DestinationColumn))
                                {
                                    kv_S_D.Add(map.SourceOrdinal, destinationNamesIndex[map.DestinationColumn]);
                                    kv_D_S.Add(destinationNamesIndex[map.DestinationColumn], map.SourceOrdinal);
                                }
                            }
                            else
                            {
                                if (sourceIndexNames.ContainsKey(map.SourceOrdinal) && destinationIndexNames.ContainsKey(map.DestinationOrdinal))
                                {
                                    kv_S_D.Add(map.SourceOrdinal, map.DestinationOrdinal);
                                    kv_D_S.Add(map.DestinationOrdinal, map.SourceOrdinal);
                                }
                            }
                        }
                    }
                    for (int i = 0; i < dtServer.Columns.Count; i++)
                    {
                        var colName = dtServer.Columns[i].ColumnName;
                        lNames.Add(colName);
                        if (kv_D_S.ContainsKey(i))
                        {
                            list.Add(kv_D_S[i]);
                            continue;
                        }
                        if (sourceNamesIndex.ContainsKey(colName))
                        {
                            list.Add(sourceNamesIndex[colName]);
                            continue;
                        }
                    }
                    //if (list.Count != dtServer.Columns.Count)
                    //{
                    //    throw new Exception("目标字段和源字段没有一一对应！");
                    //}
                }
                catch
                {
                    throw new Exception("目标字段或源字段不存在!");
                }
            }
            #endregion
            desNames = lNames.ToArray();
            return list.ToArray();
        }

        private long _OracleBulkInsert(DataTable dt, int BatchSize)
        {
            long count = -1;
            //每次插入的条数
            if (BatchSize <= 0)
            {
                BatchSize = 100;
            }
            DataTable dtServer = this._InitTable(dt);
            //处理字段名
            string open = "", close = "";
            string strNames = "", strParameterNames = "";
            for (int i = 0; i < dtServer.Columns.Count; i++)
            {
                strNames += open + dtServer.Columns[i].ColumnName + close + ",";
                strParameterNames += Regex.Replace(dtServer.Columns[i].ColumnName.Replace(" ", ""), "^\"|\"$", "") + ",";
            }
            string[] parameterNames = strParameterNames.Split(',');

            this._client.Open();
            bool ifTran = true;
            try
            {
                this._client.BeginTransaction();
            }
            catch
            {
                ifTran = false;
            }
            int cnt = 0;
            string strValue = "";
            string strParaName = "";
            for (int i = 0; i < dtServer.Rows.Count; i++)
            {

                if (i % this.NotifyAfter == 0)
                {
                    this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(i), null, null);
                }
                for (int j = 0; j < dtServer.Columns.Count; j++)
                {
                    if (j == 0)
                    {
                        if (cnt == 0)
                        {
                            strValue = " SELECT ";
                        }
                        else
                        {
                            strValue += " UNION SELECT ";
                        }
                    }
                    strParaName = string.Format(":\"{0}a_b{1}\"", parameterNames[j], cnt);
                    this._client.Parameters.AddWithValue(strParaName, dtServer.Rows[i][j]);
                    if (j == dtServer.Columns.Count - 1)
                    {
                        strValue += strParaName;
                        strValue += " FROM DUAL ";
                    }
                    else
                    {
                        strValue += strParaName + ",";
                    }
                }
                if (++cnt >= BatchSize)
                {
                    cnt = 0;
                    try
                    {
                        this._client.CommandText = string.Format(" INSERT INTO {0} ({1}) {2} ", this.DestinationTableName, strNames.Substring(0, strNames.Length - 1), strValue);
                        count += this._client.ExecuteNonQuery();
                    }
                    catch { }
                    finally
                    {
                        this._client.Parameters.Clear();
                    }
                }
                if (ifTran && (i + 1) % this.BatchSize == 0 && this._client.Command.Transaction != null)
                {
                    this._client.Commit();
                }
            }

            if (ifTran && this._client.Command.Transaction != null)
            {
                try
                {
                    this._client.Commit();
                }
                catch { }
            }
            this._client.Dispose();
            return count;
        }
        //private long OracleBulkInsert(IDataReader reader, int BatchSize)
        //{
        //    long count = -1;
        //    if (reader == null) return count;
        //    //每次插入的条数
        //    if (BatchSize <= 0)
        //    {
        //        BatchSize = 100;
        //    }
        //    Dictionary<string, string> kvSD = this.initReader(reader);
        //    string[] D = new string[kvSD.Count];
        //    string[] S = new string[kvSD.Count];
        //    kvSD.Values.CopyTo(D, 0);
        //    kvSD.Keys.CopyTo(S, 0);
        //    //处理字段名
        //    string open = "", close = "";
        //    string strNames = "", strParameterNames = "";
        //    for (int i = 0; i < reader.FieldCount; i++)
        //    {
        //        strNames += open + reader.GetName(i) + close + ",";
        //        strParameterNames += Regex.Replace(reader.GetName(i).Replace(" ", ""), "^\"|\"$", "") + ",";
        //    }
        //    string[] parameterNames = strParameterNames.Split(',');

        //    this._client.Open();
        //    bool ifTran = true;
        //    try
        //    {
        //        this._client.BeginTransaction();
        //    }
        //    catch
        //    {
        //        ifTran = false;
        //    }
        //    int cnt = 0;
        //    string strValue = "";
        //    string strParaName = "";
        //    int intReaded = 0;
        //    count = 0;
        //    while (reader.Read())
        //    {
        //        if (this.RowsCopied != null && this.NotifyAfter != 0)
        //        {
        //            if (intReaded % this.NotifyAfter == 0)
        //            {
        //                this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(intReaded), null, null);
        //            }
        //        }
        //        for (int j = 0; j < D.Length; j++)
        //        {
        //            if (j == 0)
        //            {
        //                if (cnt == 0)
        //                {
        //                    strValue = " SELECT ";
        //                }
        //                else
        //                {
        //                    strValue += " UNION SELECT ";
        //                }
        //            }
        //            strParaName = string.Format(":\"{0}a_b{1}\"", parameterNames[j], cnt);
        //            int index = reader.GetOrdinal(S[j]);
        //            this._client.Parameters.AddWithValue(strParaName, reader.GetValue(index));
        //            if (j == D.Length - 1)
        //            {
        //                strValue += strParaName;
        //                strValue += " FROM DUAL ";
        //            }
        //            else
        //            {
        //                strValue += strParaName + ",";
        //            }
        //        }

        //        try
        //        {
        //            if (++cnt >= BatchSize)
        //            {
        //                cnt = 0;
        //                this._client.CommandText = string.Format(" INSERT INTO {0} ({1}) {2} ", this.DestinationTableName, strNames.Substring(0, strNames.Length - 1), strValue);
        //                count += this._client.ExecuteNonQuery();
        //                this._client.Parameters.Clear();
        //            }
        //        }
        //        catch { }
        //        if (ifTran && (intReaded + 1) % BatchSize == 0 && this._client.Command.Transaction != null)
        //        {
        //            this._client.Commit();
        //        }

        //    }


        //    if (ifTran && this._client.Command.Transaction != null)
        //    {
        //        try
        //        {
        //            this._client.Commit();
        //        }
        //        catch { }
        //    }
        //    this._client.Dispose();
        //    return count;
        //}
        /// <summary>
        /// Access/Sqlite请用BatchSize = 1,不支持oracle
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="BatchSize"></param>
        private long _Bulk_Insert(DataTable dt, int BatchSize)
        {
            long count = 0;
            //每次插入的条数
            if (BatchSize <= 0)
            {
                BatchSize = 100;
            }
            DataTable dtServer = this._InitTable(dt);

            //处理字段名
            string open = "", close = "";
            string strNames = "", strParameterNames = "";
            for (int i = 0; i < dtServer.Columns.Count; i++)
            {
                strNames += open + dtServer.Columns[i].ColumnName + close + ",";
                strParameterNames += Regex.Replace(dtServer.Columns[i].ColumnName.Replace(" ", ""), "^\"|\"$", "") + ",";
            }
            string[] parameterNames = strParameterNames.Split(',');
            if (this._client.State != ConnectionState.Open)
            {
                this._client.Open();
            }

            bool ifTran = true;
            try
            {
                this._client.BeginTransaction();
            }
            catch
            {
                ifTran = false;
            }
            string strValue = "";
            string strParaName = "";
            int cnt = 0;
            for (int i = 0; i < dtServer.Rows.Count; i++)
            {
                if (i % this.NotifyAfter == 0)
                {
                    this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(i), null, null);
                }
                for (int j = 0; j < dtServer.Columns.Count; j++)
                {
                    if (j == 0)
                    {
                        if (cnt == 0)
                        {
                            strValue = "(";
                        }
                        else
                        {
                            strValue += ",(";
                        }
                    }
                    strParaName = string.Format("@{0}a_b{1}", parameterNames[j], cnt);
                    object val = dtServer.Rows[i][j];
                    this._client.Parameters.AddWithValue(strParaName, dtServer.Rows[i][j]);
                    if (j == dtServer.Columns.Count - 1)
                    {
                        strValue += strParaName;
                        strValue += ")";
                    }
                    else
                    {
                        strValue += strParaName + ",";
                    }
                }
                if (++cnt >= BatchSize)
                {
                    cnt = 0;
                    try
                    {
                        this._client.CommandText = string.Format(" INSERT INTO {0} ({1}) VALUES {2} ", this.DestinationTableName, strNames.Substring(0, strNames.Length - 1), strValue);
                        this._client.ExecuteNonQuery();
                        count += BatchSize;
                    }
                    catch { }
                    finally
                    {
                        this._client.Parameters.Clear();
                    }
                }
                if (ifTran && (i + 1) % this.BatchSize == 0 && this._client.Command.Transaction != null && this._client.Command.Transaction.Connection != null)
                {
                    this._client.Commit();
                }
            }

            this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(dt.Rows.Count), null, null);

            if (ifTran && this._client.Command.Transaction != null && this._client.Command.Transaction.Connection != null)
            {
                this._client.Commit();
            }
            this._client.Dispose();
            return count;
        }

        /// <summary>
        /// 参数非DT插入 不支持Oracle
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="BatchSize"></param>
        //private void BulkInsert_2(IDataReader reader, int BatchSize)
        //{
        //    if (reader == null) return;
        //    //每次插入的条数
        //    if (BatchSize <= 0)
        //    {
        //        BatchSize = 100;
        //    }
        //    Dictionary<string, string> kvSD = this.initReader(reader);
        //    string[] D = new string[kvSD.Count];
        //    string[] S = new string[kvSD.Count];
        //    kvSD.Values.CopyTo(D, 0);
        //    kvSD.Keys.CopyTo(S, 0);

        //    //处理字段名
        //    string open = "", close = "";
        //    string strNames = "", strParameterNames = "";
        //    for (int i = 0; i < D.Length; i++)
        //    {
        //        strNames += open + D[i] + close + ",";
        //        strParameterNames += Regex.Replace(D[i].Replace(" ", ""), "^\"|\"$", "") + ",";
        //    }
        //    string[] parameterNames = strParameterNames.Split(',');

        //    this._client.Open();
        //    bool ifTran = true;
        //    try
        //    {
        //        this._client.BeginTransaction();
        //    }
        //    catch
        //    {
        //        ifTran = false;
        //    }
        //    int count = 0;
        //    string strValue = "";
        //    string strParaName = "";
        //    int intReaded = 0;
        //    while (reader.Read())
        //    {
        //        intReaded++;
        //        if (this.RowsCopied != null && this.NotifyAfter != 0)
        //        {
        //            if (this.NotifyAfter > 0 && intReaded % this.NotifyAfter == 0)
        //            {
        //                this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(intReaded), null, null);
        //            }
        //        }
        //        for (int j = 0; j < D.Length; j++)
        //        {
        //            if (j == 0)
        //            {
        //                if (count == 0)
        //                {
        //                    strValue = "(";
        //                }
        //                else
        //                {
        //                    strValue += ",(";
        //                }
        //            }
        //            strParaName = string.Format("\"@{0}a_b{1}\"", parameterNames[j], count);
        //            int index = reader.GetOrdinal(S[j]);
        //            this._client.Parameters.AddWithValue(strParaName, reader.GetValue(index));
        //            if (j == D.Length - 1)
        //            {
        //                strValue += strParaName;
        //                strValue += ")";
        //            }
        //            else
        //            {
        //                strValue += strParaName + ",";
        //            }
        //        }
        //        try
        //        {
        //            if (++count >= BatchSize)
        //            {
        //                count = 0;
        //                this._client.CommandText = string.Format(" INSERT INTO {0} ({1}) VALUES {2} ", this.DestinationTableName, strNames.Substring(0, strNames.Length - 1), strValue);
        //                this._client.ExecuteNonQuery();
        //                this._client.Parameters.Clear();
        //            }
        //        }
        //        catch { }
        //        if (ifTran && (intReaded + 1) % BatchSize == 0 && this._client.Command.Transaction != null)
        //        {
        //            this._client.Commit();
        //        }
        //    }

        //    if (ifTran && this._client.Command.Transaction != null)
        //    {
        //        this._client.Commit();
        //    }
        //    this._client.Dispose();

        //}



        /// <summary>
        /// 使用DataTable批量插入
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="BatchSize"></param>
        private void _Bulk_Insert(IDataReader reader, int BatchSize)
        {
            if (reader == null) return;
            if (BatchSize <= 0) BatchSize = 2000;
            int intReaded = 0;
            DataTable dt0 = new DataTable();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dt0.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }
            DataTable dt = dt0.Copy();
            write _writeToServer = new write(this.InsertToServer);
            List<IAsyncResult> listIR = new List<IAsyncResult>();
            IAsyncResult ar = null;
            DateTime time = DateTime.Now;
            while (reader.Read())
            {
                intReaded++;
                DataRow dr = dt.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    dr[i] = reader.GetValue(i);
                }
                dt.Rows.Add(dr);
                if (intReaded % BatchSize == 0)
                {
                    DateTime time2 = DateTime.Now;
                    TimeSpan ts = time2 - time;
                    if (ar != null && !ar.IsCompleted)
                        //ar.AsyncWaitHandle.WaitOne();
                        _writeToServer.EndInvoke(ar);
                    DateTime time3 = DateTime.Now;
                    TimeSpan ts2 = time2 - time3;
                    ar = _writeToServer.BeginInvoke(dt, null, null);
                    dt = dt0.Copy();
                    time = DateTime.Now;
                }

                if (intReaded % this.NotifyAfter == 0)
                {
                    this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(intReaded), null, null);
                }
            }
            if (dt.Rows.Count > 0)
            {
                if (ar != null && !ar.IsCompleted)
                    //ar.AsyncWaitHandle.WaitOne();
                    _writeToServer.EndInvoke(ar);
                ar = _writeToServer.BeginInvoke(dt, null, null);
            }
            if (ar != null && !ar.IsCompleted)
                //ar.AsyncWaitHandle.WaitOne();
                _writeToServer.EndInvoke(ar);

            this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(intReaded), null, null);
            dt = null;
        }




        /// <summary>
        /// 非参数插入数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="BatchSize"></param>
        private void _VerticaBulkInsertWithOutParament(DataTable dt, int BatchSize)
        {
            //每次插入的条数
            if (BatchSize <= 0)
            {
                BatchSize = 100;
            }
            DataTable dtServer = this._InitTable(dt);
            //处理字段名
            string open = "", close = "";
            string strNames = "", strParameterNames = "";
            for (int i = 0; i < dtServer.Columns.Count; i++)
            {
                strNames += open + dtServer.Columns[i].ColumnName + close + ",";
                strParameterNames += Regex.Replace(dtServer.Columns[i].ColumnName.Replace(" ", ""), "^\"|\"$", "") + ",";
            }
            string[] parameterNames = strParameterNames.Split(',');

            this._client.Open();
            bool ifTran = true;
            try
            {
                this._client.BeginTransaction();
            }
            catch
            {
                ifTran = false;
            }
            int count = 0;
            string strValue = "";
            string strParaName = "";
            for (int i = 0; i < dtServer.Rows.Count; i++)
            {
                if (this.RowsCopied != null)
                {
                    if (i % this.NotifyAfter == 0)
                    {
                        this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(i), null, null);
                    }
                }
                for (int j = 0; j < dtServer.Columns.Count; j++)
                {
                    if (j == 0)
                    {
                        if (count == 0)
                        {
                            strValue = " SELECT ";
                        }
                        else
                        {
                            strValue += " UNION SELECT ";
                        }
                    }
                    //这里不使用参数,数量>1会报错
                    //strParaName = string.Format("@{0}a_b{1}", parameterNames[j], count);
                    //this._client.Parameters.AddWithValue(strParaName, dtServer.Rows[i][j]);
                    strParaName = string.Format("{0}", dtServer.Rows[i][j]);
                    strParaName = string.Format("'{0}'", strParaName.Replace("'", "''"));
                    if (j == dtServer.Columns.Count - 1)
                    {
                        strValue += strParaName;
                        strValue += " FROM DUAL ";
                    }
                    else
                    {
                        strValue += strParaName + ",";
                    }
                }
                if (++count >= BatchSize)
                {
                    count = 0;
                    try
                    {
                        this._client.CommandText = string.Format(" INSERT INTO {0} ({1}) {2} ;", this.DestinationTableName, strNames.Substring(0, strNames.Length - 1), strValue);
                        this._client.ExecuteNonQuery();
                    }
                    catch { }
                    finally
                    {
                        this._client.Parameters.Clear();
                    }
                }
                if (ifTran && (i + 1) % this.BatchSize == 0 && this._client.Command.Transaction != null)
                {
                    this._client.Commit();
                }
            }

            if (ifTran && this._client.Command.Transaction != null)
            {
                try
                {
                    this._client.Commit();
                }
                catch { }
            }
            this._client.Dispose();

        }

        private long VerticaBulkInsert(DataTable dt)
        {
            long count = -1;
            if (dt == null || dt.Rows.Count == 0) return count;
            const char ColSplit = '\t';
            const char RowSplit = '\n';
            const char EnClosed = '\"';
            List<string> lstFields = new List<string>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                lstFields.Add(dt.Columns[i].ColumnName);
            }
            string strFiledList = string.Join(",", lstFields.ToArray());
            string strCopyStatement =
                $"COPY {this.DestinationTableName}({strFiledList}) FROM STDIN RECORD TERMINATOR E'{RowSplit}' DELIMITER E'{ColSplit}' ENCLOSED E'{EnClosed}' ENFORCELENGTH NO COMMIT";
            DataTable dtServer = this._InitTable(dt);
            StringBuilder sbText = new StringBuilder();
            using (var conn = this._client.Connection)// as VerticaConnection)
            {
                ConnectionState state = this._client.State;
                this._client.Open();
                using (var txn = conn.BeginTransaction())
                {
                    count = 0;
                    for (int i = 0; i < dtServer.Rows.Count; i++)
                    {
                        for (int j = 0; j < dtServer.Columns.Count; j++)
                        {
                            if (j == 0)
                            {
                                sbText.AppendFormat("{1}{0}{1}", dtServer.Rows[i][j], EnClosed);
                            }
                            else
                            {
                                sbText.AppendFormat("{0}{2}{1}{2}", ColSplit, dtServer.Rows[i][j], EnClosed);
                            }
                        }
                        sbText.Append(RowSplit);

                        if (this.NotifyAfter > 0 && i % this.NotifyAfter == 0)
                        {
                            this.RowsCopied?.BeginInvoke(this, new RowsCopiedEventArgs(i), null, null);
                        }
                        if (this.BatchSize > 0 && i % this.BatchSize == 0)
                        {
                            count += this._VerticaCopy(sbText.ToString(), strCopyStatement);
                            sbText = new StringBuilder();
                        }
                    }
                    if (sbText.Length > 0)
                    {
                        count += this._VerticaCopy(sbText.ToString(), strCopyStatement);
                        sbText = new StringBuilder();
                    }

                    if (count != dtServer.Rows.Count)
                    {
                        try
                        {
                            if (txn != null)
                                txn.Rollback();
                            count = this._Bulk_Insert(dtServer, 1);
                        }
                        catch
                        {
                            txn.Rollback();
                            conn.Close();
                            string msg = string.Format("有{0}条数据导入失败,程序回滚!", dtServer.Rows.Count - count);
                            throw new Exception(msg);
                        }
                    }
                    else
                    {
                        if (txn != null)
                            txn.Commit();
                        if (state == ConnectionState.Closed)
                            conn.Close();
                    }
                }
            }
            return count;
        }

        private long _VerticaCopy(string Text, string strCopyStatement)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buff = Encoding.UTF8.GetBytes(Text);
                ms.Write(buff, 0, buff.Length);
                ms.Flush();
                ms.Position = 0;

                var assembly = Factory.GetAssembly(this.DbType);
                var NameSpace = Factory.GetNamespace(this.DbType + "");
                var vconnType = assembly.GetType("VerticaConnection");
                var conn = Convert.ChangeType(this._client.Connection, vconnType);
                var copyer = assembly.CreateInstance($"{NameSpace}.VerticaCopyStream", true, BindingFlags.Default, null, new object[] { this._client.Connection, strCopyStatement }, null, null);
                var oType = copyer.GetType();
                oType.GetMethod("Start").Invoke(copyer, null);
                oType.GetMethod("AddStream").Invoke(copyer, new object[] { ms, false });
                oType.GetMethod("Execute").Invoke(copyer, null);
                long count = (long)oType.GetMethod("Finish").Invoke(copyer, null);
                return count;
            }
        }


        public void Dispose()
        {
            this._client.Dispose();
            this._columnMappings = null;
            this.RowsCopied = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xDM.xData.xClient.Extensions;

namespace xDM.xData.xClient
{
    public class BulkLoader : System.IDisposable
    {
        #region 属性
        public string DbType { get { return this._client.DbType; } }
        private DataClient _client { get; set; }
        public string FileName { get; set; }
        private List<string> _columns = new List<string>();
        /// <summary>
        /// 字段列表
        /// </summary>
        public List<string> Columns { get { return this._columns; } }
        public IDbConnection Connection { get { return this._client.Connection; } }
        public string CharacterSet { get; set; } = "";
        /// <summary>
        /// 转义字符
        /// </summary>
        public string EscapeCharacter { get; set; }
        /// <summary>
        /// EnclosedCharacter 字段封闭标识字符，其为 char
        /// </summary>
        public string FieldQuotationCharacter { get; set; }
        /// <summary>
        /// 要跳过的行前缀
        /// </summary>
        public string LinePrefix { get; set; }
        /// <summary>
        /// 行分割符号
        /// </summary>
        public string LineTerminator { get; set; }
        /// <summary>
        /// 字段间分割符号
        /// </summary>
        public string FieldTerminator { get; set; }
        /// <summary>
        /// 要跳过的行数
        /// </summary>
        public int NumberOfLinesToSkip { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }
        public int Timeout
        {
            get { return this._client.ConnectionTimeout; }
            set { this._client.ConnectionTimeout = value; }
        }
        public string ConnectionString
        {
            get { return this._client.ConnectionString; }
            set { this._client.ConnectionString = value; }
        }
        #endregion

        #region 构造函数
        public BulkLoader(DataClient client)
        {
            this._client = client;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 目前仅支持mysql，vertica
        /// </summary>
        /// <returns></returns>
        public long Load()
        {
            switch (Common.GetClientType(this.DbType))
            {
                case ClientType.MySql:
                    return _MysqlLoad();
                case ClientType.Oracle:
                    return _OracleLoad();
                case ClientType.Vertica:
                    return _VerticaLoad();
                case ClientType.SQLServer:
                    return _SqlServerLoad();
                case ClientType.PostgreSQL:
                    return _PostgreSQLLoad();
                default:
                    {
                        return GetBulkCopy().WriteToServer(_GetDataReader());
                    }
            }
        }
        private BulkCopy GetBulkCopy()
        {
            BulkCopy bc = new BulkCopy(this._client);
            bc.DestinationTableName = TableName;
            return bc;
        }
        private TextDataReader _GetDataReader()
        {
            DataTable dt = new DataTable();
            foreach (var col in Columns)
            {
                dt.Columns.Add(col);
            }
            return new TextDataReader(FileName, null, FieldTerminator.GetChar(), FieldQuotationCharacter.GetChar(), dt, null, TextDataReader.TrimMode.Both);
        }
        private int _SqlServerLoad()
        {
            this._client.CommandText = _BuildSqlServerCommand();
            return this._client.ExecuteNonQuery();
        }
        private int _MysqlLoad()
        {
            this._client.CommandText = _BuildMySqlCommand();
            return this._client.ExecuteNonQuery();
        }

        private int _OracleLoad()
        {
            string columns = this.Columns.ToArray().ToStr(',');
            this._client.CommandText = $@"LOAD DATA INFILE '{FileName}' 
                        APPEND INTO TABLE {TableName} Fields terminated by '{FieldTerminator}'({columns})";
            return this._client.ExecuteNonQuery();
        }

        private int _VerticaLoad()
        {
            string columns = this.Columns.ToArray().ToStr(',');
            //如果文件是带bom，转换成无bom，用\n填充
            bool isBom = false;
            try
            {
                using (FileStream fs = new FileStream(this.FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    byte[] bom = new byte[3];
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.Read(bom, 0, bom.Length);
                    if (bom[0] == 239 && bom[1] == 187 && bom[2] == 191)
                    {
                        isBom = true;
                        bom = new byte[] { (byte)'\n', (byte)'\n', (byte)'\n' };
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.Write(bom, 0, bom.Length);
                    }
                }
            }
            catch { }
            this._client.CommandText = _BuildVerticaCommand();
            var count = this._client.ExecuteNonQuery();
            //如果原文件是带bom，转换回去
            if (isBom)
            {
                try
                {
                    using (FileStream fs = new FileStream(this.FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        byte[] bom = new byte[] { 239, 187, 191 };
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.Write(bom, 0, bom.Length);
                    }
                }
                catch { }
            }
            return count;
        }
        private int _PostgreSQLLoad()
        {
            this._client.CommandText = _BuildPostgreSQLCommand();
            Encoding encoding = Encoding.UTF8;
            try { encoding = Encoding.GetEncoding(CharacterSet); } catch { }
            using (var sr = new StreamReader(FileName, encoding))
            {
                //NpgsqlConnection conn = new NpgsqlConnection(_client.Connection.ConnectionString);
                //conn.Open();
                //NpgsqlCommand cmd = new NpgsqlCommand(BuildPostgreSQLCommand(), conn);
                var conn = Factory.CreateOther<IDbConnection>(DbType, "NpgsqlConnection", new object[] { _client.Connection.ConnectionString });
                conn.Open();
                var cmd = Factory.CreateOther<IDbCommand>(DbType, "NpgsqlCommand", new object[] { _BuildPostgreSQLCommand(), _client.Connection });
                //NpgsqlCopyIn copyIn = new NpgsqlCopyIn(cmd, conn, sr.BaseStream);
                //copyIn.Start();
                var copyIn = Factory.CreateOther<object>(this.DbType, "NpgsqlCopyIn", new object[] { cmd, conn, sr.BaseStream }
                    , new Type[] { cmd.GetType(), conn.GetType(), typeof(System.IO.Stream) });

                copyIn.GetType().GetMethod("Start").Invoke(copyIn, null);
                //_client.Close();
            }
            return this._client.ExecuteNonQuery();
        }

        private string _BuildVerticaCommand()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"COPY {TableName} ");
            if (Columns.Count > 0)
            {
                builder.Append($"({Columns.ToArray().ToStr(',')}) ");
            }
            builder.Append($"FROM LOCAL ");
            if (Path.DirectorySeparatorChar == '\\')
            {
                builder.Append($"'{FileName.Replace(@"\", @"\\")}' ");
            }
            else
            {
                builder.Append($"'{FileName}' ");
            }
            if (FieldTerminator + "" != "" && FieldTerminator != "|")
            {
                if (FieldTerminator.Length == 2 && FieldTerminator[0] == '\\')
                {
                    builder.Append($"DELIMITER E'{FieldTerminator}' ");
                }
                else
                {
                    builder.Append($"DELIMITER '{FieldTerminator}' ");
                }
            }
            if (FieldQuotationCharacter + "" != "")
            {
                if (FieldQuotationCharacter.Length == 2 && FieldQuotationCharacter[0] == '\\')
                {
                    builder.Append($"ENCLOSED BY E'{FieldQuotationCharacter}' ");
                }
                else
                {
                    builder.Append($"ENCLOSED BY '{FieldQuotationCharacter}' ");
                }
            }
            builder.Append($"NULL 'null' ");
            if (EscapeCharacter + "" != "" && EscapeCharacter != "\\")
            {
                if (EscapeCharacter.Length == 2 && EscapeCharacter[0] == '\\')
                {
                    builder.Append($"ESCAPE AS E'{EscapeCharacter}' ");
                }
                else
                {
                    builder.Append($"ESCAPE AS '{EscapeCharacter}' ");
                }
            }
            if (LineTerminator != "\n" && LineTerminator != "\r\n" && LineTerminator + "" != "")
            {
                if (LineTerminator.Length == 2 && LineTerminator[0] == '\\')
                {
                    builder.Append($"RECORD TERMINATOR E'{LineTerminator}' ");
                }
                else
                {
                    builder.Append($"RECORD TERMINATOR '{LineTerminator}' ");
                }
            }
            if (NumberOfLinesToSkip > 0)
            {
                builder.Append($"SKIP {NumberOfLinesToSkip} ");
            }
            builder.Append("DIRECT ");
            return builder.ToString();
        }

        private string _BuildSqlServerCommand()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"BULK INSERT {TableName} ");
            builder.Append($"FROM ");
            if (Path.DirectorySeparatorChar == '\\')
            {
                builder.Append($"'{this.FileName.Replace(@"\", @"\\")}' ");
            }
            else
            {
                builder.Append($"'{this.FileName}' ");
            }
            builder.Append("WITH ");
            StringBuilder builder2 = new StringBuilder();
            if (this.FieldTerminator != "\t")
            {
                builder2.Append($"FIELDTERMINATOR = '{this.FieldTerminator}' ");
            }
            if (this.NumberOfLinesToSkip >= 0)
            {
                builder.Append($",FIRSTROW = {this.NumberOfLinesToSkip + 1} ");
            }
            if (this.LineTerminator != "\r\n")
            {
                builder2.Append($",ROWTERMINATOR = '{this.LineTerminator}' ");
            }

            builder.Append($"({builder2}) ");

            return builder.ToString();
        }

        private string _BuildMySqlCommand()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("LOAD DATA LOCAL INFILE ");
            if (Path.DirectorySeparatorChar == '\\')
            {
                builder.Append($"'{this.FileName.Replace(@"\", @"\\")}' ");
            }
            else
            {
                builder.Append($"'{this.FileName}' ");
            }
            builder.Append($"INTO TABLE {this.TableName} ");
            if (this.CharacterSet + "" != "")
            {
                builder.Append($"CHARACTER SET {this.CharacterSet} ");
            }
            StringBuilder builder2 = new StringBuilder(string.Empty);
            if (this.FieldTerminator != "\t")
            {
                builder2.Append($"TERMINATED BY '{this.FieldTerminator}' ");
            }
            if (EscapeCharacter != "\\" && EscapeCharacter + "" != "")
            {
                builder2.Append($"ESCAPED BY '{this.EscapeCharacter}' ");
            }
            if (builder2.Length > 0)
            {
                builder.Append($"FIELDS {builder2}");
            }
            builder2 = new StringBuilder(string.Empty);
            if ((this.LinePrefix != null) && (this.LinePrefix.Length > 0))
            {
                builder2.Append($"STARTING BY '{this.LinePrefix}' ");
            }
            if (this.LineTerminator != "\n")
            {
                builder2.Append($"TERMINATED BY '{this.LineTerminator}' ");
            }
            if (builder2.Length > 0)
            {
                builder.Append($"LINES {builder2}");
            }
            if (this.NumberOfLinesToSkip > 0)
            {
                builder.Append($"IGNORE {this.NumberOfLinesToSkip} LINES ");
            }
            if (this.Columns.Count > 0)
            {
                builder.Append($"({this.Columns.ToArray().ToStr(',')}) ");
            }
            return builder.ToString();

        }

        private string _BuildPostgreSQLCommand()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($@"COPY {TableName} ");
            if (Columns.Count > 0)
            {
                builder.Append($"({Columns.ToArray().ToStr(',')}) ");
            }
            builder.Append($"FROM STDIN ");
            //builder.Append($"'{FileName.Replace(@"\", @"/")}' ");
            StringBuilder builder2 = new StringBuilder();
            if (NumberOfLinesToSkip > 0)
            {
                builder2.Append("HEADER true ");
            }
            else
            {
                builder2.Append("HEADER false ");
            }
            //builder2.Append(",FORMAT 'CSV'");
            if (FieldTerminator + "" != "")
            {
                builder2.Append($",DELIMITER '{FieldTerminator.GetChar()}' ");
            }
            //if (FieldQuotationCharacter + "" != "")
            //{
            //    builder2.Append($",QUOTE '{FieldQuotationCharacter}' ");
            //}
            if (EscapeCharacter + "" != "" && EscapeCharacter != "\\")
            {
                builder2.Append($",ESCAPE  '{EscapeCharacter}' ");
            }
            if (this.CharacterSet + "" != "")
            {
                builder2.Append($",ENCODING '{this.CharacterSet}' ");
            }
            builder.Append($"WITH({builder2}) ");
            return builder.ToString();
        }

        public void Dispose()
        {
            try
            {
                if (this._client.State != ConnectionState.Closed)
                {
                    this._client.Close();
                }
            }
            catch { }
            this._client.Dispose();
            this._columns = null;
        }
        #endregion
    }
}

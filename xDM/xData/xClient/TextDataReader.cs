using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Data.OleDb;

namespace xDM.xData.xClient
{
    /**
     *从文本文件中读取数据
     **/
    public class TextDataReader : IDataReader
    {
        /// <summary>
        /// 文件全路径名称
        /// </summary>
        private string _fileName;
        /// <summary>
        /// 数据源是否打开标志
        /// </summary>
        private bool _isOpened { get; set; }
        /// <summary>
        /// 文件流
        /// </summary>
        private StreamReader _fileReader;
        /// <summary>
        /// 读取当前行的数据
        /// </summary>
        private string _currentLine;
        /// <summary>
        /// 当前行数据
        /// </summary>
        private object[] _currentValues;
        /// <summary>
        /// 字符两端空格处理模式
        /// </summary>
        private TrimMode _trimMode { get; set; }
        /// <summary>
        /// 字段之间的分隔符
        /// </summary>
        private char _fieldTermiter = ',';
        /// <summary>
        /// 字段封闭符号
        /// </summary>
        private char _enclosed { get; set; }
        /// <summary>
        /// 表字段名称
        /// </summary>
        private Dictionary<string, int> _colNames { get; set; } = new Dictionary<string, int>();
        private DataTable _schemaTable { get; set; }
        /// <summary>
        /// 要操作的字段index，如{0,1,2,3,4,}，数量和schemaTable的Colunms数量一至，如果index超出，值设置为"",此参数可为null
        /// </summary>
        private int[] _fieldsNums { get; set; }

        private Encoding _encoding { get; set; }
        

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="encoding">编码，null表示自动获取编码（不一定准确）</param>
        /// <param name="fieldTermiter">字段分割符号</param>
        /// <param name="enclosed">字段封闭符号，'\0'表示无封闭符号</param>
        /// <param name="schemaTable">字段名称、类型、表名在这里设置</param>
        /// <param name="fieldsNums">要操作的字段index，如{0,1,2,3,4,}，数量和schemaTable的Colunms数量一至，如果index超出，值设置为"",此参数可为null</param>
        /// <param name="trimMode"></param>
        public TextDataReader(string fileName, System.Text.Encoding encoding, char fieldTermiter, char enclosed, DataTable schemaTable, int[] fieldsNums, TrimMode trimMode)
        {
            _fileName = fileName;
            _trimMode = trimMode;
            _schemaTable = schemaTable;
            _fieldTermiter = fieldTermiter;
            _enclosed = enclosed;
            for (int i = 0; i < _schemaTable.Columns.Count; i++)
                _colNames.Add(_schemaTable.Columns[i].ColumnName, i);
            var count = FieldCount;
            if (fieldsNums != null)
                _fieldsNums = fieldsNums;
            else
            {
                _fieldsNums = new int[count];
                for (int i = 0; i < count; i++)
                    _fieldsNums[i] = i;
            }
            if (_fieldsNums.Length != count)
                throw new Exception("要操作的字段与schemaTable字段数目不一致！");
            if (encoding == null)
                encoding = Extensions.MyFunction.GetEncoding(fileName);
            _encoding = encoding;
            _fileReader = new StreamReader(fileName, encoding);
            _isOpened = true;
        }

        #region 实现接口的方法成员
        public void Close()
        {
            if (_fileReader != null)
                _fileReader.Close();
            _isOpened = false;
        }

        public void Dispose()
        {
            _fileReader.Dispose();
        }

        public bool GetBoolean(int i)
        {
            return Convert.ToBoolean(_currentValues[i]);
        }

        public Byte GetByte(int i)
        {
            return Convert.ToByte(_currentValues[i]);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            if (i < 0 || i > FieldCount - 1)
                throw new IndexOutOfRangeException($"字段索引超出范围！（0 - {FieldCount - 1}）");
            var bytes = _encoding.GetBytes(_currentValues[i] + "");
            var j = 0;
            for (int lenBytes = bytes.Length,lenBuffer = buffer.Length; j < length && j + fieldOffset < lenBytes && j + bufferoffset < lenBuffer; j++)
                buffer[j + bufferoffset] = bytes[j + fieldOffset];
            return j;
        }

        public char GetChar(int i)
        {
            return Convert.ToChar(_currentValues[i]);
        }

        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferoffset, int length)
        {
            if (i < 0 || i > FieldCount - 1)
                throw new IndexOutOfRangeException($"字段索引超出范围！（0 - {FieldCount - 1}）");
            var chars = (_currentValues[i] + "").ToArray();
            var j = 0;
            for (int lenBytes = chars.Length, lenBuffer = buffer.Length; j < length && j + fieldOffset < lenBytes && j + bufferoffset < lenBuffer; j++)
                buffer[j + bufferoffset] = chars[j + fieldOffset];
            return j;
        }

        /// <summary>
        /// 返回指定的列序号的 IDataReader
        /// </summary>
        /// <param name="i">列号</param>
        /// <returns>IDataReader</returns>
        public IDataReader GetData(int i)
        {
            if (i == 0)
                return this;
            return null;
        }

        public string GetDataTypeName(int i)
        {
            return _schemaTable.Columns[i].DataType.Name;
        }

        public DateTime GetDateTime(int i)
        {
            return Convert.ToDateTime(_currentValues[i]);
        }

        public decimal GetDecimal(int i)
        {
            return Convert.ToDecimal(_currentValues[i]);
        }

        public double GetDouble(int i)
        {
            return Convert.ToDouble(_currentValues[i]);
        }

        public Type GetFieldType(int i)
        {
            return _schemaTable.Columns[i].DataType;
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(_currentValues[i]);
        }

        public Guid GetGuid(int i)
        {
            Guid gid;
            if (Guid.TryParse(_currentValues[i] + "", out gid))
                return gid;
            throw new Exception($"{_currentValues[i] + ""}不是正确的 Guid!");
        }

        public Int16 GetInt16(int i)
        {
            return Convert.ToInt16(_currentValues[i]);
        }

        public Int32 GetInt32(int i)
        {
            return Convert.ToInt32(_currentValues[i]);
        }

        public Int64 GetInt64(int i)
        {
            return Convert.ToInt64(_currentValues[i]);
        }

        public string GetName(int i)
        {
            return _schemaTable.Columns[i].ColumnName;
        }

        public int GetOrdinal(string name)
        {
            if (_colNames.ContainsKey(name))
                return _colNames[name];
            return -1;
        }

        public DataTable GetSchemaTable()
        {
            return _schemaTable;
        }
        public string GetString(int i)
        {
            return _currentValues[i].ToString();
        }

        public Object GetValue(int i)
        {
            object value = _currentValues[i];
            if (IsDBNull(i))
                value = DBNull.Value;
            return value;
        }

        public int GetValues(Object[] values)
        {
            values = _currentValues;
            if (_currentValues != null)
                return _currentValues.Length;
            else return 0;
        }

        public bool IsDBNull(int i)
        {
            return _currentValues[i] + "" == "NULL";
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            bool readSucc = true;

            while ((_currentLine = _fileReader.ReadLine()) != null)
            {
                if (_currentLine + "" != "")
                    break;
            }
            if (_currentLine == null)
            {
                _currentValues = null;
                return false;
            }
            var line = _currentLine;
            string[] datas;
            if (_enclosed != '\0')
            {
                List<string> listDatas = new List<string>();
                var len = _currentLine.Length;
                var subLen = -1;
                var open = false;
                foreach (var c in _currentLine)
                {
                    subLen++;
                    if (c == _enclosed)
                        open = !open;
                    else if (c == _fieldTermiter && !open)
                    {
                        var start = 0;
                        var end = subLen;
                        if (line[0] == _enclosed && line[end - 1] == _enclosed)
                        {
                            start++;
                            end -= 2;
                        }
                        listDatas.Add(line.Substring(start, end));
                        line = line.Substring(subLen + 1);
                        subLen = -1;
                    }
                }
                datas = listDatas.ToArray();
            }
            else
            {
                datas = _currentLine.Split(_fieldTermiter);
            }
            var count = FieldCount;
            var length = datas.Length;
            object[] values = new object[count];
            for (int i = 0; i < count; i++)
            {
                var idx = _fieldsNums[i];
                if (i < length && idx < length)
                    values[i] = TrimEnclosed(_trimMode, datas[idx]);
                else
                    //values[i] = DBNull.Value;  
                    values[i] = "";
            }

            _currentValues = values.ToArray();
            return readSucc;
        }
        private string TrimEnclosed(TrimMode mode, string val)
        {
            if (val == null || val == "") return "";
            switch (_trimMode)
            {
                case TrimMode.TrimStart:
                    val = val.TrimStart();
                    break;
                case TrimMode.TrimEnd:
                    val = val.TrimEnd();
                    break;
                case TrimMode.Both:
                    val = val.Trim();
                    break;
            }
            var len = val.Length;
            if (_enclosed != '\0' && len >= 2 && val[0] == _enclosed && val[len - 1] == _enclosed)
                val = val.Substring(1, len - 2);
            return val;
        }


        public object this[int i]
        {
            get { return _currentValues[i]; }
        }

        public object this[string name]
        {
            get
            {
                int index = GetOrdinal(name);
                if (index == -1)
                    return null;
                else
                    return _currentValues[index];
            }
        }
        #endregion

        #region 实现接口的属性成员
        public int Depth
        {
            //不支持行的嵌套，总返回0
            get { return 0; }
        }

        public int FieldCount
        {
            get { return _schemaTable.Columns.Count; }
        }

        public bool IsClosed
        {
            get { return !_isOpened; }
        }

        public int RecordsAffected
        {
            //不支持SQL操作，总返回-1
            get { return -1; }
        }
        #endregion

        public enum TrimMode
        {
            TrimStart = 1,
            TrimEnd = 2,
            Both = 3,
            None = 4
        }
    }
}

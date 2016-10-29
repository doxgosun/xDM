using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xData.xClient
{
    public class BulkColumnMapping
    {
        private string _destinationColumn = null;
        private int _destinationOrdinal = -1;
        private string _sourceColumn = null;
        private int _sourceOrdinal = -1;

        // 摘要: 
        //     用于初始化新的 System.xClient.BulkCopyColumnMapping 对象的默认构造函数。
        //
        // 摘要: 
        //     创建新的列映射，并使用列序号引用源列和目标列。
        //
        // 参数: 
        //   sourceColumnOrdinal:
        //     数据源中源列的序号位置。
        //
        //   destinationOrdinal:
        //     目标表中目标列的序号位置。
        public BulkColumnMapping(int sourceColumnOrdinal, int destinationOrdinal)
        {
            this._sourceOrdinal = sourceColumnOrdinal;
            this._destinationOrdinal = destinationOrdinal;
        }
        //
        // 摘要: 
        //     创建新的列映射，并使用列序号引用源列和目标列的列名称。
        //
        // 参数: 
        //   sourceColumnOrdinal:
        //     数据源中源列的序号位置。
        //
        //   destinationColumn:
        //     目标表中目标列的名称。
        public BulkColumnMapping(int sourceColumnOrdinal, string destinationColumn)
        {
            this._sourceOrdinal = sourceColumnOrdinal;
            this._destinationColumn = destinationColumn;
        }
        //
        // 摘要: 
        //     创建新的列映射，并使用列名称引用源列和目标列的列序号。
        //
        // 参数: 
        //   sourceColumn:
        //     数据源中源列的名称。
        //
        //   destinationOrdinal:
        //     目标表中目标列的序号位置。
        public BulkColumnMapping(string sourceColumn, int destinationOrdinal)
        {
            this._sourceColumn = sourceColumn;
            this._destinationOrdinal = destinationOrdinal;
        }
        //
        // 摘要: 
        //     创建新的列映射，并使用列名称引用源列和目标列。
        //
        // 参数: 
        //   sourceColumn:
        //     数据源中源列的名称。
        //
        //   destinationColumn:
        //     目标表中目标列的名称。
        public BulkColumnMapping(string sourceColumn, string destinationColumn)
        {
            this._sourceColumn = sourceColumn;
            this._destinationColumn = destinationColumn;
        }

        // 摘要: 
        //     正在目标数据库表中映射的列的名称。
        //
        // 返回结果: 
        //     System.xClient.BulkCopyColumnMapping.DestinationColumn 属性的字符串值。
        public string DestinationColumn { get { return this._destinationColumn; } }
        //
        // 摘要: 
        //     目标表中目标列的序号值。
        //
        // 返回结果: 
        //     System.xClient.BulkCopyColumnMapping.DestinationOrdinal 属性的整数值，或者如果尚未设置该属性，则为
        //     -1。
        public int DestinationOrdinal { get { return this._destinationOrdinal; } }
        //
        // 摘要: 
        //     正在数据源中映射的列的名称。
        //
        // 返回结果: 
        //     System.xClient.BulkCopyColumnMapping.SourceColumn 属性的字符串值。
        public string SourceColumn { get { return this._sourceColumn; } }
        //
        // 摘要: 
        //     数据源中源列的序号位置。
        //
        // 返回结果: 
        //     System.xClient.BulkCopyColumnMapping.SourceOrdinal 属性的整数值。
        public int SourceOrdinal { get { return this._sourceOrdinal; } }
    }
}

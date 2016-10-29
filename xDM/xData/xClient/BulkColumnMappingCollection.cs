using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xDM.xData.xClient
{
    public class BulkColumnMappingCollection : CollectionBase
    {
        // 摘要: 
        //     获取位于指定索引位置的 System.xClient.BulkCopyColumnMapping 对象。
        //
        // 参数: 
        //   index:
        //     要查找的 System.xClient.BulkCopyColumnMapping 的从零开始的索引。
        //
        // 返回结果: 
        //     一个 System.xClient.BulkCopyColumnMapping 对象。
        public BulkColumnMapping this[int index] { get { return (BulkColumnMapping)this.InnerList[index]; } }

        // 摘要: 
        //     将指定的映射添加到 System.xClient.BulkCopyColumnMappingCollection 中。
        //
        // 参数: 
        //   bulkCopyColumnMapping:
        //     描述要添加到集合中的映射的 System.xClient.BulkCopyColumnMapping 对象。
        //
        // 返回结果: 
        //     一个 System.xClient.BulkCopyColumnMapping 对象。
        public BulkColumnMapping Add(BulkColumnMapping bulkColumnMapping)
        {
            this.InnerList.Add(bulkColumnMapping);
            return bulkColumnMapping;
        }
        //
        // 摘要: 
        //     通过使用序号指定源列和目标列，创建一个新的 System.xClient.BulkCopyColumnMapping 并将其添加到集合中。
        //
        // 参数: 
        //   sourceColumnIndex:
        //     数据源中源列的序号位置。
        //
        //   destinationColumnIndex:
        //     目标表中目标列的序号位置。
        //
        // 返回结果: 
        //     System.xClient.BulkCopyColumnMapping。
        public BulkColumnMapping Add(int sourceColumnIndex, int destinationColumnIndex)
        {
            BulkColumnMapping bulkCopyColumnMapping = new BulkColumnMapping(sourceColumnIndex, destinationColumnIndex);
            this.InnerList.Add(bulkCopyColumnMapping);
            return bulkCopyColumnMapping;
        }
        //
        // 摘要: 
        //     通过对源列使用序号和对目标列使用字符串，创建一个新的 System.xClient.BulkCopyColumnMapping
        //     并将其添加到集合中。
        //
        // 参数: 
        //   sourceColumnIndex:
        //     数据源中源列的序号位置。
        //
        //   destinationColumn:
        //     目标表中目标列的名称。
        //
        // 返回结果: 
        //     System.xClient.BulkCopyColumnMapping。
        public BulkColumnMapping Add(int sourceColumnIndex, string destinationColumn)
        {
            BulkColumnMapping bulkCopyColumnMapping = new BulkColumnMapping(sourceColumnIndex, destinationColumn);
            this.InnerList.Add(bulkCopyColumnMapping);
            return bulkCopyColumnMapping;
        }
        //
        // 摘要: 
        //     通过使用列名称描述源列，同时使用序号指定目标列，从而创建一个新的 System.xClient.BulkCopyColumnMapping
        //     并将其添加到集合中。
        //
        // 参数: 
        //   sourceColumn:
        //     数据源中源列的名称。
        //
        //   destinationColumnIndex:
        //     目标表中目标列的序号位置。
        //
        // 返回结果: 
        //     System.xClient.BulkCopyColumnMapping。
        public BulkColumnMapping Add(string sourceColumn, int destinationColumnIndex)
        {
            BulkColumnMapping bulkCopyColumnMapping = new BulkColumnMapping(sourceColumn, destinationColumnIndex);
            this.InnerList.Add(bulkCopyColumnMapping);
            return bulkCopyColumnMapping;
        }
        //
        // 摘要: 
        //     通过使用列名称指定源列和目标列，创建一个新的 System.xClient.BulkCopyColumnMapping 并将其添加到集合中。
        //
        // 参数: 
        //   sourceColumn:
        //     数据源中源列的名称。
        //
        //   destinationColumn:
        //     目标表中目标列的名称。
        //
        // 返回结果: 
        //     System.xClient.BulkCopyColumnMapping。
        public BulkColumnMapping Add(string sourceColumn, string destinationColumn)
        {
            BulkColumnMapping bulkCopyColumnMapping = new BulkColumnMapping(sourceColumn, destinationColumn);
            this.InnerList.Add(bulkCopyColumnMapping);
            return bulkCopyColumnMapping;
        }

        //
        // 摘要: 
        //     获取一个值，该值指示集合中是否存在指定的 System.xClient.BulkCopyColumnMapping 对象。
        //
        // 参数: 
        //   value:
        //     有效的 System.xClient.BulkCopyColumnMapping 对象。
        //
        // 返回结果: 
        //     如果集合中存在指定映射，则为 true；否则为 false。
        public bool Contains(BulkColumnMapping value)
        {
            return this.InnerList.Contains(value);
        }
        //
        // 摘要: 
        //     从特定索引处开始，将 System.xClient.BulkCopyColumnMappingCollection 的元素复制到
        //     System.xClient.BulkCopyColumnMapping 项的数组。
        //
        // 参数: 
        //   array:
        //     作为从 System.xClient.BulkCopyColumnMappingCollection 复制的元素的目标位置的一维
        //     System.xClient.BulkCopyColumnMapping 数组。该数组必须具有从零开始的索引。
        //
        //   index:
        //     array 中从零开始的索引，在此处开始复制。
        public void CopyTo(BulkColumnMapping[] array, int index)
        {
            this.InnerList.CopyTo(array, index);
        }
        //
        // 摘要: 
        //     获取指定 System.xClient.BulkCopyColumnMapping 对象的索引。
        //
        // 参数: 
        //   value:
        //     要搜索的 System.xClient.BulkCopyColumnMapping 对象。
        //
        // 返回结果: 
        //     列映射的从零开始的索引；如果在集合中未找到该列映射，则为 -1。
        public int IndexOf(BulkColumnMapping value)
        {
            return this.InnerList.IndexOf(value);
        }
        //
        // 摘要: 
        //     在指定索引处插入一个新 System.xClient.BulkCopyColumnMapping。
        //
        // 参数: 
        //   index:
        //     System.xClient.BulkCopyColumnMappingCollection 中要插入新 System.xClient.BulkCopyColumnMapping
        //     的位置的整数值。
        //
        //   value:
        //     要插入集合中的 System.xClient.BulkCopyColumnMapping 对象。
        public void Insert(int index, BulkColumnMapping value)
        {
            this.InnerList.Insert(index, value);
        }
        //
        // 摘要: 
        //     从 System.xClient.BulkCopyColumnMappingCollection 中移除指定的 System.xClient.BulkCopyColumnMapping
        //     元素。
        //
        // 参数: 
        //   value:
        //     要从集合中移除的 System.xClient.BulkCopyColumnMapping 对象。
        public void Remove(BulkColumnMapping value)
        {
            this.InnerList.Remove(value);
        }
        //
        // 摘要: 
        //     从集合中移除指定索引处的映射。
        //
        // 参数: 
        //   index:
        //     要从集合中移除的 System.xClient.BulkCopyColumnMapping 对象的从零开始的索引。
        public new void RemoveAt(int index)
        {
            this.InnerList.Remove(index);
        }
    }
}

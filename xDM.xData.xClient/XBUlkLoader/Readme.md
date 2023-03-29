

# 基于 XDataClient 的快速导入文件到数据库

## MySql/Vertica/Oracle 会使用原生Load Sql，其他会使用 TextDataReader 将文件读取成 IDataReader，再使用 XBulkCopy.WriteToServer() 进行数据导入
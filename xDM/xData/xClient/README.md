#xDM.xData.xClient

一个支持各种数据库的大DBHelper

使用方法：

引用 xDM.xData.xClient.dll
下载数据库驱动 xDbDrevers,放到程序目录下

using xDM.xData.xClient;

var client = new xDM.xData.xClient.DataClient(ClientType.SQLite);
client.ConnectionString = "xxxxxx";
client.CommandText = "xxxxx";
var dt = client.ExecuteDataTable();
......

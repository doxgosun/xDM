#xDM.xData.xClient

可以理解为一个支持各种数据库的DBHelper，但功能理强大，使用更方便

使用方法：

引用 xDM.xData.xClient.dll

下载数据库驱动 xDbDrevers,放到程序目录下

https://pan.baidu.com/s/1gfFPK0j#list/path=%2FGitHub%2FxDM%2FxDbDrivers&parentPath=%2FGitHub

using xDM.xData.xClient;

var client = new DataClient(ClientType.SQLite);

client.ConnectionString = "xxxxxx";

client.CommandText = "xxxxx";

var dt = client.ExecuteDataTable();

......

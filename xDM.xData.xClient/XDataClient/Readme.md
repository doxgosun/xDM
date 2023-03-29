
# 数据库安全使用原则：能用参数化查询绝对不拼接语句

## XDataClient
### XDataClient为DataClient升级版本，修复一些BUG，增加支持缓存/开启新连接查询等功能
### 创建对象
#### 使用xDM提供的驱动
##### xDM提供的驱动，获取数据库驱动 xDbDrivers6，放程序可执行文件目录下，win下可以放在 windows目录下，linux放在/usr/xdm 下
```
    //使用xDM提供的驱动
    var client = new xDM.xData.xClient.XDataClient(xDM.xData.DatabaseType.Vertica);
````

#### 使用其他的驱动
##### 使用各种数据库提供的驱动，nuget包或dll都可以，正常引用即可
````
    //使用其他的驱动，只需要提供连接的Type
    var connType = typeof(Vertica.Data.VerticaClient.VerticaConnection);
    var client = new xDM.xData.xClient.XDataClient(connType);
```

### 设置连接字符串
#### 方式一（使用默认）
```
    //方式一（使用默认）
    client.SetAdoDbConnectionString("127.0.0.1", -1, "sa", "ggg123", "dbtest");
````

#### 方式二
````
    //方式二
    var connStr = "Server={0},{4};Database={1};Uid={2};PWD='{3}';";
    client.SetConnectionString(connStr);
```

### 基本用法
```
    var sql = "select * from rawdata.data_ticket_today limit 100";

    var dt = client.ExecuteDataTable(sql); //返回 DataTable

    sql = "delete from user_table where user id = 88";
    var cnt = client.ExecuteNonQuery(sql); //返回受影响的行数

    //如果结果只有一行一列，则可以这样
    sql = "select count(*) from rawdata.data_ticket_today";
    var count = client.ExecuteScalar(sql); //返回唯一值


    //如果sql有多句组成（用分号分割），则可以返回DataSet
    sql = "语句0; 语句1;";
    var ds = client.ExecuteDataSet(sql); //返回数据集

    //或返回多个语句中指定索引语句的 DataTable
    dt = client.ExecuteDataTable(sql, 1); //返回 语句1 的结果

    //当然，也可以返回一个 DataReader，用于。。。你想干嘛就干嘛吧，比如大批量数据的导出/移库等
    var reader = client.ExecuteReader(sql);
```

### 导出数据到文件
```
    /// <summary>
    /// 执行查询,将结果导出到文本文件，默认为 csv 文件
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="fileName">要导出文件的完整路径</param>
    /// <param name="fieldTerminator">字段分割符，默认\t</param>
    /// <param name="enclosed">字段封闭符号,一般为空(null)或双引号</param>
    /// <param name="firstFieldName">是否保存首行为字段名</param>
    /// <param name="parameters"></param>
    /// <param name="trimMode"></param>
    /// <param name="encoding">编码</param>
    /// <param name="onWriting">写文件过程中报告已写行数</param>
    /// <param name="reportCount">多少行报告一次，默认为1000</param>
    /// <returns>导出文件的记录数,不包括字段名行,导出失败则返回-1</returns>

    var linesCount = client.Export(sql, "保存的文件.cxv", Encoding.UTF8, ';', "\"", true);
```

### 批量导入数据
#### 从文件导入
```
    /// <summary>
    /// 将文件导入到数据库中（使用 BulkLoad）
    /// </summary>
    /// <param name="destinationTableName">目标表</param>
    /// <param name="fileName">要导入的文件完整路径</param>
    /// <param name="encoding">文件编码</param>
    /// <param name="firstRowIsFieldNames">文件首行为字段名，如果为 false，则必须配置 fieldNames（字段名） 参数</param>
    /// <param name="fieldNames">如果 firstRowIsFieldNames=false，则此参数为必须，要求字段不包含除了下划线外的任何符号，使用 fieldTerminator 定义的符号分割（默认为英文逗号），可以有封闭符。
    /// 如件首行字段名使用同样要求</param>
    /// <param name="fieldTerminator"></param>
    /// <param name="enclosed"></param>
    /// <returns></returns>

    client.Import("目标表", "文件.csv", Encoding.UTF8, true, "id,name,val", ',', "");
````

#### 从 DataTable 导入
````
    // dt = 从其他地方来的DataTable 如 Excel 表
    //批量导入数据，请确保 DataTable 的字段名在目标表中存在，如果字段名需要映射，请使用 XBulkCopy
    client.Import("目标表", dt);
````

#### 从 IDataReader 导入
```
    // reader = 从其他地方来的 IDataReader
    //批量导入数据，请确保 IDataReader 的字段名在目标表中存在，如果字段名需要映射，请使用 XBulkCopy
    client.Import("目标表", reader);
```

### 参数化查询
```
    //参数化查询，参数化查询可以用在所有的 ExcuteXXX 方法里，所有带 parameters 参数的函数用法一样

    //比如：
    //如果你的应用拼接的sql没有做严格的过滤，则有可能造成sql注入漏洞，参数化查询可以规避此风险
    //比如查询账号密码
    sql = "select count(*) from user where username=@user and pw=@pw"; //@为sqlserver的参数化查询前缀，不同的数据不同
    var parameters = client.CreateParameterCollection();
    parameters.AddWithValue("@user", "admin");
    parameters.AddWithValue("@pw", "admin123");
    var c = client.ExecuteScalar(sql, parameters: parameters);
    if (int.Parse(c + "") > 0)
    {
        //用户名密码正确
    }
```

### 另一种使用方法： ClientCommand
```
    //Command用法，和基本用法基本一至，sql不作为参数而是作为一个属性，方法名基本一至（类似旧 DataClient 的用法）
    var cmd = client.CreateClientCommand();
    cmd.CommandText = "select * from tb1 where id > 100 limit 100";
    dt = cmd.ExecuteDataTable();

    //也支持参数化查询
    cmd.CommandText = "select * from tb1 where id = @id";
    cmd.Parameters.Clear();
    cmd.Parameters.AddWithValue("@id", 99);
    dt = cmd.ExecuteDataTable();
```

### 使用事务
#### 方法一：使用 client 的公共连接
```
    //使用事务
    client.BeginTransaction(); //开启事务，client.CreateClientCommand() 产生的所有 Command 的查询也在同一事务上
    sql = "delete from user_table where user id = 88";
    cnt = client.ExecuteNonQuery(sql);
    cmd = client.CreateClientCommand();
    cmd.CommandText = "delete from user_table where user id = 99";
    cnt = cmd.ExecuteNonQuery();

    client.Commit(); //提交事务
    //或者回滚
    client.Rollback(); //回滚事务
```

#### 方法二：利用 ClientTransactionCommand 使用独立连接
```
    //由于 client 的事务使用公共连接，如果有单独连接使用事务的需求，则可以创建独立连接的 Command 用于事务。
    var tranCmd = client.CreateClientTransactionCommand();
    tranCmd.BeginTransaction(); //开启事务，这里的事务不影响 client 或 普通的 Command 
    //后面操作和 普通 Command 一至
    tranCmd.CommandText = "delete from user_table where user id = 88";
    cnt = tranCmd.ExecuteNonQuery();
    tranCmd.CommandText =  "delete from user_table where user id = 99";
    cnt = tranCmd.ExecuteNonQuery();

    tranCmd.Commit(); //提交事务
    //或者回滚
    tranCmd.Rollback(); //回滚事务
```

### 整合缓存
#### 使用默认的缓存
```
    //XDataClient 提供默认的缓存，使用 xDM.xCache.XSimpleMemCache 提供缓存功能

    /// <summary>
    /// 使用默认缓存，如果使用自定义缓存，请停止使用默认缓存
    /// </summary>
    /// <param name="aliveSeconds">对象缓存秒数，每次从缓存获取将会增加此生存时间</param>
    /// <param name="maxAliveSeconds">对象最大缓存秒数，超过此时间将从缓存清除</param>

    var aliveSeconds = 30; //缓存30秒，如果30秒内从缓存中获取值，则每次获取缓存时间增加30秒
    var maxAliveSeconds = 300; //最长缓存 300 秒，无论从缓存中获取值几次，超过这个生存时间就多缓存里清除，防止缓存老旧不更新
    client.UsingDefaultMemCache(aliveSeconds, maxAliveSeconds); //开户默认缓存 

    dt = client.ExecuteDataTable(sql); //第一次从数据库读取数据
    dt = client.ExecuteDataTable(sql); //请求参数没有变化，则从缓存里获取，提升效率，减少数据库压力

    client.StopUsingDefaultMemCache(); //停止使用默认缓存 
```

#### 使用自定义缓存
自定义缓存实现很简单，在查询执行前事件里根据 sql 及其他参数，在缓存里查询是否有结果，有则返回结果，没有则从数据库查询，在查询执行后，检查是否是从数据库查询的结果，是则缓存
```
    //使用自定义缓存
    client.BeforeExecute += Client_BeforeExecute; //处理查询执行前事件
    client.AfterExecute += Client_AfterExecute; //处理查询执行后事件

    //do something
```

```
    /// <summary>
    /// 执行查询前事件，在这里读取缓存
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Client_BeforeExecute(object sender, BeforeExecuteEventArgs e)
    {
        //你自己定义的key
        //var key = $"clientexecutekey:{SQL}||+{ExecuteMethod}+{ExecuteResultDataTableIndex}+||{Parameters.GetParametersInfo().JoinWith(",")}".Md5();
        var key = e.DefaultExecuteDataKey; //使用默认key: $"clientexecutekey:{SQL}||+{ExecuteMethod}+{ExecuteResultDataTableIndex}+||{Parameters.GetParametersInfo().JoinWith(",")}".Md5();

        var val = GetFromCache(key);
        if (val == null)
        {
            //根据类型设置返回值（执行 e.SetExecuteResult() 方法
            switch (e.ExecuteMethod)
            {
                case ExecuteMethod.ExecuteDataTable:
                    e.SetExecuteResult(转换成DataTable(val));
                    break;
                case ExecuteMethod.ExecuteDataSet:
                    e.SetExecuteResult(转换成DataSet(val));
                    break;
                case ExecuteMethod.ExecuteScalar:
                    e.SetExecuteResult(转换成你要的值类型(val));
                    break;
                default:
                    break;
            }
        }
    }
```

```
    /// <summary>
    /// 执行查询后事件，在这里设置缓存
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Client_AfterExecute(object sender, AfterExecuteEventArgs e)
    {
        //你自己定义的key
        //var key = $"clientexecutekey:{SQL}||+{ExecuteMethod}+{ExecuteResultDataTableIndex}+||{Parameters.GetParametersInfo().JoinWith(",")}".Md5();
        var key = e.DefaultExecuteDataKey; //使用默认key: $"clientexecutekey:{SQL}||+{ExecuteMethod}+{ExecuteResultDataTableIndex}+||{Parameters.GetParametersInfo().JoinWith(",")}".Md5();
        if (e.ExecuteResultFromDbServer) //从数据服务器返回了数据
        {
            var val = e.ExecuteResult;
            //SetToCache(key, val); //保存到缓存里
        }
    }
```
## 扩展高级用法
这里是DataClient的扩展方法，不是DataClient自带的方法，要 using xDM.xData.xClient;

### 分页查询
#### 获取表的分页数据
```
    /// <summary>
    /// 获取表的分页数据，字段为表所有字段，效率嘛。。。。未测试，别抱太大希望,不支持sybase!!!!!!!!!!!DB2没有环境测试！
    /// </summary>
    /// <param name="client"></param>
    /// <param name="fromTable">表名</param>
    /// <param name="selectFields">要查询的字段名</param>
    /// <param name="identityName">id字段名，access sqlserver要用到，mysql oracle可写null</param>
    /// <param name="page">要获取的页,从1开始</param>
    /// <param name="pageSize">每页数据量</param>
    /// <param name="where">如果有where条件，这里传入(不带where关键字)，如果是参数化查询，请预先Parameters.AddWithValue(),否则先Parameters.Clear()</param>
    /// <param name="orderBy">排序，如“order by id desc”</param>
    /// <returns></returns>
    
    var dt = client.GetPagingDataTableFromDB(string fromTable, string selectFields, 
        string identityName, int page, int pageSize, string where = null, string orderBy = null, IDataParameterCollection parameters = null)
```

#### 获取表的分页数据，并返回数据总数
```
    /// <summary>
    /// 获取表的分页数据，字段为表所有字段，效率嘛。。。。别抱希望,不支持sybase!!!!!!!!!!!DB2没有环境测试！
    /// </summary>
    /// <param name="client"></param>
    /// <param name="total">输出总数</param>
    /// <param name="fromTable">表名</param>
    /// <param name="selectFields">要查询的字段名</param>
    /// <param name="identityName">id字段名，access sqlserver要用到，mysql oracle可写null</param>
    /// <param name="page">要获取的页,从1开始</param>
    /// <param name="pageSize">每页数据量</param>
    /// <param name="where">如果有where条件，这里传入(不带where关键字)，如果是参数化查询，请预先Parameters.AddWithValue(),否则先Parameters.Clear()</param>
    /// <param name="orderBy">排序，如“order by id desc”</param>
    /// <returns></returns>

    var dt = client.GetPagingDataTableFromDB(out int total, string fromTable, 
        string selectFields, string identityName, int page, int pageSize, string where = null, string orderBy = null
        , IDataParameterCollection parameters = null)
```

### ORM 实体类映射
#### 的一个实体类，字段属性和数据表一一对应，如：

```
    [TableInfo("表名", "ID列名，如 T_ID, 一般为主键列")]
    public class YourData : xDM.xData.ITxDbEntity
    {
	    public int T_ID { get; set; }
	    public string Name { get; set; }
	    public int Age { get; set; }
	    public DateTime? AddTime { get; set; }

	    [NoInsert]// 不会插入到数据库的字段
	    public string NiMaBi { get; set; }

	    [NoUpdate]// 不会被UPDATE到数据库的字段 
	    public string NiDaYe { get; set; }

	    [NotDbField]// 这个字段不是数据表内字段，不会被插入编辑
	    public bool WoBuShiWo { get; set; }
    }
```
#### 调用方式
所有方法均支持两种调用方式，并且两种方式是等价的
client.方法<实体类名>(参数);     //调用方式一：泛型调用
client.方法(实体类型, 其他参数); //调用方式二：类型调用
如：client.GetEntityByID<YourData>(1);
和：client.GetEntityByID(typeof(YourData), 1);
两者是等价的。
下面使用泛型调用的例子，如果有需要，都可自行转换成类型调用

#### 单实体操作
```
    //获取单个实体
    var entity1 = client.GetEntityByID<YourData>(1);
    var entity2 = client.GetEntityByID<YourData>(1, "表2"); //从 表2 获取数据

    //更新单个实体
    entity1.Name = "NiDaYe";
    client.UpdateEntity(entity1);
    client.UpdateEntity(entity1, "表2");

    //插入单个实体
    var yourData = new YourData();
    yourData.Name = "YourName";
    client.InsertEntity(yourData);
    client.InsertEntity(yourData, "表2");

    //删除单个实体
    client.DeleteEntity(entity1);
    client.DeleteEntity(entity1, "表2");

    //通过id删除单个实体
    client.DeleteEntityByID<YourData>(9);
    client.DeleteEntityByID<YourData>(9, "表2");

```
#### 查询返回多个实体
##### 基本使用
```
    //获取实体列表
    var entities1_1 = client.GetEntities<YourData>(); 
    //也可以返回DataTable
    var dtAll = client.GetEntitiesDataTable<YourData>();

    //获取分页数据
    var page = 1; //第1页(从1开始)
    var pageSize = 20; //每页数
    var entitiesPagging = client.GetEntities<YourData>(page, pageSize);
    //也可以返回DataTable
    var dtPagging = client.GetEntitiesDataTable<YourData>(page, pageSize);

    //如果分布的时间要返回总数，则可以使用 out total 参数
    var total = -1;
    var entitiesPaggingWithTotal = client.GetEntities<YourData>(out total, page, pageSize);
    //也可以返回DataTable
    var dtPaggingWithTotal = client.GetEntitiesDataTable<YourData>(out total, page, pageSize);

    //模糊查询，如果表数据量巨大，最好不要把%放后面
    var entitiesLike1 = client.GetEntitiesLike<YourData>("%宋%"); //查询所有字段 like %宋%
    //也可以返回DataTable
    var entitiesLike1 = client.GetEntitiesDataTableLike<YourData>("%宋%"); 
    //也可以分页
    var entitiesLikePagging = client.GetEntitiesLike<YourData>(page, pageSize, "%宋%"); 
    var entitiesLikePaggingWithTotal = client.GetEntitiesLike<YourData>(out total, page, pageSize, "%宋%"); 
    //也可以返回分页DataTable
     var entitiesDtLikePagging = client.GetEntitiesDataTableLike<YourData>(page, pageSize, "%宋%"); 
    var entitiesDtLikePaggingWithTotal = client.GetEntitiesDataTableLike<YourData>(out total, page, pageSize, "%宋%");     
```
##### 其他可选参数
###### 查询返回多个实体的查询方法支持其他可选参数，以实现更多实用的功能
```
    var entitiess = client.GetEntities<YourData>(where: "1 = 2", orderBy: "AddTime,Name", selectFields: "Name,NiMaBi", tableName: "从别的表获取");
```
    //删除一组实体
    var delCount1 = client.DeleteEntities<YourData>("T_ID > 3");
    var delCount2 = client.DeleteEntities(typeof(YourData), "T_ID > 3");

    /*****************************************************/
    /* 所有方法均可带tableName参数，从指定的表里获取数据 */
    /*****************************************************/

```
#### 还有其他，自己挖掘
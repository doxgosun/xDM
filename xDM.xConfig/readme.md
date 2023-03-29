
# xConfig 配置文件操作相关类
## 配置管理器 XConfigor 可读取配置文件到配置类实例/自动监控配置文件的更改/自动监控配置对象属性的改变

## 使用方法：

### 字典方式使用
#### 基本的配置使用方式，配置文件为  name = value 的形式
你有一个配置文件 yourconfig.txt ：

```

# #号开关作为注释
IP=127.0.0.1

#端口号
Port=3389


```

### 配置对象方式使用

### 自定义配置文件格式使用，如 json/xml/yaml等

你有一个配置文件 yourconfig.txt ：

```

# #号开关作为注释
IP=127.0.0.1

#端口号
Port=3389

```

程序示例代码：

```
using xDM.xConfig;

static void Main(string[] args)
{
	//获取一个配置管理器
    var configFile =  "yourconfig.txt";
    var configor = XConfigor.New(configFile);//配置类形式
    //var configor = XConfigor.New(configFile);//配置字典模式

    //配置管理器为全局对象，以配置文件路径为id，如果配置管理器已存在，可以这样获取
    // var configor = XConfigor.Get(configFile);

    //读取配置文件，如果配置文件不存在，则会自动新建
    configor.Load();

    //或你也可以读取其他的配置文件，读取后会更新configor.Config
    var otherConfig = configor.LoadFrom("其他配置文件路径");

    //你可以保存配置到文件
    configor.Save();

    //你也可以另存为
    configor.SaveAs("配置文件路径");


    //如果你要监听配置文件和配置变化，可以这样
    //监听文件改变事件
    configor.ConfigFileChenged += (sender, e)=> {
        //这里用lambda表达式，正常你应该使用事件正常的监听方法

        var cfgor = sender as XConfigor;

        //这里是新文件的配置（配置类模式）
        var newConfig = e.NewConfig;

        //配置字典，name-value形式
        var configDict = e.NewConfigDict;

        //这里是改变的配置项，你可以遍历看看哪些项目有了变化
        var changedProperties = e.ChangedProperties;

        //默认更新到你的配置里，如果不更新，设置 UpdateToConfig = false
        e.UpdateToConfig = false;
    };


    //如果你的配置对象的属性改变了，你要获取通知，可以这样
    configor.ConfigPropertiesChenged += (sender, e) =>
    {
        //这里用lambda表达式，正常你应该使用事件正常的监听方法


        var cfgor = sender as XConfigor;
        //这里是改变的配置项，你可以遍历看看哪些项目有了变化
        var changedProperties = e.ChangedProperties;

        //你可以保存配置到文件
        cfgor.Save();
    };

    //然后开启监控
    configor.StartWatchingConfigTask();

    //要停止监控
    configor.StopWatchingConfigTask();
}
```

同时建一个相应的配置类：
属性必须是 { get;set; }

类的成员必须为值类型或一个成员为值类型的类，最大深度为16，如果超过
```
public class YourConfig
{
	public string IP { get;set; }
	public int Port{ get;set; }
}
```

程序示例代码：

```
using xDM.xConfig;

static void Main(string[] args)
{
	//获取一个配置管理器
    var configFile =  "yourconfig.txt";
    var configor = XConfigor.New<YourConfig>(configFile);//配置类形式
    //var configor = XConfigor.New(configFile);//配置字典模式

    //配置管理器为全局对象，以配置文件路径为id，如果配置管理器已存在，可以这样获取
    // var configor = XConfigor.Get<YourConfig>(configFile);

    //读取配置文件，如果配置文件不存在，则会自动新建
    configor.Load();


    //你的配置为configor.Config，此为只读对象，更新/重新读取配置文件不会改变此对象的引用
    var config = configor.Config;

    configor.Load();

    //或你也可以读取其他的配置文件，读取后会更新configor.Config
    var otherConfig = configor.LoadFrom("其他配置文件路径");

    //你可以保存配置到文件
    configor.Save();

    //你也可以另存为
    configor.SaveAs("配置文件路径");



    //如果你要监听配置文件和配置变化，可以这样
    //监听文件改变事件
    configor.ConfigFileChenged += (sender, e)=> {
        //这里用lambda表达式，正常你应该使用事件正常的监听方法

        var cfgor = sender as XConfigor;

        //这里是新文件的配置（配置类模式）
        var newConfig = e.NewConfig;

        //配置字典，name-value形式
        var configDict = e.NewConfigDict;

        //这里是改变的配置项，你可以遍历看看哪些项目有了变化
        var changedProperties = e.ChangedProperties;

        //默认更新到你的配置里，如果不更新，设置如下 
        e.UpdateToConfig = false;
    };


    //如果你的配置对象的属性改变了，你要获取通知，可以这样
    configor.ConfigPropertiesChenged += (sender, e) =>
    {
        //这里用lambda表达式，正常你应该使用事件正常的监听方法


        var cfgor = sender as XConfigor;
        //这里是改变的配置项，你可以遍历看看哪些项目有了变化
        var changedProperties = e.ChangedProperties;

        //你可以保存配置到文件
        cfgor.Save();
    };

    //然后开启监控
    configor.StartWatchingConfigTask();

    //要停止监控
    configor.StopWatchingConfigTask();
}
```

# 其他静态方法及属性请参考源码或自己“点”开看看，不写了



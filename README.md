
# 写的很差的代码

## 相关工具类
## xDM6.nuget 包

### xDM.xData.xClient.DataClient
一个多数据库支持的客户端，支持基本sql数据库操作及实体类操作

### xDM.xLog.XLogger
类似 log4j 一类的logger，支持保存到数据库/文件，亦提供控制台下的个性化信息显示及输出到文件


### xDM.xCache.XSimpleMemCache
一个简单的本地内存缓存 ，应用级别，与应用同生命周期，占用本地内存，如果对象数量及内存占用巨大，请使用第三方缓存，如Redis/Memcached

### xDM.xConfig.XConfigor
配置文件操作相关类，可以支持 key = value 格式的配置文件，支持 #注释，也支持自定义格式，支持自动监听配置文件改动并自动更新配置

### xDM.xConfole.ConsoleBase
一个虚类，使用时需要被继承，命令行下实现一个类似Python中CMDLoop的东西，功能更屌一些

### xDM.xCrypto.XCryptor
一个支持RSA/SM3/SM4/SHA/MD5/HMac等等加解密的工具

### xDM.xIO.xFiles.XFileWatcher
对指定文件夹进行文件监控，监听文件的新建/修改/删除，通过遍历方式进行文件检测，时间为1秒以上，对于文件数量巨大的文件夹，请不要用这个，这个会占用一定的磁盘IO，如果需要更加好的效率，请使用巨硬自带的

### xDM.xNet.xFTP
FtpWebClient和XFtpClient，ftp操作工具

### xDM.xNet.xHttp.XHttpRequest
类似python的requests，用HttpWebRequest实现的一个web请求类，目的是让用户在python和C#之间切换不会突兀

### xDM.xNet.xHttp.XHttpServer
这里实现一个简单的静态HTTPServer，支持http及https

### xDM.xNet.xHttp.XHttpProxyServer
一个http代理程序，可以在https请求里替换你自己的PKI，从而实现证书登陆，用来。。。。。不说了，自己想象

### xDM.xPackage.XPackageBase
一个二进制数据的数据包协议，在串口/socket通讯工具里用到，使用在 XPackageFactory 可以进行包的合并，合并的包接收顺序可以是乱序

### xDM.xReflection.DelegateBuilder
通过EMIT创建类型的反射委托创建，能够对类的方法调用、构造函数调用，获取或设置属性和获取或设置字段提供委托，效率比直接反射要好

### xDM.xSafeDelivery.SafeDeliverer
安全交付工具，如果你处理一个数据要确保处理完成（就算断电、死机、各种意外），请使用这个工具，它将每一个处理的数据保存到磁盘，处理完成后删除，所以如果短时间处理大量数据，它可能会对磁盘造成较大的压力，这种情况请使用队列系统如 kafka

### xDM.xUtilites.xAssemblyInfo
程序集特性访问器，可以获取程序集信息，如作者/版本/版权等

## xDM.xExtensions
里面有几百或更多的扩展方法，如对字符串的扩展，把文字转换成拼音： "汉字".ToPinYin()

## xDM6.xIO.xPorts.xSerialPorts.nuget 包

### xDM.xIO.xPorts.xSerialPorts.XSerialPortService
一个串口通讯工具，支持文本/文件/二进制等通过串口进行通讯

## xDM6.MvcBase.nuget 包

## xDMoCrypto.nuget 包

### xDM.oCrypto2.XCrypto2
一个加密解密工具

## xDM6.System.Management.nuget 包

### xDM.xUtilites.ProcessInfo
获取电脑cpu 内存 进程等信息


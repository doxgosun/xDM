# xDM.xBuild

生成的dll文件都在这里，可以根据需要下载

xDM.dll

xDM.xCommon.dll

xDM.xData.dll

xDM.xData.xClient.dll

xDM.xData.xORM.dll

xDM.xNet.xSockets.dll

xDM.xNet.xSockets.oHPSocket.dll

xDM.xNet.xSockets.xSocket.dll	

xDM.xReflection.dll

dll文件依赖规则：

1、全部x开头可以单独引用，有o开头为依赖其他库，引用时要把依赖库复制至此dll同一目录下

2、除有o开头的dll外，大的命名空间已包含此命名空间下的所有命名空间，dll不是同时引用，如xDM.xData.dll和xDM.xData.xClient.dll不可同时引用，因为xDM.xData.dll已经包含xDM.xData.xClient.dll的所有代码


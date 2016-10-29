#xDM.xCommon

 一个通用类库，集成了许多方便的小函数及扩展程序

使用方法：引用 xDM.xCommon.dll

using xDM.xCommon;

如需要扩展，则
using xDM.xCommon.xExtensions;

 比如：

 var name = "你大爷";

 var pinyin = name.ToPinYin();

 将得到：pinyin = "nidaye";

 又如：

 序列化   obj.Serialzable();  obj.SerialzeToBytes();

 又或者反序列化。。。。



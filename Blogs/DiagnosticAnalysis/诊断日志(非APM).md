## 四种诊断日志记录形式

* Debugger
* TraceSource
* EventSource
* DiagnosticSource

文档汇总：
.NET Core下的日志（2）：日志模型详解 ：https://www.cnblogs.com/artech/p/inside-net-core-logging-2.html
.NET Core的日志[4]:将日志写入EventLog：https://www.cnblogs.com/artech/p/logging-for-net-core-04.html
.NET Core的日志[5]:利用TraceSource写日志：https://www.cnblogs.com/artech/p/logging-for-net-core-05.html
如何利用ETW（Event Tracing for Windows）记录日志：https://www.cnblogs.com/artech/p/logging-via-etw.html

### 1. 静态类型Debugger
静态类型Debugger是.NET Core应用与调试器进行通信的媒介，可以利用它
* 人为启动调试器
* 在某行代码出发断点
* 向调试器发送日志消息

.NET Core采用的JIT Debugging加载调试器的时机：
* 应用程序遇到错误
* 调用某些方法时

一般来说，调试器会集成在开发IDE上
* 如果直接在vs中F5(Start Debugging)模式启动应用时，调试器会直接附加到进程中
* 如果直接ctrl+F5方式，则调试器不会被附加进来
* 即使在调试器没有加载和启动时，也可以利用Debugger的相应方法来启动调试器
```
public static class Debugger
{
    - DefaultCategory
    - IsAttached
    + Launch() : bool
    + Break()
    + IsLogging() : bool
    + Log(int level, string category, string message)
}
```

#### 1.1 Debug
**Debug提供一系列方法标注了ConditionAttribute特性，并将条件编译符设置为Debug => 这些方法的调用只会存在于Debug模式下编译的程序集**
* Write
* WriteLine
* WriteIf
* WriteLineIf

### 2. TraceSource - 观察者模式(发布订阅模式)
* 日志发布者成为TraceSource
* 日志订阅者称为TraceListener
* 消息总是由某一个发布者Source发出，并由一个或多个订阅者Listener收到并消费
* SourceSwitch - 日志过滤；每个TraceSouce都有一个SourceSwitch对应

![TraceSource日志模型完整结构](https://note.youdao.com/yws/api/personal/file/WEB62fd824157b0af1e6e2c2475f2ee65c7?method=download&shareKey=52add0315718a0aa8fc52234278b28cc)

## Datadog.Tracer

### 一、DataDog.Tracer

一组 .NET 库，可用于跟踪 .NET 代码的任何部分。它可以开箱即用地自动检测支持的库，还支持自定义检测来检测您自己的代码。

### 二、配置以支持Send Traces to Datadog
#### 1. 需要做的
* 配置Datadog Agent
* 项目添加Datadog Tracing Library

#### 2. 一言以蔽之：
* 从被Datadog Tracing Library Instrumented的项目生成的Traces发送到Datadog Agent
* 从Datadog Agent获取数据到Datadog backend再呈现到UI界面

### 三、自动跟踪

#### 1. 已支持自动Instrument的类库
[已支持自动Instrument的类库](https://docs.datadoghq.com/tracing/setup_overview/compatibility_requirements/dotnet-core#integrations)

#### 2. 依赖.NET CLR Profiling API
Datadog's automatic instrumentation relies on the .NET CLR Profiling API. This API allows only one subscriber (for example, APM). To ensure maximum visibility, run only one APM solution in your application environment.

#### 3. 步骤(windows,linux,nuget)
**可以看出来三种引入Datadog功能的方式正好印证了.NET Tracer可以在机器维度和应用维度来进行功能引入**

==在这里我们仅以nuget来做研究==
* 引入nuget包Datadog.Monitoring.Distribution
* 设置环境变量以支持自动instrument来attach到你的应用上
* 默认情况下，Datadog Agent监控应用8126端口来接收trace数据
* 配置Datadog Agent？一般来说Docker环境：apm_config/datadog.yaml；基于容器化，serviceless，云等等，那基于公司的Jean如何配置？
* 配置.NET Tracer，优先级应用硬编码 > 环境变量 > 应用配置(app.config,web.config) > datadog.json

### 四、手动跟踪
暂且按下不表


### 五、疑问点
* Datadog.Trace的打包方式
* Demo中Datadog.Trace.ClrProfile.Native.so；CreateLogPath.sh；libddwaf.so的文件从何而来
* Datadog Agent收集Trace的方式和数据格式是否是遵循了OpenTracing，而OpenTelemetry是OpenTracing的进一步，理应能兼容OpenTracing的数据格式
* Where is Datadog Agent Configuration File？[Agent配置文件](https://docs.datadoghq.com/agent/guide/agent-configuration-files/?tab=agentv6v7#agent-main-configuration-file)
* 为什么要修改，修改了哪些东西，解决了什么问题
## 基于Blazor WebAssembly搭建WEB

### 一、项目结构
#### 1. Client
* ET.Client - Client端
* ET.Components - Client端自定义控件
* ET.Pages - Client端所有页面

#### 2. Server
* ET.Server - Server端
* ET.Domain - 业务服务类实现及基础设施接口层
* ET.Domain.Interfaces - 业务服务类接口及实体层
* ET.Infrastructure - 业务基础设施实现层

#### 3. Shared
* EFBServer.Shared - 共有类库

#### 4. Auth
* Microsoft.AspNetCore.Authentication.TongCheng - TC OAuth SDK

#### 5. services - 辅助测试用
* ChatTogether.Hub - Singlar项目，测试用

### 二、Blazor类库选型

**MudBlazor**：[官方地址](https://mudblazor.com/docs/overview)
* 国外开源的Blazor组件库，star不错，还挺活跃
* 布局及各种Flex Breakpoints支持的不错
* 组件丰富，基本满足需要
* 文档齐全

### 三、已集成功能
#### 1. OAuth登录 - 单台可以，多台未测试
* 站点发布到多台服务器的登录问题
单台服务器登录没问题；发布到多台服务器，采用负载均衡策略会导致某台服务器上的站点登陆不成功的问题
**目前采用DataProtection的解决方案，将登录信息key记录到redis里**

#### 2. Redis
* 直接基于StackExchange.Redis创建RedisClientFactory
* 通过RedisClientFactory拿到Redis的IDatabase，直接操作数据库

#### 3. Attribute注册服务
* 通过RegistAsService标签来给各个assembly程序集的类注册服务
* 通过GleanerService标签来注册其他服务

#### 4. SignalR的测试

#### 5. Call JS from .NET

参考文档：[JS的位置](https://docs.microsoft.com/zh-cn/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-6.0#location-of-javascript)

##### 1.1 在<head>标记中加载脚本 - 依赖其他，拖慢速度，不推荐使用
* 在wwwroot/index.html的head元素中加入js；实际项目中不推荐这么用，不做考虑

##### 1.2 在<body>标记中加载脚本 - 页面臃肿，拖慢速度，不考虑使用
* 在wwwroot/index.html的body元素中加入js；加在载入blazor.webassembly.js脚本之后

##### 1.3 在与组件并置的JS文件中加载脚本
* 详见EFServer.Pages的Foos中的示例

##### 1.4 引入外部js的脚本或者外部razor类库的js脚本
* 详见EFServer.Pages的Foos中的示例

##### 1.5. 推荐写法

* 推荐使用引入JS模块的方式加载脚本调用JS函数，这样不会污染JS全局的命名空间
* 需要全局使用的JS函数，可直接在index.html中引入Js脚本

#### 6. hot-reload

* dotnet watch ET.Server项目

可选非必要：
* ET.Server项目配置launchSettings.json加入"hotReloadProfile": "aspnetcore"
* ET.Client项目配置launchSettings.json加入"hotReloadProfile": "blazorwasm"

#### 7. 日志
* 直接基于Serilog集成日志服务

参考文档：
https://cloud.tencent.com/developer/article/1006043
https://cloud.tencent.com/developer/article/1006044

HBase

写在前面
关于Hadoop - 分布式文件系统，用于存储大数据；使用MapReduce来处理；擅长存储各种格式的庞大数据，任意格式甚至非结构化的处理
   弊端：只能执行批量处理，并且只以顺序方式访问数据 -> 意味着即使最简单的搜索工作都要搜索整个数据集
新的解决方案：访问数据中任何点（随机访问）单元：HBase，Cassandra，couchDB，Dynamo，MongoDB

HBase是建立在Hadoop文件系统之上的分布式，面向列的分布式数据库
特点：
极易扩展：基于上层处理能力(RegionServer)的扩展，基于存储(HDFS)的扩展
可快速随机访问海量结构化数据；
提供对数据的随机实时读写访问；
可直接通过HBase存储HDFS数据，或者HDFS上读取消费/随机访问数据
HBase上的数据以StoreFile(HFile)二进制流的形式存储在HDFS上的block块中
HDFS并不知道HBase存储什么，只把存储文件认为是二进制文件，所以HBase的存储数据对于HDFS文件系统时透明的

HBase与Google Bigtable的比较：
HBase是Google Bigtable的开源实现
BigTable利用GFS作为其文件存储系统；HBase利用Hadoop HDFS作为其文件存储系统
BigTable通过运行MapReduce来处理海量数据；HBase利用Hadoop MapReduce来处理海量数据
BigTable通过Chubby作为协同服务；HBase通过Zookeeper作为协同服务
一言以蔽之：Hbase是分布式、面向列的开源数据库（其实准确的说是面向列族）。HDFS为Hbase提供可靠的底层数据存储服务，MapReduce为Hbase提供高性能的计算能力，Zookeeper为Hbase提供稳定服务和Failover机制，因此我们说Hbase是一个通过大量廉价的机器解决海量数据的高速存储和读取的分布式数据库解决方案。

HBase概念辨析
一、Column Family - 列族
HBase通过列族划分数据的存储；列族下面可以包含任意多的列
HBase表的创建的时候就必须指定列族（就像关系型数据库RDBMS创建的时候必须指定具体的列是一样的）
HBase列族不是越多越好，官方推荐列族最好小于或者等于3
二、Rowkey
Rowkey的概念和mysql中的主键是完全一样的，HBase使用Rowkey来唯一区分某一行的数据
HBase支持三种方式的查询：
基于Rowkey的单行查询
基于Rowkey的范围查询
全表扫描
所以Rowkey对HBase的性能影响非常大，设计的时候要兼容基于Rowkey的单行查询也要兼顾全表扫描
三、Region
Region的概念和关系型数据库的分区和分片差不多
HBase会将一个大表的数据基于Rowkey的不同范围分配到不同的Region中
每个Region负责一定范围的数据访问和存储
当Region的某个列族达到一个阈值（默认256M）时就会分成两个新的Region
四、Timestamp
Timestamp是实现HBase多版本的关键，在HBase中使用不同的timestamp来标识相同rowkey行对应的不同版本的数据
写入数据，用户若没有指定对应的timestamp，HBase会自动添加一个，其值跟服务器时间保持一致
HBase中，相同Rowkey的数据按照timestamp倒序排列，默认查询的是最新的版本；用户也可指定timestamp的值来读取旧版本的数据

HBase架构:
![HBase架构](../../../Images/HBase%E6%9E%B6%E6%9E%84.jpg)

一、Client
Client包含访问HBase的接口，维护对应Cache加速对HBase的访问
二、Zookeeper
负责master的高可用、RegionServer的监控、元数据的入口以及集群配置的维护等工作
Zookeeper保证集群中只有一个master在运行，如果master异常，会通过竞争机制产生新的master提供服务
Zookeeper监控RegionServer的状态，当RegionServer有异常的时候，通过回调的形式通知Master RegionServer上下限的消息
Zookeeper存储元数据的统一入口
三、Hmaster - 不干活的主节点
Hmaster为RegionServer分配Region
Hmaster维护整个集群的负载均衡
Hmaster维护集群的元数据信息
Hmaster发现失效的Region时，将失效的Region分配到正常的RegionServer上
Hmaster发现RegionServer失效时，协调对应Hlog的拆分
四、HregionServer - 真正干活的节点
管理master为其分配的Region
处理来自客户端的读写请求
负责和底层HDFS交互，存储数据到HDFS
负责Region变大以后的拆分
负责Storefile的合并工作
五、HDFS
HDFS为HBase提供最终的底层数据存储服务，同时为HBase提供高可用（Hlog存储在HDFS）的支持
提供元数据和表数据的底层分布式存储服务
数据多副本、保证高可靠和高可用性
六、Store
每一个Region由至少一个或多个Store组成，HBase会把一起访问的数据放在一个Store里面：为每个ColumnFamily建一个Store
有几个ColumnFamily就有几个Store
一个Store由一个memStore和零个或者多个StoreFile组成
Store的的大小被HBase用来判断是否需要切分Region
七、MemStore
八、StoreFile
memStore内存中的数据写到文件后就是StoreFile，StoreFile底层是以HFile的格式保存的
九、HFile
十、Hlog
Hlog记录数据的所有变更，可以用来恢复文件；一旦RegionServer宕机，就可以从log中进行恢复

HBase的Region详细介绍
Region类似数据库的分片和分区的概念，每个Region负责一小部分Rowkey范围的数据的读写和维护
Region包含了对应的起始行到结束行的所有信息
master将对应的region分配给不同的regionServer，由RegionServer来提供Region的读写服务和相关的管理工作
一、Region的寻址
RegionServer为一定数量的Region服务，那么Client要对某一行数据做读写时，如何能及知道具体要去访问哪个RegionServer呢
1. 老的Region寻址方式
    不谈
2. 新的Region寻址方式
![新的Region寻址方式](../../../Images/HBase%E6%96%B0Region%E5%AF%BB%E5%9D%80%E6%96%B9%E5%BC%8F.jpg)

    HBase 0.96版本之前有两个特殊的表：-ROOT-表 和 .META表，之后就剩.META表
    a. Client请求ZK获取.META所在的RegsionServer的地址
    b. Client请求.META所在的RegionServer获取访问数据所在的RegionServer地址，Client会将.META的相关信息cache下来，以便下一次快速访问
    c. Client请求数据所在的RegionServer，获取所需要的数据
1. 总结去掉-ROOT-表的原因：
    a. 提高性能
    b. 2层结构已经足以满足集群的要求
2. 缓存问题
    Client会缓存.META的数据，用来加快访问；但是当.META更新了怎么办？
    a. 当region不在某个regionserver上，client的缓存并没有更新，而.META的缓存更新了；client会根据自己的缓存去访问，当然会出错，但是当异常达到充实次数后就会去访问.META表获取最新的数据。
    b. 当.META表所在的regionserver也变了呢？那么client会最终重试之后，到.META所在的regionserver上去获取.META的数据，再根据这个数据去获取具体的数据

HBase写逻辑
HBase写逻辑涉及到写内存(memStore)，写log(Hlog)，刷盘(memStore持久化 -> storeFile（HFile）)
一、HBase写入逻辑：https://blog-10039692.file.myqcloud.com/1506396036453_6524_1506396038477.png
1. Client获取数据需要写入的region所在的regionserver
2. 请求写Hlog
3. 请求写MemStore
4. 当且仅有写Hlog和MemStore都成功了才算请求写入完成，MemStore后续会逐渐刷到HDFS中
Hlog存储在HDFS，当RegionServer发生异常，需要Hlog来恢复数据
二、MemStore刷盘
为了提高Hbase的写入性能，当写请求写入MemStore后，不会立即刷盘；而是等到一定的时候进行刷盘操作，下面时会触发刷盘操作的场景：
1. 全局内存控制
    memStore占据整个heap的最大比例时，会触发刷盘操作 -> 参数：hbase.regionserver.global.memstore.upperLimit，默认为整个heap内存的40%
    这并不意味着全局内存控制触发的刷盘操作会将所有的memStore都进行刷盘，而是通过另一个参数 -> hbase.regionserver.global.memstore.loverLimit来控制，默认为整个heap内存的30%，当flush到所有memStore占整个heap内存的比率为35%的时候，就停止刷盘，这样减少刷盘对业务带来的影响，实现平滑系统负载的目的
2. MemStore达到上限
    当MemStore的大小达到设置值时会触发刷盘，默认128M大小 -> 参数：hbase.hregion.memstore.flush.size
3. RegionServer的Hlog数量达到上限
    前面说到：有且仅有写Hlo和MemStore都成功了才算请求写入完成 -> 如果Hlog太多的话，会导致故障回复的时间太长（Hlog可以在regionServer故障时恢复数据）,因此HBase会对Hlog有最大个数的限制，默认是32个 -> 参数：hbase.regionserver.max.logs（因为Hlog是滚动的，滚动周期是period参数决定的）
    所以 当Hlog达到最大个数时，会强制刷盘
4. 手工触发
    通过hbase shell或者api手工触发flush的操作
5. 关闭RegionSever触发
    正常关闭RegionSever会触发刷盘操作，全部刷盘结束后再启动RegionSever就不需要通过Hlog来恢复数据
6. Region使用HLOG恢复完数据后触发
    当RegionSever出现故障时，上面的Region会迁移到其他正常的RegionSever上，在恢复完Region的数据后，会触发刷盘，完成后才会提供给业务访问
三、Hlog
Hlog是HBase实现WAL（write ahead log）方式产生的日志信息，每部是一个简单的顺序日志
每个RegionServer对应一个Hlog（可以开启MultiWAL功能，开启多个Hlog），所以对RegionSever的写入都被记录到Hlog中
为了保证RegionServer出现问题后数据恢复的效率，HBase会限制最大保存的Hlog数量 -> hbase.regionsever.max.logs
(这边可能会有理解上的困难，说好了每个RegionSever对应一个Hlog，现在又说超过了最大Hlog数量？Hlog会有一个period周期，当超过了这个周期，Hbase就会创建一个新的Hlog文件，这就是Hlog的滚动，但是在同一时间，RegionSever只对应一个Hlog)
超过最大保存的Hlog数量时，会触发强制刷盘操作，其对应的Hlog会有一个过期的概念，过期后，会被监控线程移动到.oldlogs，然后被自动删除掉
1. 单个Region在Hlog中按照时间顺序存储的，但是多个Region可能并不是安全按照时间顺序
2. 每个Hlog最小单元由Hlogkey和WALEdit两部分组成
3. Hlogkey由sequenceid，timestamp等等组成；WALEdit由一些列key/value组成，是对一行上所有列（所有KeyValue）的更新操作，主要是为了实现写入一行多列的原子性
Memstore在达到一定的条件会触发刷盘的操作，刷盘的时候会获取刷新到最新的一个sequenceid的下一个sequenceid，并将新的sequenceid赋给oldestUnflushedSequenceId，并刷到Ffile中
Hlog文件对应所有Region的store中最大的sequenceid如果已经刷盘，就认为Hlog文件已经过期，就会移动到.oldlogs，等待被移除
当RegionServer出现故障的时候，需要对Hlog进行回放来恢复数据。回放的时候会读取Hfile的oldestUnflushedSequenceId中的sequenceid和Hlog中的sequenceid进行比较，小于sequenceid的就直接忽略，但与或者等于的就进行重做。回放完成后，就完成了数据的恢复工作
4. Hlog的生命周期
    Hlog从产生到最后删除需要经历如下过程：
产生
    所有涉及到数据的变更都会先写Hlog，除非你关闭了Hlog
滚动
    Hlog的大小通过周期period控制，默认一个小时，时间达到了HBase就会创建一个新的Hlog文件，这就是滚动
    滚动的目的是为了控制单个Hlog文件过大的情况，方便后续的过期和删除
    控制参数：hbase.regionserver.logroll.period
过期
    Hlog过期依赖对sequenceid的判断
    HBase会将Hlog的sequenceid和Hfile最大的sequenceid进行比较，如果前者比后者小，就说明Hlog过期了；过期了之后Hlog就会被移动到.oldlogs目录
删除
    显示zookeeper上的Hlog节点被删除，然后HBase会每隔habase.master.cleaner.interval(默认60s)时间回去检查.oldlogs目录下的所有Hlog，确认对应的Zookeeper的Hlog节点是否被删除；
    如果zk上不存在对应的Hlog节点，自然。oldlogs中的Hlog会被直接删除
    hbase.master.logcleaner.ttl（默认10分钟） -> 这个参数设置Hlog在.oldlogs目录保留的最长时间

介绍完MemStore和Hlog之后，上述所说的刷盘都是针对的 MemStore 的，记住！！！！！！

RegionServer的故障恢复
RegionServer通过Socket和Zookeeper建立session会话，前者周期性向zk发送ping消息包，zk收到ping包后，则会更新对应的session超时时间
恢复过程：RegionSever宕机 -> zk检测到RegionSever异常 -> Master启动数据恢复 -> Hlog切分 -> Region重新分配 -> Hlog重放 -> 恢复完成并提供服务
故障恢复三种模式：LogSplitting / Distributed Log Splitting / Distributed Log Replay

Region拆分
三种默认拆分策略：ConstantSizeRegionSplitPolicy / IncreasingToUppperBoundRegionSplitPolicy / SteppingSplitPolicy

Region合并 - 小合并和大合并












































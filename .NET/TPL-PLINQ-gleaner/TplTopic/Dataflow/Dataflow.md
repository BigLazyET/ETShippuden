### 数据流-Dataflow

**数据流模型是通过向粗粒度的数据流和管道任务提供进程内消息传递来促进基于角色的编程**

#### 一、技术背景 - 为什么需要它？

* 多个操作异步相互沟通
* 在数据可用时才对其处理（TODO：跟channel的异同？）
* 适用于处理高吞吐量，低滞后时间的而占用大量CPU时间和IO操作的程序
* 有效避免使用回调+同步对象(锁)来实现对统一资源的同步或者独占访问

#### 二、数据流块

##### 1. 源块、目标块和传播块

* 源块作为数据源，可读取 - ISourceBlock\<TOutput>
* 目标块作为数据接收方，可写入 - ITargetBlock\<TInput>
* 传播块作为源块和目标块，可读取和写入 - PropagatorBlock<TInput,TOutput>

##### 2. 管道和网络 - 如何链接彼此

可以通过连接各个数据流块来形成管道或网络，数据可从源到目标传播数据

* SourceBlock\<TOutput>.LinkTo方法将源数据流块链接到目标块
* 源可以链接到零个或多个目标
* 目标可以从零个或多个源进行链接
* 可以向既定的管道或网络中增加或者移除数据流块；预定义的数据流块会自行处理所有建立和移除链接的线程安全性
* 目标流块可以根据源传过来的值进行接受或者拒绝的反馈 - 筛选机制
* 一般来说，源连接到多个目标块，目标块拒绝消息时，源将向下一个目标提供该消息；一般来说，一个目标接受消息，源会停止向其他目标提供此消息，除非源时BroadcastBlock类型

##### 3. 消息传递 - 链接之后消息如何发送和接收

**不要被下面的概念限定死：ActionBlock只能充当目标块，但是需要它发送Post方法，然后自身获取数据执行用户的委托**

一般来说：

###### 3.1 发送和接收1

* 源向目标流块发送消息：Post-同步运行；SendAsync-异步运行
* 目标接收源流块的消息：Receive、ReceiveAsync、TryReceive

###### 3.2 发送和接收2

* 源向目标流块发送消息：OfferMessage
* 目标回应消息可以有三种情况：
  * 接收消息 - OfferMessage返回Accepted
  * 拒绝消息 - OfferMessage返回Declined；当目标不再接收来自源的任何消息时，返回DecliningPermanently
  * 推迟消息 - 目标推迟消息以备日后使用，OfferMessage返回Postponed，目标块会调用ReserveMessage对消息进行暂存，但该消息仍然被源拥有并会发给其他的目标流块；一旦消息被其他目标流块接收，则此目标流块再次调用ConsumeMessage尝试消费暂存的消息就会为null
  * 推迟消息 - 如果源已经暂存了，目标流块稍后需要消息，可以调用ConsumeMessage来尝试消费暂存的消息；如果不需要消息，可以调用ReleaseReservation方法释放暂存消息

##### 4. 数据流块的完成 - 很重要！！！

**源Post数据没有被receive，则源永远没有complete；即使调用了complete()方法只是表示即将完成，发出通知而已，但并不能作为已经完成的标志**

数据流块有完成的概念，其内部提供了一个Task对象 - Completion属性，用来代表完成任务

数据流块提供：
* Task类型的Completion属性 - 完成任务
* Complete方法 - 可用于向其他数据流块发送完成请求的通知

根据Task完成的原理，可以针对数据流块的Completion属性做如下操作，来确保数据流块的完成，以及在完成之后做自定义的后续操作

* Completion.Wait()
* Completion.ContinueWith(Action\<task>)
  * 调用了Complete()方法，正常执行结束，会走到ContinueWith里
  * 通过CancellationToken取消了数据流块，也会走到ContinueWith里

#### 三、预定义数据流块类型

**分为三个类型：缓冲块、执行块、分组块**

* 缓冲块用于存放数据供数据使用者使用
* 执行块为每条接收数据调用用户提供的委托
* 执行块可以设置**并行度**，默认情况下，这些类以接收消息的顺序来处理消息，一次执行处理一条数据；指定并行度之后，一次可以处理多条数据！
* 分组块在各种约束下合并一个或多个源的数据

##### 1. 缓冲块 - BufferBlock

**类定义：决定它能充当的角色：源块、目标块和传播块**
```
public sealed class BufferBlock<T> : System.Threading.Tasks.Dataflow.IPropagatorBlock<T,T>, 
System.Threading.Tasks.Dataflow.IReceivableSourceBlock<T>, 
System.Threading.Tasks.Dataflow.ISourceBlock<T>, 
System.Threading.Tasks.Dataflow.ITargetBlock<T>
```

* 存储的先进先出FIFO的消息队列
* 可以多个源写入
* 可以多个目标读取；但只有一个目标接收每条消息，并不是所有目标都能接收每条消息
* 读一次消息就少一个

##### 2. 缓冲块 - BroadcastBlock

**类定义：决定它能充当的角色：源块、目标块和传播块**
```
public sealed class BroadcastBlock<T> : System.Threading.Tasks.Dataflow.IPropagatorBlock<T,T>, 
System.Threading.Tasks.Dataflow.IReceivableSourceBlock<T>, 
System.Threading.Tasks.Dataflow.ISourceBlock<T>, 
System.Threading.Tasks.Dataflow.ITargetBlock<T>
```

* 多条消息传递给另一个组件，且组件只需要最新的值
* 向多个组件广播消息
* 值被读取后消息并不会被移除，依然存在于BroadcastBlock源里；直到你改变BroadcastBlock的值为止
* 可以给BroadcastBlock发多个消息，但是只会保存最后一个消息
* 每次接收的值只是BroadcastBlock发出消息的拷贝副本

##### 3. 缓冲块 - WriteOnceBlock

**类定义：决定它能充当的角色：源块、目标块和传播块**
```
public sealed class WriteOnceBlock<T> : System.Threading.Tasks.Dataflow.IPropagatorBlock<T,T>, 
System.Threading.Tasks.Dataflow.IReceivableSourceBlock<T>, 
System.Threading.Tasks.Dataflow.ISourceBlock<T>, 
System.Threading.Tasks.Dataflow.ITargetBlock<T>
```

* 与BroadcastBlock基本一致，不同的是，WriteOnceBlock只能被写入一次，且消息被目标流块接收后不会被删除
* 每次接收的值只是BroadcastBlock发出消息的拷贝副本

##### 4. 执行块 - ActionBlock

**类定义：决定它能充当的角色：目标块**
```
public sealed class ActionBlock<TInput> : System.Threading.Tasks.Dataflow.ITargetBlock<TInput>
```

* 默认情况下，ActionBlock会FIFO的处理每一个数据，而且一次只能处理一个数据，一个处理完了再处理第二个，但也可以通过配置来并行的执行多个数据
* 在接收数据的时候调用委托的目标块，可以认为其是数据可用时的异步运行的委托
* 委托类型可以是Action\<T>或者Func<T,Task>
* 前者当接收到数据之后所有的委托返回时即代表已完成；后者则是委托Task执行结束后才代表已完成

##### 5. 执行块 - TransformBlock

**类定义：决定它能充当的角色：源块、目标块和传播块**
```
public sealed class TransformBlock<TInput,TOutput> : System.Threading.Tasks.Dataflow.IPropagatorBlock<TInput,TOutput>, 
System.Threading.Tasks.Dataflow.IReceivableSourceBlock<TOutput>, 
System.Threading.Tasks.Dataflow.ISourceBlock<TOutput>, 
System.Threading.Tasks.Dataflow.ITargetBlock<TInput>
```

* 与ActionBlock基本一致，包括完成的状态的判断(**输入元素被处理然后被receive才算整个完整的完成**)
* 不同的是，TransformBlock<TInput,TOutput>可以充当源块和目标块，从类定义可以看出；但是ActionBlock只能充当目标块的角色
* 为每一个输入值仅生成一个输出值

##### 6. 执行块 - TransformManyBlock

**类定义：决定它能充当的角色：源块、目标块和传播块**
```
public sealed class TransformManyBlock<TInput,TOutput> : System.Threading.Tasks.Dataflow.IPropagatorBlock<TInput,TOutput>, 
System.Threading.Tasks.Dataflow.IReceivableSourceBlock<TOutput>, 
System.Threading.Tasks.Dataflow.ISourceBlock<TOutput>, 
System.Threading.Tasks.Dataflow.ITargetBlock<TInput>
```

* 为每个输入值生成零个或多个输出值

##### 7. 分组块 - BatchBlock

**类定义：决定它能充当的角色：源块、目标块和传播块**
```
public sealed class BatchBlock<T> : System.Threading.Tasks.Dataflow.IPropagatorBlock<T,T[]>, 
System.Threading.Tasks.Dataflow.IReceivableSourceBlock<T[]>, 
System.Threading.Tasks.Dataflow.ISourceBlock<T[]>, 
System.Threading.Tasks.Dataflow.ITargetBlock<T>
```
* 将一系列输入数据合并到输出数据数组
* 创建BatchBlock对象时可以指定批大小
* 默认情况下（贪婪模式），当有持续输入数据时，BatchBlock会接收到指定批大小的数据，再传播出去
* 当调用Complete()方法后，此时无论接收到的数据数量是否已到达批大小，都会传播剩余的这些所有消息形成最终的数组
* 当不调用Complete()方法，即使默认贪婪模式下，都Receive不到任何消息，一定要等收到的数据数量到达批大小才行！
* 贪婪模式：有数据输入，就接收数据，直到接收的数据到达指定批大小再输出传播
* 非贪婪模式：推迟所有源传入的数据，直到有足够的源给块提供消息来形成批，才接收消息并传播输出
* 可以通过BatchBlock的dataflowBlockOptions参数的Greedy属性来指定是否贪婪，true-贪婪（默认），false-非贪婪

##### 8. 分组块 - JoinBlock

**类定义：决定它能充当的角色：源块**
```
public sealed class JoinBlock<T1,T2> : System.Threading.Tasks.Dataflow.IReceivableSourceBlock<Tuple<T1,T2>>, 
System.Threading.Tasks.Dataflow.ISourceBlock<Tuple<T1,T2>>

public sealed class JoinBlock<T1,T2,T3> : System.Threading.Tasks.Dataflow.IReceivableSourceBlock<Tuple<T1,T2,T3>>, 
System.Threading.Tasks.Dataflow.ISourceBlock<Tuple<T1,T2,T3>>
```

* 接收输入元素并传播包含这些元素的元组数据
* JoinBlock<T1,T2> 传播 Tuple<T1,T2>；JoinBlock<T1,T2,T3> 传播 Tuple<T1,T2,T3>；
* 贪婪与非贪婪模式与BatchBlock一致

##### 9. 分组块 - BatchedJoinBlock

* BatchBlock和JoinBlock的合成体
* 貌似无法设置其为非贪婪模式，TODO待验证

#### 四、指定任务计划程序 - TaskScheduler

**例子可参照BroadcastBlockWinform项目**

* TaskScheduler.FromCurrentSynchronizationContext() - 创建一个在当前上下文中执行的TaskScheduler，对于UI Thread操作很有用
* var taskSchedulerPair = new ConcurrentExclusiveSchedulerPair();
* taskSchedulerPair.ConcurrentScheduler - 并发
* taskSchedulerPair.ExclusiveScheduler - 独占

#### 五、并行度

* 默认执行块ExecutionBlocks一次只接收处理一条消息，并且接收的顺序与消息生产的顺序一致
* 为了让执行块可以并发处理多条消息，可以通过设置ExecutionDataflowBlockOptions.MaxDegreeOfParallelism来构建执行块对象
* 默认MaxDegreeOfParallelism为1，执行块处理并发度<=MaxDegreeOfParallelism设定的值，因为可能极低的并行度就能满足需求不需要额外的并行或者计算机核心数资源的原因等等
* 每个执行块设置的MaxDegreeOfParallelism是独立的：意思是四个执行块都设置自己的MaxDegreeOfParallelism为1，这四个执行块都并发同步执行

#### 六、取消链接数据流块

**LinkTo方法有一个重载，包含MaxMessages属性的DataflowLinkOptions对象，当设置MaxMessages为1时，代表 源块在目标收到源块的一条消息后取消与目标的链接！**

#### 七、取消数据流块

**详细请参照 DataflowCancellationWinform 项目** 

#### 八、参考文档

* [TPL Dataflow初探1](https://www.cnblogs.com/haoxinyue/archive/2013/03/01/2938959.html)
* [TPL DataFlow初探2](https://www.cnblogs.com/haoxinyue/archive/2013/03/01/2938953.html)
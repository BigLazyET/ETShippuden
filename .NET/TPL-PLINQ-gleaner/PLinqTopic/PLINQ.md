### TPL-PLINQ

#### 一、PLINQ基本认识
PLINQ，又称并行LINQ，是LINQ(语言集成查询)模式的并行实现

#### 1. PLINQ与LINQ的异同
* 都可以对内存中的IEnumerable或IEnumerable\<T>数据源执行操作
* 都**推迟了执行(延迟执行，或称为延迟加载)**，在枚举查询前不会开始执行
```
public static void DogsFeed()
{
    Console.WriteLine("Dinner Time!");

    // 起始脏兮兮的狗列表中只有A
    var dirtyDogs = new List<string>();
    dirtyDogs.Add("dirtyDogA");
    // 针对列表进行LINQ查询操作（将脏兮兮的狗洗干净）
    var cleanDogs = from dog in dirtyDogs select dog.Replace("dirty","clean");
    // 此时再对脏兮兮的狗列表加入B    
    dirtyDogs.Add("dirtyDogB");
    // 最后对LINQ查询的结果进行遍历：输出 干净的狗得到食物
    foreach(var cleanDog in cleanDogs)
    {
        Console.WriteLine($"{cleanDog} will get his food"); // 狗B依然得到了食物，且洗干净了
    }

    // 结论：
    // 会发现虽然LINQ查询在前，添加狗B在后，但是最终结果狗B依然洗干净了，得到了食物
    // 这就是LINQ的延迟执行
}
```
* 不同的是，PLINQ可以充分利用计算机所有的处理器，进行并行任务，从而提高处理的速度；但这并不意味着PLINQ的速度就一定比LINQ的速度快（比如当计算机只有一个处理器的时候或者排序查询的时候）

#### 2. PLINQ的认知
* PLINQ并行执行的原理是，将数据源分区成片段（会将源**分区**为可以同时并行操作的任务），然后在各个处理器上针对单独工作线程上的每个片段进行并行查询
* PLINQ是保守的，如果发现并不能完全保证查询的安全性，则还是与LINQ一致保持顺序执行
* 除此之外，PLINQ也不是每次执行都是并行执行的；PLINQ会对执行计划进行比对，在选择比较昂贵的并行算法和成本较低的顺序算法之间，依然会选择顺序算法，**这也是PLINQ默认的执行策略**
* **并行策略**：当你确信并行执行能显著提高性能的时候，使用WithExecutionMode方法指定并行策略是可行的
```
// 强制PLINQ的并行执行模式
// 因为默认PLINQ的执行策略是检查查询的表达式和运算符，依据此分析是顺序执行效率高，还是并行执行更合适
// 但我们可以强行指定并行执行模式，不让PLINQ走默认的执行策略
var dogs = new string[] { "tom", "max", "larry", "simen", "philx", "jerry", "jack", "john", "jorage" };
var parallelQuery = from dog in dogs.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism) where dog.Length > 4 select dog;
parallelQuery.ForAll(d => Console.WriteLine($"dog's name：{d}"));
parallelQuery.ToList().ForEach(d => Console.WriteLine($"the name of dog is：{d}"));
```
* **并行度**：可以指定PLINQ执行的并行度，因为毕竟计算机的处理器是昂贵的，必须给其他进程一定的CPU时间；所以我们可以通过指定WithDegreeOfParallelism来指定使用的处理器的个数

#### 3. AsOrdered、AsUnOrdered和AsSequential运算符
某些特定情况下，PLINQ查询结果必须保留源序列的排序，在这里PLINQ提供了AsOrdered运算符
* AsOrdered和AsSequential是不同的，**前者执行上依然是并行的，但结果是保持着与源序列一样的排序；而后者只是表示在执行上是顺序执行的，而非结果上**
* AsOrdered仍然还是并行处理的，但是由于结果上还要保持与源序列一样的顺序，所以各个并行的结果会被缓冲缓存下来；所以多了这一步操作一般来说会导致AsOrderd比AsUnOrderd更慢，但也不绝对
* 默认情况下，PLINQ是不暂留源序列的顺序。当使用AsOrdered启用顺序暂留后，之后依然可以使用AsUnOrdered来禁用顺序暂留；因为顺序暂留的计算成本可能非常高
* AsSequential指示并行LINQ引擎按顺序执行查询，即不并行执行

#### 4. 合并选项WithMergeOptions
PLINQ查询是对源数据进行分区，再由多个线程去并发处理。但如果需要最后在一个线程中去使用PLINQ查询的结果（比如foreach），则必须将每个线程的结果合并到一个序列中

**PLINQ执行的合并类型依具体查询中的运算符而定！各个模式之间并没有优劣性，需要按照特定的需求和环境自行决定选用哪个模式**
**并不是一定说NotBuffered一定是运行最快的！但是NotBuffered一定是最顺畅出结果的！**

* 默认情况下，大多数运算符进行部分缓冲，然后分批生成结果 - 这个对应着合并选项的 Auto Buffered
* 而类似ForAll运算符，则PLINQ查询不需要缓冲任何结果，直接从所有的线程中输出
* 而像OrderBy这种对结果需要强制施加顺序的运算符则必须缓冲所有线程的结果。所以一般完全缓冲查询可能会运行很长时间才能生成第一个结果
* 如果特定查询无法支持请求执行的合并选项，则会忽略你的合并选项配置！
* 大多数情况下，不需要对PLINQ查询指定合并选项，只有你确定某种合并选项能显著提高性能，才去配置
* ForAll 始终为 NotBuffered；它立即生成元素。 OrderBy 始终为 FullyBuffered；它必须先对整个列表进行排序，再生成元素

**合并选项**

* Not Buffered：每个生成的元素一生成都由各自的线程返回，**类似于“流式传输”输出**；如果有AsOrdered运算符，Not Buffered会暂留元素的顺序
* Auto Buffered：查询将元素收到缓冲区，**定期一次性**将所有缓冲内容生成到**使用线程**中
* Fully Buffered：缓冲整个查询的输出，再生成任何元素

#### 5. ForAll运算符

LINQ和PLINQ的延迟查询会一直延迟到在foreach循环中或者调用ToList，ToArray，ToDictionary方法枚举查询的时候执行

Foreach本身不会并行执行，所以要要求所有并行任务的数据结果必须合并到当前foreach的线程中

而ForAll就不用做最后合并这一步！

#### 6. 并行任务的异常处理

从.NET 2.0开始，标准的.NET做法是使未处理的异常使进程崩溃，并且Tasks遵守此要求（同时仍使异常能够封送至连接点）。如果您确实要禁用此功能，则确实存在\<legacyUnhandledExceptionPolicy />配置设置，该设置可将未处理的异常处理行为还原回1.x行为，其中未处理的异常不会崩溃。

* 并行任务异常，在并行任务外加上try/catch块
* 并行任务中某一个任务异常，需要在每个任务上加上try/catch块；如果不加，单个任务的异常不会冒泡到并行任务整体上去，则程序会立即crash
* 并行任务中每个任务加上try/catch可以及时输出异常错误信息；也可以保存到列表里，然后以AggregateException的异常抛出去，让外层的并行任务的try/catch块捕捉到

#### 7. 并行任务的取消

需要结合第6点的异常处理来看：
* 任务取消一般抛出的OperationCancelException
* 并行任务异常一般抛出AggregateException（注意并行任务中的某个任务异常会直接抛出异常，这个异常不一定是AggregateException，但是可以通过获取插入到自定义封装的AggregateException中）

基于以上的认识：
* PLINQ框架不会将OperationCancelException滚动到AggregateException中，所以catch需要区分异常类型
* 如果一个或多个委托(并行任务中的某些任务)抛出OperationCancelException，则依然抛出的是OperationCancelException
* 但是如果一个委托抛出OperationCancelException，另一个委托抛出另一种类型的异常，则这两个异常都会滚动到AggregateException

#### 自定义分区程序

#### 9. 扩展

##### 9.1 衡量PINQ的性能
* [并发可视化工具](https://docs.microsoft.com/zh-cn/visualstudio/profiling/concurrency-visualizer?view=vs-2019)

##### 9.2 AsOrdered和Orderby
两者不是同一个概念！
* AsOrdered着重于**保持结果的顺序与源序列的顺序一致**
* Orderby着重于根据某个属性或这某些属性集合**自行自定义决定结果的顺序，跟源序列毫无关系**

#### 10. PLINQ注意点

* 不要假定并行的速度总是更快
* 不要写入共享内存位置
* 不要过度并行化：因为对源集合进行分区和工作线程之间的同步也会耗费很多时间；可以用第7点去做衡量
* 不要调用非线程安全方法：比如并行执行filestream写入文件，会导致文件内容丢失，数据损坏甚至异常
* 限制对线程安全方法的调用：同样会影响性能和速度
* 避免不必要的排序操作：排序意味着各个并行执行任务的结果需要缓存，而不是立即输出结果，对性能和速度是有影响的
* 尽量首选ForAll输出而非Foreach；前者不需要将各个并行执行的线程结果合并到同一个线程中去，而是在每个线程上直接输出
* 注意线程的关联；当winform，wpf这些应用中，针对控件的访问只能在主线程上！如果在PLINQ查询中访问控件则会报错
* 不要假定ForEach，For和ForAll迭代始终并行执行！如果你在并行任务中设置，其中一些任务依赖于其他任务的信号量，那么就很可能导致死锁！
  * 因为等待信号量的线程可能与发出信号量的线程在同一个线程上
```
// 官方示例
ManualResetEventSlim mre = new ManualResetEventSlim();
Enumerable.Range(0, Environment.ProcessorCount * 100).AsParallel().ForAll((j) =>
{
    if (j == Environment.ProcessorCount)
    {
        Console.WriteLine("Set on {0} with value of {1}", Thread.CurrentThread.ManagedThreadId, j);
        mre.Set();
    }
    else
    {
        Console.WriteLine("Waiting on {0} with value of {1}", Thread.CurrentThread.ManagedThreadId, j);
        mre.Wait();
    }
}); //deadlocks

在此示例中，一个迭代设置一个事件，而所有的其他迭代则等待该事件。 在设置事件的迭代完成之前，任何等待迭代均无法完成。 但是，在设置事件的迭代有机会执行之前，等待迭代可能会阻止用于执行并行循环的所有线程。 这将导致死锁 – 设置事件的迭代将永不会执行，并且等待迭代将永远不会醒来。
具体而言，并行循环的一个迭代绝不应该等待循环的另一个迭代来继续执行。 如果并行循环决定按相反的顺序安排迭代，则会发生死锁
```

#### 参考文档
* [PLINQ介绍](https://docs.microsoft.com/zh-cn/dotnet/standard/parallel-programming/introduction-to-plinq)
* [LINQ查询之延迟执行](https://blog.csdn.net/w343516704/article/details/72772365)
* [不要假定迭代始终并行运行](https://docs.microsoft.com/zh-cn/dotnet/standard/parallel-programming/potential-pitfalls-with-plinq#dont-assume-that-iterations-of-foreach-for-and-forall-always-execute-in-parallel)
* [异常处理(Stephen Toub)](https://social.msdn.microsoft.com/Forums/en-US/0ef2fc53-0545-4ff5-b8f1-a6fea3c5dedb/task-parallel-library-exception-handling-concept)
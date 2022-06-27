using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelTopic
{
    class Program
    {
        static AutoResetEvent autoReset1 = new AutoResetEvent(false);
        static AutoResetEvent autoReset2 = new AutoResetEvent(false);
        static AutoResetEvent autoReset3 = new AutoResetEvent(false);
        static AutoResetEvent autoReset4 = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine("【Press number to choose：1-Parallel.For，2-Parallel.Foreach，3-Parallel.Invoke，4-Exit】");

            Task.Run(Switch);

            Task.Run(ParallelForDemo);

            Task.Run(ParallelForeachDemo);

            Task.Run(ParallelInvokeDemo);

            autoReset4.WaitOne();
        }

        #region Parallel.For
        /// <summary>
        /// IsCompleted：获取该循环是否已运行完成（即，该循环的所有迭代均已执行，并且该循环没有收到提前结束的请求）。如果该循环已运行完成，则为 true；否则为 false。
        /// LowestBreakIteration：获取从中调用 System.Threading.Tasks.ParallelLoopState.Break 的最低迭代的索引。返回一个表示从中调用 Break 语句的最低迭代的整数。
        /// </summary>
        static void CleanRooms()
        {
            var result = Parallel.For(0, 10, index =>
            {
                Console.WriteLine($"第{index}位人员开始打扫房间-TaskId：{Task.CurrentId}-ThreadId:{Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep((index % 2 == 0 ? 2 : 1) * 1000);
            });
            Console.WriteLine($"Is compeleted：{result.IsCompleted}；lowest break index：{result.LowestBreakIteration}");
        }

        static void StrikeCleanRooms()
        {
            var result = Parallel.For(0, 10, (index, state) =>
            {
                if (index % 3 == 0)
                {
                    Console.WriteLine($"第{index}位人员呼吁罢工-TaskId：{Task.CurrentId}-ThreadId:{Thread.CurrentThread.ManagedThreadId}");
                    state.Break();  // 用此方法来中断迭代，但是并不能中断已经提交的并行任务！
                    // TODO: 可以进一步挖掘state.Stop()的操作！
                }
                else
                {
                    Console.WriteLine($"第{index}位人员开始打扫房间-TaskId：{Task.CurrentId}-ThreadId:{Thread.CurrentThread.ManagedThreadId}");
                }
                Thread.Sleep((index % 2 == 0 ? 2 : 1) * 1000);
            });
            Console.WriteLine($"Is compeleted：{result.IsCompleted}；lowest break index：{result.LowestBreakIteration}");
        }

        /// <summary>
        /// Parallel 设置 CancellationToken 超时或者手动取消，针对的是Parllel整体而言，而非单独的每个并行同步任务
        /// 因此加try/catch需要加在Parllel整体上，而不是内在的每一个单独的并行任务上
        /// </summary>
        static void DealLongTimeCancelCleanRooms()
        {
            var parallelOptions = new ParallelOptions();
            parallelOptions.MaxDegreeOfParallelism = 3;
            parallelOptions.CancellationToken = new CancellationTokenSource(2000).Token;

            try
            {
                var result = Parallel.For(0, 10, parallelOptions, index =>
                {
                    Console.WriteLine($"第{index}位人员开始打扫房间-TaskId：{Task.CurrentId}-ThreadId:{Thread.CurrentThread.ManagedThreadId}");
                    Thread.Sleep((index % 2 == 0 ? 2 : 3) * 1000);
                });

                Console.WriteLine($"Is compeleted：{result.IsCompleted}；lowest break index：{result.LowestBreakIteration}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 并行同步任务，其中一个并行任务出错会导致整个并行任务crash，未来得及运行的任务不再运行
        /// 但是try/catch加在Parllel上并不能catch exception，阻止程序crash；而是需要把try/catch加在每个并行同步任务上
        /// 设计理念：单个并行任务报错时.NET并不吞掉错误，然后冒泡交由最上层的Parllel处理AggregateException；而是及时报错抛出异常，让所有的并行任务不再运行下去；
        /// </summary>
        static void DealException1CleanRooms()
        {
            var result = Parallel.For(0, 10, index =>
            {
                try
                {
                    Console.WriteLine($"第{index}位人员开始打扫房间-TaskId：{Task.CurrentId}-ThreadId:{Thread.CurrentThread.ManagedThreadId}");
                    if (index == 6)
                    {
                        throw new Exception($"第{index}位人员打扫房间出错");
                    }
                    Thread.Sleep((index % 2 == 0 ? 2 : 3) * 1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            Console.WriteLine($"Is compeleted：{result.IsCompleted}；lowest break index：{result.LowestBreakIteration}");
        }

        /// <summary>
        /// 包了两层try/catch
        /// 将每个并行任务可能出现的错误收集，然后在最外层的Parllel里catch到，再解析输出或者记录到日志中
        /// </summary>
        static void DealException2CleanRooms()
        {
            var exceptions = new ConcurrentQueue<Exception>();

            try
            {
                var result = Parallel.For(0, 10, index =>
                {
                    try
                    {
                        Console.WriteLine($"第{index}位人员开始打扫房间-TaskId：{Task.CurrentId}-ThreadId:{Thread.CurrentThread.ManagedThreadId}");

                        if (index % 3 == 0)
                        {
                            throw new Exception($"{index} 是3的倍数");
                        }

                        Thread.Sleep((index % 2 == 0 ? 2 : 3) * 1000);
                    }
                    catch (Exception ex)
                    {
                        exceptions.Enqueue(ex);
                    }
                });
                Console.WriteLine($"Is compeleted：{result.IsCompleted}；lowest break index：{result.LowestBreakIteration}");

                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Parallel.Foreach 基本用法与 Parllel.For类似
        static void Study()
        {
            var subjects = new string[] { "语文", "数学", "英语", "物理", "化学", "生物", "历史", "政治", "地理" };
            var result = Parallel.ForEach(subjects, (value, state, index) =>
            {
                Console.WriteLine($"第{index}位人员开始学习{value}：{Task.CurrentId}-ThreadId:{Thread.CurrentThread.ManagedThreadId}");
                Thread.Sleep((index % 2 == 0 ? 2 : 1) * 1000);
            });
            Console.WriteLine($"Is compeleted：{result.IsCompleted}；lowest break index：{result.LowestBreakIteration}");
        }
        #endregion

        #region Parallel.Invoke
        static void Shopping()
        {
            var products = new string[] { "香蕉", "苹果", "草莓", "黄瓜", "荔枝", "杨梅", "车厘子" };

            var actions = new ConcurrentQueue<Action>();

            //var actions = from pro in products
            //              select (() => { Console.WriteLine($"我要买{pro}"); Thread.Sleep(5000); });

            products.AsParallel().ForAll(p => actions.Enqueue(()=> { Thread.Sleep(2000); Console.WriteLine($"我要买{p}"); Thread.Sleep(3000); }));

            Parallel.Invoke(actions.ToArray());
        }
        #endregion

        #region Parallel Extension
        // about exceptions
        #endregion

        private static void Switch()
        {
            while (true)
            {
                var result = Console.ReadLine();
                Console.WriteLine($"user print {result}");
                if (string.IsNullOrWhiteSpace(result))
                    continue;
                if (result == "1")
                {
                    autoReset1.Set();
                }
                else if (result == "2")
                {
                    autoReset2.Set();
                }
                else if (result == "3")
                {
                    autoReset3.Set();
                }
                else if (result == "4")
                {
                    autoReset4.Set();
                }
            }
        }

        private static void ParallelForDemo()
        {
            while (true)
            {
                autoReset1.WaitOne();

                Console.WriteLine("------Parallel.For：CleanRooms------");
                CleanRooms();
                Console.WriteLine("------Parallel.For：StrikeCleanRooms------");
                StrikeCleanRooms();
                Console.WriteLine("------Parallel.For：DealLongTimeCancelCleanRooms------");
                DealLongTimeCancelCleanRooms();
                Console.WriteLine("------Parallel.For：DealException1CleanRooms------");
                DealException1CleanRooms();
                Console.WriteLine("------Parallel.For：DealException2CleanRooms------");
                DealException2CleanRooms();

                Console.WriteLine("【Press number to choose：1-Parallel.For，2-Parallel.Foreach，3-Parallel.Invoke，4-Exit】");
            }
        }

        private static void ParallelForeachDemo()
        {
            while (true)
            {
                autoReset2.WaitOne();

                Console.WriteLine("------Parallel.Foreach：Study------");
                Study();

                Console.WriteLine("【Press number to choose：1-Parallel.For，2-Parallel.Foreach，3-Parallel.Invoke，4-Exit】");
            }
        }

        private static void ParallelInvokeDemo()
        {
            while (true)
            {
                autoReset3.WaitOne();

                Console.WriteLine("------Parallel.Invoke：Shopping------");
                Shopping();

                Console.WriteLine("【Press number to choose：1-Parallel.For，2-Parallel.Foreach，3-Parallel.Invoke，4-Exit】");
            }
        }
    }
}

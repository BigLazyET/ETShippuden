using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PLinqTopic
{
    public static class DealExceptionDemo
    {
        static ConcurrentQueue<Exception> exceptions = new ConcurrentQueue<Exception>();

        /// <summary>
        /// 并行任务中单个任务异常导致整个并行任务进程奔溃
        /// try/catch并不能抓取到
        /// 查询无法在异常抛出后继续运行。 在应用代码捕获到异常时，PLINQ 已停止对所有线程运行查询
        /// </summary>
        public static void CrashNotKeepOn()
        {
            var flags = Enumerable.Range(1, 10).ToArray();
            flags[5] = 0;

            var parallelQuery = from flag in flags.AsParallel()
                                let field = flag * 10
                                where 50 % field == 0
                                select new { field };

            // 延迟执行
            try
            {
                parallelQuery.ForAll(x => Console.WriteLine($"50取余为0：{x}"));
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    Console.WriteLine($"{ex.GetType()}-{ex.Message}");
                }
            }
        }

        /// <summary>
        /// 异常不会中断整个并行任务
        /// 异常不会冒泡上来，因为我们已经在它抛出的地方处理了
        /// </summary>
        public static void CrashStillKeepOn()
        {
            var flags = Enumerable.Range(1, 10).ToArray();
            flags[5] = 0;

            var parallelQuery = from flag in flags.AsParallel()
                                let field = flag * 10
                                where IsTrue(field)
                                select new { field };

            // 延迟执行
            try
            {
                parallelQuery.ForAll(x => Console.WriteLine($"50整除为1：{x}"));
            }
            // 异常不会冒泡上来，因为我们已经在它抛出的地方处理了
            catch (AggregateException ae)
            {
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    Console.WriteLine($"{ex.GetType()}-{ex.Message}");
                }
            }

            static bool IsTrue(int flag)
            {
                try
                {
                    Console.WriteLine($"flag-{flag}");
                    return 50 / flag == 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"flag-{flag}：{ex.Message}");
                    return false;
                }
                
            }
        }

        public static void ExceptionCollectStillKeepOn()
        {
            var flags = Enumerable.Range(1, 10).ToArray();
            flags[5] = 0;

            var parallelQuery = from flag in flags.AsParallel()
                                let field = flag * 10
                                where IsTrue(field)
                                select new { field };

            // 延迟执行
            try
            {
                parallelQuery.ForAll(x => Console.WriteLine($"50整除为1：{x}"));

                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }
            }
            // 异常不会冒泡上来，因为我们已经在它抛出的地方处理了
            catch (AggregateException ae)
            {
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    Console.WriteLine($"{ex.GetType()}-{ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            static bool IsTrue(int flag)
            {
                try
                {
                    Console.WriteLine($"flag-{flag}");
                    return 50 / flag == 1;
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                    return false;
                }

            }
        }
    }
}

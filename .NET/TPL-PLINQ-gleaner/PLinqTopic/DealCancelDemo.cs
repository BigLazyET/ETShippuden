using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PLinqTopic
{
    public static class DealCancelDemo
    {
        /// <summary>
        /// 取消并行任务
        /// 在源并行任务上加上CancellationToken
        /// </summary>
        public static void ParallelCancel()
        {
            // C# 9.0 support
            //using CancellationTokenSource cts = new();

            var cts = new CancellationTokenSource();

            Task.Factory.StartNew(async() =>
            {
                var random = new Random();
                await Task.Delay(random.Next(100,300));
                cts.Cancel();
            });

            var flags = Enumerable.Range(0, 100000000);

            var result = new List<int>();
            try
            {
                var parallelQuery = from flag in flags.AsParallel().WithCancellation(cts.Token) where flag % 3 == 0 && flag % 7 == 0 orderby flag descending select flag;
                result = parallelQuery.ToList();
            }
            catch (OperationCanceledException oce)
            {
                Console.WriteLine($"OperationCanceledException-{oce}");
            }
            catch (AggregateException ae)
            {
                Console.WriteLine($"AggregateException-{ae}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception-{ex}");
            }

            foreach (var flag in result)
            {
                Console.WriteLine($"flag：{flag}");
            }
        }

        /// <summary>
        /// 取消并行任务中的一个耗时的并行任务
        /// 每个任务都加上CancellationToken，触发其中耗时任务的取消
        /// </summary>
        public static void ParallelCancelCostPlentyTimeTask()
        {
            var cts = new CancellationTokenSource();

            Task.Factory.StartNew(async () =>
            {
                var random = new Random();
                await Task.Delay(random.Next(100, 500));
                Console.WriteLine("press c to cancel");
                if (Console.ReadKey().KeyChar == 'c')
                {
                    cts.Cancel();
                }
            });

            var flags = Enumerable.Range(1, 10000000);

            var result = Array.Empty<double>();
            try
            {
                var parallelQuery = from flag in flags.AsParallel() where flag % 3 == 0 && flag % 7 == 0 orderby flag descending select Function(flag,cts.Token);
                result = parallelQuery.ToArray();

                //result = (from flag in flags.AsParallel() where flag % 3 == 0 && flag % 7 == 0 orderby flag descending select Function(flag, cts.Token)).ToArray();
            }
            catch (OperationCanceledException oce)
            {
                Console.WriteLine($"OperationCanceledException-{oce}");
            }
            catch (AggregateException ae)
            {
                Console.WriteLine($"AggregateException-{ae}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception-{ex}");
            }

            foreach (var flag in result)
            {
                Console.WriteLine($"flag：{flag}");
            }

            static double Function(int flag,CancellationToken ct)
            {
                for (int i = 0; i < 5; i++)
                {
                    // TODO: What does 50000 means？the parameter interation means？
                    Thread.SpinWait(50000);

                    ct.ThrowIfCancellationRequested();
                }

                Console.WriteLine(flag);

                return Math.Sqrt(flag);
            }
        }
    }
}

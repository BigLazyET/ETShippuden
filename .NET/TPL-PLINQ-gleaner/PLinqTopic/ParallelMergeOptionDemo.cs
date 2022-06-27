using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace PLinqTopic
{
    public class ParallelMergeOptionDemo
    {
        static AutoResetEvent resetEvent = new AutoResetEvent(false);

        public static void ChooseMergeMode()
        {
            while (true)
            {
                Console.WriteLine("press key to choose：1-NotBuffered，2-AutoBuffered，3-FullyBuffered");
                var print = Console.ReadLine();
                var mergeOption = ParallelMergeOptions.Default;
                switch (print)
                {
                    case "1":
                        mergeOption = ParallelMergeOptions.NotBuffered;
                        break;
                    case "2":
                        mergeOption = ParallelMergeOptions.AutoBuffered;
                        break;
                    case "3":
                        mergeOption = ParallelMergeOptions.FullyBuffered;
                        break;
                    default:
                        break;
                }
                SwitchParallelMergeOption(mergeOption);
            }
        }

        static void SwitchParallelMergeOption(ParallelMergeOptions mergeOption)
        {
            Console.WriteLine($"the current parallel merge option is: {mergeOption.ToString()}");

            var flags = Enumerable.Range(1, 5000);

            var parallelQuery = from flag in flags.AsParallel().WithMergeOptions(mergeOption) where flag % 2 == 0 select ExpensiveFunc(flag);

            var sw = new Stopwatch();
            sw.Start();
            // 延迟执行，所以只需要记录遍历结果过程的耗时即可
            foreach (var item in parallelQuery)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine($"{mergeOption}合并模式下耗时：{sw.ElapsedMilliseconds}ms");

            static string ExpensiveFunc(int i)
            {
                Thread.SpinWait(2000000);
                return $"{i} ***************************************";
            }
        }
    }
}

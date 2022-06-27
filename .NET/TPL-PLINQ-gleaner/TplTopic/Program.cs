using System;
using System.Linq;
using System.Threading.Tasks;
using TplTopic.Dataflow;
using TplTopic.Dataflow.BufferingBlocks;
using TplTopic.Dataflow.CustomizeDataflows;
using TplTopic.Dataflow.ExecutionBlocks;
using TplTopic.Dataflow.GroupingBlocks;

namespace TplTopic
{
    class Program
    {
        static void Main(string[] args)
        {
            // BufferingBlocks - BufferBlock
            //Console.WriteLine("------BufferingBlocks - BufferBlock------");
            //BufferBlockDemo.ReportNumber();

            // BufferingBlocks - BroadcastBlock
            //Console.WriteLine("------BufferingBlocks - BroadcastBlock------");
            //BroadcastBlockDemo.DinnerTime();

            // BufferingBlocks - WriteOnceBlock
            //Console.WriteLine("------BufferingBlocks - WriteOnceBlock------");
            //WriteOnceBlockDemo.DinnerTime();

            // ExecutionBlocks - ActionBlock
            //Console.WriteLine("------ExecutionBlocks - ActionBlock------");
            //ActionBlockDemo.DinnerTime();
            //ActionBlockDemo.DinnerCompetition();

            // ExecutionBlocks - TransformBlock
            //Console.WriteLine("------ExecutionBlocks - TransformBlock------");
            //TransformBlockDemo.ShowerThenEat();
            //TransformBlockDemo.ShowerThenEatAsync();

            // ExecutionBlocks - TransformManyBlock
            //Console.WriteLine("------ExecutionBlocks - TransformManyBlock------");
            //TransformManyBlockDemo.RadishGame(50);

            // GroupingBlocks - BatchBlock
            //Console.WriteLine("------GroupingBlocks - BatchBlock------");
            //BatchBlockDemo.PackApplesInBox(5);

            // GroupingBlocks - JoinBlock
            //Console.WriteLine("------GroupingBlocks - JoinBlock------");
            //JoinBlockDemo.MathWork();

            //BatchedJoinBlockDemo.Test();

            // 创建数据流管道
            //DataflowPipelineDemo.MakeRandomSandwich();

            // 自定义数据流块类型 - 滑动窗口1 - 采用Encpsulate
            //SlidingWindowDemo.DemonstrateSlidingWindow(SlidingWindowDemo.CreateSlidingWindow<int>(5), Enumerable.Range(0, 111));
            // 自定义数据流块类型 - 滑动窗口2 - 采用Encpsulate
            //SlidingWindowDemo.DemonstrateSlidingWindow(new SlidingWindowDemo.SlidingWindowBlock<int>(5), Enumerable.Range(0, 111));

            // 指定数据流块处理并行度
            //var processCount = Environment.ProcessorCount;  // 处理器个数
            //TimeSpan elapsed;
            //elapsed =  DataflowParallelismDemo.HeavyWork(processCount, 1);
            //Console.WriteLine($"1 parallism of {processCount} works cost time: {elapsed.TotalMilliseconds}");
            //elapsed = DataflowParallelismDemo.HeavyWork(processCount, processCount);
            //Console.WriteLine($"{processCount} parallism of {processCount} works cost time: {elapsed.TotalMilliseconds}");

            // JoinBlock从多个源获取数据，并针对共享源读取相互协调工作
            DataflowJoinBlockDemo.SealPaper();

            Console.ReadLine();
        }
    }
}

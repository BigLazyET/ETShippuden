using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow.ExecutionBlocks
{
    public static class ActionBlockDemo
    {
        public static void DinnerTime()
        {
            var actionBlock = new ActionBlock<int>(x => Console.WriteLine($"dog{x} needs feed"));

            foreach (var item in Enumerable.Range(0,10))
            {
                actionBlock.Post(item);
            }

            actionBlock.Complete();

            actionBlock.Completion.Wait();

            actionBlock.Completion.ContinueWith(task =>
            {
                Console.WriteLine($"task status：{task.Status}");
            });
        }

        public static void DinnerCompetition()
        {
            var actionBlock = new ActionBlock<int>(async x =>
            {
                await Task.Delay(x * 1000);

                Console.WriteLine($"{DateTimeOffset.Now} dog{x} got food");
            },new ExecutionDataflowBlockOptions
            {
                // 如果想并行处理，可以设置ExecutionDataflowBlockOptions类型的参数的MaxDegreeOfParallelism属性
                //MaxDegreeOfParallelism = Environment.ProcessorCount
                MaxDegreeOfParallelism = 1  // 默认值 - 并行度为1
            });

            foreach (var item in Enumerable.Range(0, 10))
            {
                Console.WriteLine($"{DateTimeOffset.Now} post：{item}");
                actionBlock.Post(item);
            }

            Enumerable.Range(10, 10).AsParallel().ForAll(item =>
             {
                 Console.WriteLine($"{DateTimeOffset.Now} post：{item}");
                 actionBlock.Post(item);
             });

            Console.WriteLine($"设置ActionBlock完成的状态通知（其实并没有真正完成），实际还是需要等待每个Task完成之后才是最终的完成状态");
            actionBlock.Complete();

            // 两种方式等待任务的完成
            //actionBlock.Completion.Wait();

            actionBlock.Completion.ContinueWith(task =>
            {
                Console.WriteLine($"{DateTimeOffset.Now} task status：{task.Status}");
            });
        }
    }
}

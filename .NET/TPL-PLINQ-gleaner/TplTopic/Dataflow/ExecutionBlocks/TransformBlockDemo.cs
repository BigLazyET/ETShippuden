using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow.ExecutionBlocks
{
    public static class TransformBlockDemo
    {
        public static void ShowerThenEat()
        {
            var transformBlock = new TransformBlock<int, string>(x =>
             {
                 Console.WriteLine($"dirty dog{x} needs shower");
                 return $"clean dog{x}";
             });

            Enumerable.Range(0, 5).AsParallel().ForAll(x =>
             {
                 transformBlock.Post(x);
             });

            while (!transformBlock.Completion.IsCompleted)
            {
                var dog = transformBlock.Receive();
                Console.WriteLine($"{dog} got food");
            }
        }

        public static void ShowerThenEatAsync()
        {
            var transformBlock = new TransformBlock<int, string>(async x =>
            {
                Console.WriteLine($"{DateTimeOffset.Now} dirty dog{x} needs shower");
                await Task.Delay(x * 1000);
                return $"clean dog{x}";
            });

            // 虽然这个是并行发送Post，但是TransformBlock接收到消息是顺序处理的，并没有并行处理
            // 如果想并行处理，可以设置TransformBlock的ExecutionDataflowBlockOptions类型的参数的MaxDegreeOfParallelism属性
            Enumerable.Range(0, 5).AsParallel().ForAll(x =>
            {
                transformBlock.Post(x);
            });

            while (!transformBlock.Completion.IsCompleted)
            {
                var dog = transformBlock.Receive();
                Console.WriteLine($"{DateTimeOffset.Now} {dog} got food");
            }
        }
    }
}

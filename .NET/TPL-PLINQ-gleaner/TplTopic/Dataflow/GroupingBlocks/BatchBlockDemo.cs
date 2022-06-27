using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow.GroupingBlocks
{
    public static class BatchBlockDemo
    {
        /// <summary>
        /// 十个苹果包装成一箱
        /// Pack ten apples in a box
        /// </summary>
        public static void PackApplesInBox(int batchSize)
        {
            var batchBlock = new BatchBlock<int>(batchSize);

            Enumerable.Range(0, 115).AsParallel().ForAll(flag =>
             {
                 batchBlock.Post(flag);
             });

            batchBlock.Complete();

            while (!batchBlock.Completion.IsCompleted)
            {
                if(batchBlock.TryReceive(out var items))
                    Console.WriteLine($"pack in box：{string.Join(",",items)}");
            }
        }
    }
}

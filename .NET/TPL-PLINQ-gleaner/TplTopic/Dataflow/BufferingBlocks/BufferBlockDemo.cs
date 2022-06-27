using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow.BufferingBlocks
{
    public static class BufferBlockDemo
    {
        public static void ReportNumber()
        {
            var bufferBlock = new BufferBlock<int>();

            Enumerable.Range(0, 5).AsParallel().ForAll(item =>
             {
                 Console.WriteLine($"post：{item}");
                 bufferBlock.Post(item);
             });

            Enumerable.Range(0, 5).ToList().ForEach(item =>
             {
                 Console.WriteLine($"receive：{bufferBlock.Receive()}");
             });
        }
    }
}

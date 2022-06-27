using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow
{
    public static class DataflowParallelismDemo
    {
        public static TimeSpan HeavyWork(int workcount, int parallism)
        {
            var workBlock = new ActionBlock<int>(
                async time => await Task.Delay(time),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = parallism
                });

            var sw = new Stopwatch();
            sw.Start();

            for (int i = 0; i < workcount; i++)
            {
                workBlock.Post(1000);
            }

            workBlock.Complete();
            workBlock.Completion.Wait();
            sw.Stop();

            return sw.Elapsed;
        }
    }
}

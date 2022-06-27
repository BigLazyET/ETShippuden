using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow.BufferingBlocks
{
    public static class BroadcastBlockDemo
    {
        public static void DinnerTime()
        {
            var broadcaster = new BroadcastBlock<string>(null);

            var actionBlocks = Enumerable.Range(0, 5).AsParallel().Select(x =>
            {
                return new ActionBlock<string>(_ =>
                {
                    var message = broadcaster.Receive();
                    Console.WriteLine($"dog{x} got the message：{message}");
                });
            });

            Task.Run(async() =>
            {
                var dinnerStart = "Dinner Time！You have 10 seconds to eat";
                broadcaster.Post(dinnerStart);
                actionBlocks.ForAll(x => x.Post(null));

                await Task.Delay(10000);

                var dinnerEnd = "Dinner Finished！";
                broadcaster.Post(dinnerEnd);
                actionBlocks.ForAll(x => x.Post(null));
            });
        }
    }
}

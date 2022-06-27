using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow.BufferingBlocks
{
    public static class WriteOnceBlockDemo
    {
        public static void DinnerTime()
        {
            var writeOnceBlock = new WriteOnceBlock<string>(null);

            var actionBlocks = Enumerable.Range(0, 5).AsParallel().Select(x =>
            {
                return new ActionBlock<string>(_ =>
                {
                    var message = writeOnceBlock.Receive();
                    Console.WriteLine($"dog{x} got the message：{message}");
                });
            });

            Task.Run(async () =>
            {
                var dinnerStart = "Dinner Time！You have 10 seconds to eat";
                writeOnceBlock.Post(dinnerStart);
                actionBlocks.ForAll(x => x.Post(null));

                await Task.Delay(10000);

                // 虽然再次发送了 Dinner Finished的消息，但是由于WriteOnceBlock的特性，此后的消息不再发送，自动丢弃 => 所以再次获取的消息还是第一次的消息
                // WriteOnceBlock<T>对象仅能被写入一次
                var dinnerEnd = "Dinner Finished！";
                writeOnceBlock.Post(dinnerEnd);
                actionBlocks.ForAll(x => x.Post(null));
            });
        }
    }
}

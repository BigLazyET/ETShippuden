using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow
{
    public static class DataflowPipelineDemo
    {
        public static void MakeRandomSandwich()
        {
            // 仓库输出材料
            var storeHouseJoinBlock = new JoinBlock<string, string>();

            new string[] { "鸭蛋", "鸡蛋", "鹅蛋" }.AsParallel().ForAll(egg =>
            {
                Console.WriteLine($"{DateTimeOffset.Now} 从仓库拿走{egg}");
                storeHouseJoinBlock.Target1.Post(egg);
            });

            new string[] { "生菜", "番茄", "黄瓜" }.AsParallel().ForAll(veg =>
            {
                Console.WriteLine($"{DateTimeOffset.Now} 从仓库拿走{veg}");
                storeHouseJoinBlock.Target2.Post(veg);
            });

            // 处理材料
            var materialTransformBlock = new TransformManyBlock<Tuple<string, string>, string>(mats =>
              {
                  var egg = mats.Item1;
                  var veg = mats.Item2;

                  Console.WriteLine($"{DateTimeOffset.Now} 处理材料：{egg}和{veg}");

                  return new string[] { egg, veg };
              });

            // 收集各路材料集全
            var packBufferBlock = new BatchBlock<string>(1, new GroupingDataflowBlockOptions { Greedy = true });    // 设置为1的情况与设置为>1的情况，针对目标源接收的数据内容的多少不同
            //var packBufferBlock = new BatchBlock<string>(2, new GroupingDataflowBlockOptions { Greedy = true });  // >= 2 的情况，在这个场景下时一致的，取决于这个源的源JoinBlock有多少个target
            //var packBufferBlock = new BatchBlock<string>(2, new GroupingDataflowBlockOptions { Greedy = true });
            //var packBufferBlock = new BatchBlock<string>(2, new GroupingDataflowBlockOptions { Greedy = false });    // 非贪婪模式下，即使Complete()了也会卡住，必须要等待足够5个才行

            // 将所有材料给厨师
            var hand2ChiefBufferBlock = new BufferBlock<string[]>();

            // 厨师制作三明治
            var actionBlock = new ActionBlock<string[]>(async materials =>
            {
                await Task.Delay(1000);
                Console.WriteLine($"{DateTimeOffset.Now} make sandwich with {string.Join(",", materials)}");
            });

            var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };

            storeHouseJoinBlock.LinkTo(materialTransformBlock, linkOptions);
            materialTransformBlock.LinkTo(packBufferBlock, linkOptions);
            packBufferBlock.LinkTo(hand2ChiefBufferBlock, linkOptions);
            hand2ChiefBufferBlock.LinkTo(actionBlock, linkOptions);

            storeHouseJoinBlock.Complete();

            actionBlock.Completion.ContinueWith(x =>
            {
                Console.WriteLine($"{DateTimeOffset.Now} storeHouseJoinBlock status: {storeHouseJoinBlock.Completion.Status}");
                Console.WriteLine($"{DateTimeOffset.Now} materialTransformBlock status: {materialTransformBlock.Completion.Status}");
                Console.WriteLine($"{DateTimeOffset.Now} packBufferBlock status: {packBufferBlock.Completion.Status}");
                Console.WriteLine($"{DateTimeOffset.Now} deliveryBufferBlock status: {hand2ChiefBufferBlock.Completion.Status}");
                Console.WriteLine($"{DateTimeOffset.Now} actionBlock status: {actionBlock.Completion.Status}");
                Console.WriteLine($"{DateTimeOffset.Now} work done, great work！");
            });
        }
    }
}

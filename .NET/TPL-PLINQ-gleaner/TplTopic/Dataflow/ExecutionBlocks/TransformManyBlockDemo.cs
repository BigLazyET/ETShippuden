using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow.ExecutionBlocks
{
    public static class TransformManyBlockDemo
    {
        public static void RadishGame(int round)
        {
            if (round < 0)
                return;

            var radishs = new string[] { "红萝卜", "橙萝卜", "黄萝卜", "绿萝卜", "青萝卜", "蓝萝卜", "紫萝卜", "白萝卜" };

            Console.WriteLine($"源：{string.Join(",", radishs)}");

            var actionBlock = new ActionBlock<string>(message =>
            {
                Console.WriteLine(message);
            });

            var transformManyBlock = new TransformManyBlock<int, string>(index =>
             {
                 var i = index % 7;
                 var radish = radishs[i];
                 var nextRadis = radishs[i + 1];

                 return new string[] { $"{radish}蹲{radish}蹲", $"{radish}蹲完{nextRadis}蹲" };
             });

            transformManyBlock.LinkTo(actionBlock);

            for (int i = 0; i < round; i++)
            {
                transformManyBlock.Post(i);
            }

            Task.Run(async () =>
            {
                // 上面都执行结束了，但是针对block而言并没有complete，他们的状态依旧保持着WaitingForActivation，随时可以进行post、recevie等！
                await Task.Delay(10000);
                Console.WriteLine("------print block status------");
                Console.WriteLine($"transformManyBlock complete status：{transformManyBlock.Completion.Status}");
                Console.WriteLine($"actionBlock complete status：{actionBlock.Completion.Status}");
            });
        }
    }
}

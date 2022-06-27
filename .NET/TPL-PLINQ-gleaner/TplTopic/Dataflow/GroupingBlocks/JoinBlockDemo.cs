using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow.GroupingBlocks
{
    public static class JoinBlockDemo
    {
        public static void MathWork()
        {
            var joinBlock = new JoinBlock<int, int, string>();

            Enumerable.Range(0, 10).AsParallel().ForAll(x =>
             {
                 joinBlock.Target1.Post(x);
             });

            Enumerable.Range(100, 10).AsParallel().ForAll(y =>
             {
                 joinBlock.Target2.Post(y);
             });

            new string[] { "+", "-", "*", "/", "%" }.AsParallel().ForAll(o =>
            {
                joinBlock.Target3.Post(o);
            });

            joinBlock.Complete();

            joinBlock.Completion.ContinueWith(task =>
            {
                Console.WriteLine("除非你调用了Complete()方法，否则一般来说都看不到我");
            });

            while (!joinBlock.Completion.IsCompleted)
            {
                if (joinBlock.TryReceive(out var data))
                {
                    var op = data.Item3;

                    var result = op switch
                    {
                        "+" => data.Item1 + data.Item2,
                        "-" => data.Item1 - data.Item2,
                        "*" => data.Item1 * data.Item2,
                        "/" => data.Item1 / data.Item2,
                        "%" => data.Item1 % data.Item2,
                        _ => throw new NotImplementedException()
                    };

                    Console.WriteLine($"{data.Item1} {data.Item3} {data.Item2}：{result}");
                }
            }

            Console.WriteLine("Math Work End");
        }
    }
}

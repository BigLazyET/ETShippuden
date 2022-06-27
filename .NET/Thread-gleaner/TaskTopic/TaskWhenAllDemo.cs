using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTopic.Extensions;

namespace TaskTopic
{
    public static class TaskWhenAllDemo
    {
        public static async Task WhenAllClearCompelete()
        {
            var cleanTasks = Enumerable.Range(1, 10).Select(async index =>
            {
                var cost = new Random().Next(1, 10);

                Console.WriteLine($"{index}号开始打扫");
                await Task.Delay(cost * 1000);
                Console.WriteLine($"{index}号打扫完毕");
            });
            await Task.WhenAll(cleanTasks);
        }

        public static async Task WhenAllClearPartitionCompelete()
        {
            await Enumerable.Range(1, 20).ForEachAsync(4, async index =>
            {
                var cost = new Random().Next(1, 10);

                Console.WriteLine($"{index}号开始打扫");
                await Task.Delay(cost * 1000);
                Console.WriteLine($"{index}号打扫完毕");
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventWaitHandleTopic
{
    public static class ManualResetEventSlimDemo
    {
        /// <summary>
        /// ManualResetEventSlim使用上与ManualResetEvent差不多
        /// 多了spinCount的概念
        /// 作用范围也不同了
        /// </summary>
        public static void ManualResetSlimCheck()
        {
            var reset = new ManualResetEventSlim(false);

            Task.Run(async () =>
            {
                while (true)
                {
                    // 每隔3s放进要给敌人
                    await Task.Delay(3000);
                    Console.WriteLine("开城门");
                    reset.Set();
                }
            });

            foreach (var item in Enumerable.Range(1, 10))
            {
                reset.Wait();

                Console.WriteLine($"{DateTimeOffset.Now}-第{item}名敌人进入");

                Console.WriteLine("关城门");
                reset.Reset();
            }
        }
    }
}

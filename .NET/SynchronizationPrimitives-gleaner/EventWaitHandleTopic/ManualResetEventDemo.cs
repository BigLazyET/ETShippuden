using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventWaitHandleTopic
{
    public static class ManualResetEventDemo
    {
        /// <summary>
        /// ManualResetEvent
        /// 每一次Set信号量会被所有wait的线程获取
        /// </summary>
        public static void ManualResetCheck()
        {
            var reset = new ManualResetEvent(false);

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

            Task.Run(() =>
            {
                foreach (var item in Enumerable.Range(1, 10))
                {
                    reset.WaitOne();

                    Console.WriteLine("打开东门放敌人进来");
                    Console.WriteLine($"{DateTimeOffset.Now}-第{item}名敌人进入");

                    Console.WriteLine("关闭东门");
                    reset.Reset();
                }
            });

            Task.Run(() =>
            {
                foreach (var item in Enumerable.Range(1, 10))
                {
                    reset.WaitOne();

                    Console.WriteLine("打开南门放敌人进来");
                    Console.WriteLine($"{DateTimeOffset.Now}-第{item}名敌人进入");

                    Console.WriteLine("关闭南门");
                    reset.Reset();
                }
            });

            Task.Run(() =>
            {
                foreach (var item in Enumerable.Range(1, 10))
                {
                    reset.WaitOne();

                    Console.WriteLine("打开西门放敌人进来");
                    Console.WriteLine($"{DateTimeOffset.Now}-第{item}名敌人进入");

                    Console.WriteLine("关闭西门");
                    reset.Reset();
                }
            });

            Task.Run(() =>
            {
                foreach (var item in Enumerable.Range(1, 10))
                {
                    reset.WaitOne();

                    Console.WriteLine("打开北门放敌人进来");
                    Console.WriteLine($"{DateTimeOffset.Now}-第{item}名敌人进入");

                    Console.WriteLine("关闭北门");
                    reset.Reset();
                }
            });
        }
    }
}

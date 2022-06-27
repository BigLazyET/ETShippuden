using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventWaitHandleTopic
{
    public static class AutoResetEventDemo
    {
        /// <summary>
        /// AutoResetEvent
        /// 每一次的Set信号只会被一个wait的线程获取，顺序无序
        /// </summary>
        public static void AutoResetCheck()
        {
            var reset = new AutoResetEvent(false);

            Task.Run(async () =>
            {
                while (true)
                {
                    // 每隔3s放进一个敌人
                    await Task.Delay(3000);
                    Console.WriteLine("开门！");
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
                }
            });

            Task.Run(() =>
            {
                foreach (var item in Enumerable.Range(1, 10))
                {
                    reset.WaitOne();

                    Console.WriteLine("打开南门放敌人进来");
                    Console.WriteLine($"{DateTimeOffset.Now}-第{item}名敌人进入");
                }
            });

            Task.Run(() =>
            {
                foreach (var item in Enumerable.Range(1, 10))
                {
                    reset.WaitOne();

                    Console.WriteLine("打开西门放敌人进来");
                    Console.WriteLine($"{DateTimeOffset.Now}-第{item}名敌人进入");
                }
            });

            Task.Run(() =>
            {
                foreach (var item in Enumerable.Range(1, 10))
                {
                    reset.WaitOne();

                    Console.WriteLine("打开北门放敌人进来");
                    Console.WriteLine($"{DateTimeOffset.Now}-第{item}名敌人进入");
                }
            });
        }
    }
}

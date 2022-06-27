using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TplTopic
{
    // TODO
    class TaskAbout
    {
        void Test()
        {
            var task1 = Task.Factory.StartNew(async () => "task1");
            var task2 = Task.Run(async () => { await Task.Delay(1000); return "task2"; });
            var task3 = Task.Factory.StartNew(() => "task3");
            var task4 = Task.Run(() => "task4");

            var task1Unwrap = task1.Unwrap();

            Console.WriteLine($"task1 type: {task1.GetType()}");
            Console.WriteLine($"task2 type: {task2.GetType()}");
            Console.WriteLine($"task3 type: {task3.GetType()}");
            Console.WriteLine($"task4 type: {task4.GetType()}");
            Console.WriteLine($"task1Unwrap type: {task1Unwrap.GetType()}");

            task1.ContinueWith(async task =>
            {
                var result = await (await task);
                Console.WriteLine($"task1 result: {result}");
            });

            task2.ContinueWith(async task =>
            {
                var result = await task;
                Console.WriteLine($"task2 result: {result}");
            });

            task3.ContinueWith(task =>
            {
                Console.WriteLine($"task3 result: {task.Result}");
            });

            task4.ContinueWith(task =>
            {
                Console.WriteLine($"task4 result: {task.Result}");
            });

            task1Unwrap.ContinueWith(task =>
            {
                Console.WriteLine($"task1Unwrap result: {task.Result}");
            });
        }

        // About ChangeToken !
        // https://www.cnblogs.com/uoyo/p/12509871.html
    }
}

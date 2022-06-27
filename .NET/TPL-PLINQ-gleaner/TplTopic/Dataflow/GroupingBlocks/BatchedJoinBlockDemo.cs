using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow.GroupingBlocks
{
    public static class BatchedJoinBlockDemo
    {
        public static void Test()
        {
            // For demonstration, create a Func<int, int> that
            // returns its argument, or throws ArgumentOutOfRangeException
            // if the argument is less than zero.
            Func<int, int> DoWork = n =>
            {
                if (n < 0)
                    throw new ArgumentOutOfRangeException();
                return n;
            };

            // Create a BatchedJoinBlock<int, Exception> object that holds
            // seven elements per batch.
            var batchedJoinBlock = new BatchedJoinBlock<int, Exception>(3, new GroupingDataflowBlockOptions { Greedy = true });

            // Post several items to the block.
            foreach (int i in new int[] { 5, 6, -7, -22, 13, 55, 0 })
            {
                try
                {
                    // Post the result of the worker to the
                    // first target of the block.
                    batchedJoinBlock.Target1.Post(DoWork(i));
                }
                catch (ArgumentOutOfRangeException e)
                {
                    // If an error occurred, post the Exception to the
                    // second target of the block.
                    batchedJoinBlock.Target2.Post(e);
                }
            }

            // 如果那7个消息没有被receive完全，那么实际永远没有完成，则ContinueWith之后的任务则永远不会执行！
            batchedJoinBlock.Complete();
            batchedJoinBlock.Completion.ContinueWith(task => { Console.WriteLine("End Work"); });

            for (int i = 0; i < 4; i++)
            {
                // Read the results from the block.
                var results = batchedJoinBlock.Receive();

                // Print the results to the console.

                // Print the results.
                foreach (int n in results.Item1)
                {
                    Console.WriteLine(n);
                }
                // Print failures.
                foreach (Exception e in results.Item2)
                {
                    Console.WriteLine(e.Message);
                }
            }



            /* Output:
               5
               6
               13
               55
               0
               Specified argument was out of the range of valid values.
               Specified argument was out of the range of valid values.
             */
        }
    }
}

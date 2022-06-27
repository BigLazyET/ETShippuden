using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PLinqTopic.Expansions
{
    // https://devblogs.microsoft.com/pfxteam/parallel-extensions-and-io/
    public static class ParallelWithNetIODemo
    {
        static string[] Resources = new string[]
        {
            "http://www.microsoft.com", "http://www.msdn.com",
            "http://www.msn.com", "http://www.bing.com"
        };

        public static void Demo()
        {
            Stopwatch sw;
            ServicePointManager.DefaultConnectionLimit = 8;

            for (; ; )
            {
                sw = Stopwatch.StartNew();
                Seq();
                Console.WriteLine(sw.Elapsed);

                sw = Stopwatch.StartNew();
                PForEach();
                Console.WriteLine(sw.Elapsed);

                sw = Stopwatch.StartNew();
                Plinq();
                Console.WriteLine(sw.Elapsed);

                sw = Stopwatch.StartNew();
                PlinqTask();
                Console.WriteLine(sw.Elapsed);

                sw = Stopwatch.StartNew();
                GoodTasks();
                Console.WriteLine(sw.Elapsed);

                Console.ReadLine();
            }

            static void Seq()
            {
                var data = new List<byte[]>();
                var wc = new WebClient();

                foreach (string resource in Resources)
                {
                    data.Add(wc.DownloadData(resource));
                }

                // Use the data.
            }

            static void PForEach()
            {
                var data = new ConcurrentBag<byte[]>();

                Parallel.ForEach(Resources, resource =>
                {
                    data.Add((new WebClient()).DownloadData(resource));
                });

                // Use the data.
            }

            static void Plinq()
            {
                int numConcurrentRetrievals = 2;

                var data =
                    from resource in Resources.
                        AsParallel().WithDegreeOfParallelism(numConcurrentRetrievals)
                    select (new WebClient()).DownloadData(resource);

                // Sometime later...
                foreach (byte[] result in data) { }
            }

            static void PlinqTask()
            {
                int numConcurrentRetrievals = 2;

                var t = Task.Factory.StartNew(() =>
                {
                    return
                        from resource in Resources
                            .AsParallel().WithDegreeOfParallelism(numConcurrentRetrievals)
                        select (new WebClient()).DownloadData(resource).ToArray();
                });

                // Sometime later...
                foreach (byte[] result in t.Result) { }

                // OR, use a continuation
                //t.ContinueWith(dataTask =>
                //{
                //    foreach (byte[] result in dataTask.Result) { }
                //});
            }

            static void GoodTasks()
            {
                var tasks = new Queue<Task<byte[]>>();

                foreach (string resource in Resources)
                {
                    WebClient wc = new WebClient();
                    tasks.Enqueue(wc.DownloadDataTask(new Uri(resource)));
                }

                // Sometime later...
                while (tasks.Count > 0)
                {
                    byte[] result = tasks.Dequeue().Result;
                }

                // OR, use a continuation
                //Task<byte[]>.Factory.ContinueWhenAll(tasks.ToArray(), dataTasks =>
                //{
                //    foreach (var dataTask in dataTasks) { byte[] result = dataTask.Result; }
                //});
            }
        }
    }

    internal class EAPCommon
    {
        internal static TaskCompletionSource<T> CreateTaskCompletionSource<T>(object state)
        {
            // Helper to create a new TCS with the right state and creation options
            return new TaskCompletionSource<T>(state, TaskCreationOptions.AttachedToParent);
        }

        internal static void TransferCompletionToTask<T>(
            TaskCompletionSource<T> tcs, AsyncCompletedEventArgs e, Func<T> getResult)
        {
            // Transfers the results from the AsyncCompletedEventArgs and getResult() to the
            // TaskCompletionSource, but only AsyncCompletedEventArg's UserState matches the TCS.
            // This latter step is important if the same WebClient is used for multiple, asynchronous
            // operations concurrently.
            if (e.UserState == tcs)
            {
                if (e.Cancelled) tcs.SetCanceled();
                else if (e.Error != null) tcs.SetException(e.Error);
                else tcs.SetResult(getResult());
            }
        }
    }

    public static class WebClientExtensions
    {
        /// <summary>Downloads the resource with the specified URI as a byte array, asynchronously.</summary>
        /// <param name="webClient">The WebClient.</param>
        /// <param name="address">The URI from which to download data.</param>
        /// <returns>A Task that contains the downloaded data.</returns>
        public static Task<byte[]> DownloadDataTask(this WebClient webClient, Uri address)
        {
            // Create the task to be returned
            var tcs = EAPCommon.CreateTaskCompletionSource<byte[]>(address);

            // Setup the callback event handler
            DownloadDataCompletedEventHandler handler = null;
            handler = (sender, e) =>
            {
                EAPCommon.TransferCompletionToTask(tcs, e, () => e.Result);
                webClient.DownloadDataCompleted -= handler;
            };
            webClient.DownloadDataCompleted += handler;

            // Start the async work
            try
            {
                webClient.DownloadDataAsync(address, tcs);
            }
            catch
            {
                // If something goes wrong kicking off the async work,
                // unregister the callback and cancel the created task
                webClient.DownloadDataCompleted -= handler;
                tcs.TrySetCanceled();
                throw;
            }

            // Return the task that represents the async operation
            return tcs.Task;
        }
    }
}

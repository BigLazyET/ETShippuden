using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Forms;

namespace DataflowCancellationWinform
{
    public partial class Form1 : Form
    {
        CancellationTokenSource cancellationTokenSource;

        TransformBlock<WorkItem, WorkItem> startBlock;

        ActionBlock<WorkItem> completeBlock;

        ActionBlock<ProgressBar> increaseBlock;

        ActionBlock<ProgressBar> decreaseBlock;

        TaskScheduler uiTaskScheduler;

        public Form1()
        {
            InitializeComponent();

            uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            complete.Enabled = false;
        }

        private void start_Click(object sender, EventArgs e)
        {
            if (!complete.Enabled)
            {
                CreatePipeline();

                complete.Enabled = true;
            }

            foreach (var item in Enumerable.Range(0, 20))
            {
                progressBar1.Value++;
                startBlock.Post(new WorkItem());
            }

            startBlock.Complete();
            startBlock.Completion.ContinueWith(task =>
            {
                Debug.WriteLine($"{DateTimeOffset.Now} startBlock completed！");
                Debug.WriteLine($"{DateTimeOffset.Now} startBlock completion status：{startBlock.Completion.Status}");
            });
        }

        private async void complete_Click(object sender, EventArgs e)
        {
            start.Enabled = false;
            complete.Enabled = false;

            cancellationTokenSource.Cancel();

            try
            {
                Debug.WriteLine($"{DateTimeOffset.Now} Task.WhenAll Start");
                await Task.WhenAll(completeBlock.Completion, increaseBlock.Completion, decreaseBlock.Completion);
                Debug.WriteLine($"{DateTimeOffset.Now} Task.WhenAll End");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{DateTimeOffset.Now} error：{ex.Message}");
            }

            progressBar4.Value += progressBar1.Value;
            progressBar4.Value += progressBar2.Value;

            progressBar1.Value = 0;
            progressBar2.Value = 0;

            start.Enabled = true;
        }

        void CreatePipeline()
        {
            cancellationTokenSource = new CancellationTokenSource();

            increaseBlock = new ActionBlock<ProgressBar>(progressBar => progressBar.Value++,
                new ExecutionDataflowBlockOptions { CancellationToken = cancellationTokenSource.Token, TaskScheduler = uiTaskScheduler });

            decreaseBlock = new ActionBlock<ProgressBar>(progressBar => progressBar.Value--,
                new ExecutionDataflowBlockOptions { CancellationToken = cancellationTokenSource.Token, TaskScheduler = uiTaskScheduler });

            startBlock = new TransformBlock<WorkItem, WorkItem>(workItem =>
            {
                Debug.WriteLine($"{DateTimeOffset.Now} start");

                workItem.DoWork(200);

                decreaseBlock.Post(progressBar1);

                increaseBlock.Post(progressBar2);

                return workItem;
            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationTokenSource.Token
            });

            completeBlock = new ActionBlock<WorkItem>(workItem =>
            {
                Debug.WriteLine($"{DateTimeOffset.Now} complete");

                workItem.DoWork(1000);

                decreaseBlock.Post(progressBar2);

                increaseBlock.Post(progressBar3);
            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationTokenSource.Token,
                MaxDegreeOfParallelism = 2
            });

            startBlock.LinkTo(completeBlock, new DataflowLinkOptions { PropagateCompletion = true });
            completeBlock.Completion.ContinueWith(task =>
            {
                Debug.WriteLine($"{DateTimeOffset.Now} completeBlock completed！");
                Debug.WriteLine($"{DateTimeOffset.Now} completeBlock completion status：{completeBlock.Completion.Status}");
                decreaseBlock.Complete();
                increaseBlock.Complete();
            });
            decreaseBlock.Completion.ContinueWith(task =>
            {
                Debug.WriteLine($"{DateTimeOffset.Now} decreaseBlock completed！");
                Debug.WriteLine($"{DateTimeOffset.Now} decreaseBlock completion status：{decreaseBlock.Completion.Status}");
            });
            increaseBlock.Completion.ContinueWith(task =>
            {
                Debug.WriteLine($"{DateTimeOffset.Now} increaseBlock completed！");
                Debug.WriteLine($"{DateTimeOffset.Now} increaseBlock completion status：{increaseBlock.Completion.Status}");
            });
        }

        ~Form1()
        {
            cancellationTokenSource.Dispose();
        }
    }

    class WorkItem
    {
        public void DoWork(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }
    }
}

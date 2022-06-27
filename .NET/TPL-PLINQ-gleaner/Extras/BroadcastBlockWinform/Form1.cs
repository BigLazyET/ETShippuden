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

namespace BroadcastBlockWinform
{
    // 官方demo: https://docs.microsoft.com/zh-cn/dotnet/standard/parallel-programming/how-to-specify-a-task-scheduler-in-a-dataflow-block
    public partial class Form1 : Form
    {
        // Broadcast Block Link to Action Block
        // Action Block associated with each checkbox
        // Action Block do the actual work
        BroadcastBlock<int> broadcaster = new BroadcastBlock<int>(null);

        public Form1()
        {
            InitializeComponent();

            // 用于切换checkbox选中的状态的ActionBlock
            // 此数据流快需要在用户界面上执行，所以需要运行在用户界面线程上
            var toggleAction = new ActionBlock<CheckBox>(checkBox =>
            {
                checkBox.Checked = !checkBox.Checked;
            },
            new ExecutionDataflowBlockOptions
            {
                // 创建一个在当前上下文中执行的TaskScheduler
                // 当前Form1的构造是在用户界面线程执行的，所以当前的数据流块的操作也会在用户界面线程上工作
                TaskScheduler = TaskScheduler.FromCurrentSynchronizationContext()
            });

            // 同步 和 独占 -> 可用于实现读写操作
            // 拓展链接：https://www.cnblogs.com/qixinbo/p/9591333.html
            // 读和写数据流块都配置到当前的taskSchedulerPair对象上
            // 下面设置读数据流块操作可以并发执行；而写数据流块操作独占执行
            // 当涉及到多个数据流块对同一资源可以共享访问，也有需要独占使访问的时候，这种方式很合适 -> 因为我们不需要手动同步对资源的访问
            var taskSchedulerPair = new ConcurrentExclusiveSchedulerPair();

            // 用于接受boradcast block广播的消息并做出对应的action动作 -> post消息让toggleAction执行
            var readActions = from checkbox in new CheckBox[] { checkBox1, checkBox2, checkBox3 }
                              select new ActionBlock<int>(millseconds =>
                              {
                                  toggleAction.Post(checkbox);

                                  Thread.Sleep(millseconds);

                                  toggleAction.Post(checkbox);
                              },
                              new ExecutionDataflowBlockOptions
                              {
                                  TaskScheduler = taskSchedulerPair.ConcurrentScheduler
                              });

            var writeAction = new ActionBlock<int>(millseconds =>
            {
                toggleAction.Post(checkBox4);

                Thread.Sleep(millseconds);

                toggleAction.Post(checkBox4);
            },
            new ExecutionDataflowBlockOptions
            {
                TaskScheduler = taskSchedulerPair.ExclusiveScheduler
                //TaskScheduler = taskSchedulerPair.ConcurrentScheduler
            });

            // Broadcast block link to action blocks (write/read)
            foreach (var readAction in readActions)
            {
                broadcaster.LinkTo(readAction);
            }
            broadcaster.LinkTo(writeAction);

            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Debug.WriteLine($"Time Tick： {DateTimeOffset.Now}");
            broadcaster.Post(5000);
        }
    }
}

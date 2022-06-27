using System;
using System.Threading;

namespace PLinqTopic
{
    class Program
    {
        static void Main(string[] args)
        {
            // 延迟执行
            //Console.WriteLine("------延迟执行------");
            //DeferExecutionDemo.DogsFeed();

            // PLINQ的顺序
            //Console.WriteLine("------无序------");
            //OrderDemo.DogsUnOrderedFeed();
            //Console.WriteLine("------有序------");
            //OrderDemo.DogsOrderedFeed();
            //Console.WriteLine("------有序query，但ForAll输出结果------");
            //OrderDemo.DogsOrderedFeedForAllOutput();
            //Console.WriteLine("------从PLINQ查询控制排序结果中获取指定索引处的结果------");
            //OrderDemo.DogsOrderedFeedIndex();
            //Console.WriteLine("------删除排序约束以加快连接速度------");
            //OrderDemo.ChooseRubberAndPencil();
            //Console.WriteLine("------AddSequential------");
            //OrderDemo.PencilSequence();
            //Console.WriteLine("------强制PLINQ执行策略为并行执行------");
            //OrderDemo.ForceParallelism();

            // Exception的处理
            //Console.WriteLine("------异常直接导致并行任务报错------");
            //DealExceptionDemo.CrashNotKeepOn();
            //Console.WriteLine("------异常即时输出+并行任务继续------");
            //DealExceptionDemo.CrashStillKeepOn();
            //Console.WriteLine("------异常获取+并行任务继续------");
            //DealExceptionDemo.ExceptionCollectStillKeepOn();

            // PLINQ的取消
            //Console.WriteLine("------取消并行任务------");
            //DealCancelDemo.ParallelCancel();
            //Console.WriteLine("------取消并行任务中的耗时任务------");
            //DealCancelDemo.ParallelCancelCostPlentyTimeTask();

            // PLINQ的自定义累加函数算法
            //Console.WriteLine("------自定义实现标准方差------");
            //CustomizeAggregate.GradesStandardDeviation();

            // PLINQ的指定合并选项
            Console.WriteLine("------指定PLINQ查询的合并选项------");
            ParallelMergeOptionDemo.ChooseMergeMode();

            //Console.WriteLine(DateTimeOffset.Now);
            //Thread.SpinWait(500000000);
            //Console.WriteLine(DateTimeOffset.Now);

            Console.ReadLine();
        }
    }
}

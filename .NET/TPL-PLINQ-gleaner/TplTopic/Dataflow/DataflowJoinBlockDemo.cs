using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow
{
    public static class DataflowJoinBlockDemo
    {
        /// <summary>
        /// 模拟场景：一个财务人员和一个税务人员使用同一印章在各自文件上盖章
        /// </summary>
        public static void SealPaper()
        {
            var financePaper = new BufferBlock<FinancePaper>();
            var seal = new BufferBlock<Seal>();
            var taxPaper = new BufferBlock<TaxPaper>();

            var financePrepare = new JoinBlock<FinancePaper, Seal>(new GroupingDataflowBlockOptions { Greedy = false });
            var taxPrepare = new JoinBlock<TaxPaper, Seal>(new GroupingDataflowBlockOptions { Greedy = false });

            var financeWork = new ActionBlock<Tuple<FinancePaper, Seal>>(async item =>
             {
                 Console.WriteLine($"do finance work {item.Item1.Flag}...");

                 await Task.Delay(2000);

                 Console.WriteLine($"do finance work {item.Item1.Flag} done...");

                 financePaper.Post(item.Item1);
                 seal.Post(item.Item2);
             });
            var taxWork = new ActionBlock<Tuple<TaxPaper, Seal>>(async item =>
             {
                 Console.WriteLine($"do tax work {item.Item1.Flag}...");

                 await Task.Delay(1000);

                 Console.WriteLine($"do tax work {item.Item1.Flag} done...");

                 taxPaper.Post(item.Item1);
                 seal.Post(item.Item2);
             });

            // 财务
            financePaper.LinkTo(financePrepare.Target1);
            seal.LinkTo(financePrepare.Target2);
            financePrepare.LinkTo(financeWork);

            // 税务
            taxPaper.LinkTo(taxPrepare.Target1);
            seal.LinkTo(taxPrepare.Target2);
            taxPrepare.LinkTo(taxWork);

            Enumerable.Range(0, 3).AsParallel().ForAll(_ => financePaper.Post(new FinancePaper { Flag = _ }));
            Enumerable.Range(0, 4).AsParallel().ForAll(_ => taxPaper.Post(new TaxPaper { Flag = _ }));
            seal.Post(new Seal());

            //financeWork.Complete();
            //taxWork.Complete();

            //financeWork.Completion.ContinueWith(task => Console.WriteLine("finance work done!"));
            //taxWork.Completion.ContinueWith(task => Console.WriteLine("tax work done!"));
        }

        /// <summary>
        /// 财务文件
        /// </summary>
        class FinancePaper { public int Flag { get; set; } }
        /// <summary>
        /// 税务文件
        /// </summary>
        class TaxPaper { public int Flag { get; set; } }
        /// <summary>
        /// 印章
        /// </summary>
        class Seal { }
    }
}

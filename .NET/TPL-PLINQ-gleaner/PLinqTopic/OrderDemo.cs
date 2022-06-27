using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace PLinqTopic
{
    /// <summary>
    /// 此类讨论PLINQ执行的顺序问题
    /// </summary>
    public static class OrderDemo
    {
        /// <summary>
        /// PLINQ查询不控制排序，这也是默认的形式
        /// </summary>
        public static void DogsUnOrderedFeed()
        {
            var dogFlags = Enumerable.Range(1, 10);

            // 偶数狗得到食物
            var flags = from dogFlag in dogFlags.AsParallel()
                        where dogFlag % 2 == 0
                        select dogFlag;

            foreach (var flag in flags)
            {
                Console.WriteLine($"dog{flag} gets food");
            }
        }

        /// <summary>
        /// PLINQ查询控制排序 + PLINQ的延迟执行特性
        /// </summary>
        public static void DogsOrderedFeed()
        {
            var dogFlags = Enumerable.Range(1, 10).ToList();

            // 偶数狗得到食物
            var flags = from dogFlag in dogFlags.AsParallel().AsOrdered()
                        where dogFlag % 2 == 0
                        select dogFlag;

            dogFlags.AddRange(Enumerable.Range(100, 10));

            foreach (var flag in flags)
            {
                Console.WriteLine($"dog{flag} gets food");
            }
        }

        /// <summary>
        /// PLINQ查询控制排序 + 结果用ForAll输出 => 导致输出结果依然是无序的
        /// </summary>
        public static void DogsOrderedFeedForAllOutput()
        {
            var dogFlags = Enumerable.Range(1, 20);

            // 偶数狗得到食物
            (from dogFlag in dogFlags.AsParallel().AsOrdered()
             where dogFlag % 2 == 0
             select dogFlag).ForAll(f =>
             {
                 Console.WriteLine($"dog{f} gets food");
             });
        }

        /// <summary>
        /// 从PLINQ查询控制排序结果中获取指定索引处的结果
        /// </summary>
        public static void DogsOrderedFeedIndex()
        {
            var dogFlags = Enumerable.Range(1, 20);

            var flags = from dogFlag in dogFlags.AsParallel().AsOrdered()
                        where dogFlag % 2 == 0
                        select dogFlag;

            Console.WriteLine($"dog #7 in evenDogs：dog{flags.ElementAt(6)}");

            Console.WriteLine($"dog #7 in evenDogs：dog{ dogFlags.AsParallel().AsOrdered().Where(f => f % 2 == 0).ElementAt(6)}");
        }

        public static void ChooseRubberAndPencil()
        {
            var brands = new string[] { "gucci", "channel", "givency", "cpb", "dior", "ysl", "gxg", "晨光" };
            var colors = new string[] { "white", "red", "orange", "yellow", "green", "cyan", "blue", "purple", "black" };
            var random = new Random();

            var rubbers = Enumerable.Range(1, 10000000).Select(x => new Rubber { Brand = brands[random.Next(0, 7)], Id = x });
            var pencils = Enumerable.Range(1, 10000000).Select(x => new Pencil { Color = colors[random.Next(0, 8)], Id = x });

            var sw = new Stopwatch();
            sw.Start();

            var choose = rubbers.AsParallel().Where(x => x.Brand == "channel").Select(r => r).OrderByDescending(r => r.Id).Take(20)
                // 删除排序约束以加快连接速度
                .AsUnordered()
                .Join(
                pencils.AsParallel(), rub => rub.Id, pen => pen.Id, (rub, pen) =>
                {
                    return new { Brand = rub.Brand, Id = rub.Id, Color = pen.Color };
                }).OrderBy(x => x.Color);

            foreach (var item in choose)
            {
                Console.WriteLine($"{item.Id}-{item.Brand}-{item.Color}");
            }
            Console.WriteLine($"cost {sw.ElapsedMilliseconds}ms");
            sw.Stop();
        }

        /// <summary>
        /// AsSequential指示并行LINQ引擎按顺序执行查询，即不并行执行
        /// </summary>
        public static void PencilSequence()
        {
            var random = new Random();
            var owners = new string[] { "tom", "max", "larry", "simen", "philx", "jerry", "jack", "john", "jorage" };
            var pencils = Enumerable.Range(0, 9).Select(p => new Pencil { Id = p, Owner = owners[p] });
            var allPencils = from pencil in pencils.AsParallel() orderby pencil.Id select pencil;
            var unSequencePencils = (from pencil in pencils.AsParallel() orderby pencil.Id select pencil).Take(2);
            var sequencePencils = (from pencil in pencils.AsParallel() orderby pencil.Id select new { pencil.Id, pencil.Owner }).AsSequential().Take(2);

            foreach (var item in allPencils)
            {
                Console.WriteLine($"allPencils：{item.Id}-{item.Owner}");
            }
            foreach (var item in unSequencePencils)
            {
                Console.WriteLine($"unSequencePencils：{item.Id}-{item.Owner}");
            }
            foreach (var item in sequencePencils)
            {
                Console.WriteLine($"sequencePencils：{item.Id}-{item.Owner}");
            }
        }

        /// <summary>
        /// 强制PLINQ的执行策略为并行执行
        /// 通过WithExecutionMode指定
        /// </summary>
        public static void ForceParallelism()
        {
            var dogs = new string[] { "tom", "max", "larry", "simen", "philx", "jerry", "jack", "john", "jorage" };
            var parallelQuery = from dog in dogs.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism) where dog.Length > 4 select dog;
            parallelQuery.ForAll(d => Console.WriteLine($"dog's name：{d}"));    // 每次输出都是无序
            parallelQuery.ToList().ForEach(d => Console.WriteLine($"the name of dog is：{d}"));  
        }

        class Rubber
        {
            public string Brand { get; set; }

            public int Id { get; set; }
        }

        struct Pencil
        {
            public string Color { get; set; }

            public int Id { get; set; }

            public string Owner { get; set; }
        }
    }
}

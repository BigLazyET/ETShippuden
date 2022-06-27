using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PLinqTopic
{
    public static class CustomizeAggregate
    {
        public static void GradesStandardDeviation()
        {
            var groupA = new int[] { 95, 85, 75, 65, 55, 45 };
            var groupB = new int[] { 73, 72, 71, 69, 68, 67 };

            var groupASd = MockStandardDeviation(groupA);
            var groupBSd = MockStandardDeviation(groupB);

            Console.WriteLine($"Group A Standard Deviation：{groupASd}");
            Console.WriteLine($"Group B Standard Deviation：{groupBSd}");
        }

        /// <summary>
        /// 模拟标准方差方法 - 样本标准偏差 - 以N-1为底
        /// </summary>
        /// <returns></returns>
        public static double MockStandardDeviation(int[] grades)
        {
            // 平均值
            var mean = grades.AsParallel().Average();

            var standardDeviation = grades.AsParallel().Aggregate(
                0.0,    // 也可以为0d，代表初始累加值，同时此值会告诉编译器这是个double类型的累加函数
                (subTotal, item) => subTotal + Math.Pow(item - mean, 2),    // 这是在每个分区里的计算逻辑（理解：注意并行任务是通过将源序列进行分区分段，从而实现并行处理的行为）
                (total, thisThread) => total + thisThread,  // 将每个分区里的总和再次累加起来
                (finalSum) => Math.Sqrt(finalSum / (grades.Length - 1)));   // 最后所有分区累加的结果进行左后的逻辑运算得出最后的结论

            return standardDeviation;
        }
    }
}

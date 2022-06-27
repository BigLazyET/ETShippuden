using System;
using System.Collections.Generic;
using System.Linq;

namespace PLinqTopic
{
    public static class DeferExecutionDemo
    {
        // 起始脏兮兮的狗列表中只有A
        // 针对列表进行LINQ查询（将脏兮兮的狗洗干净）
        // 此时再对脏兮兮的狗列表加入B
        // 最后对LINQ查询的结果进行遍历：将洗干净的狗进行喂食
        // 会发现虽然LINQ查询在前，添加狗B在后，但是最终结果狗B依然洗干净了，得到了食物
        // 这就是LINQ的延迟执行
        public static void DogsFeed()
        {
            Console.WriteLine("Dinner Time!");
            var dirtyDogs = new List<string>();
            dirtyDogs.Add("dirtyDogA");
            var cleanDogs = from dog in dirtyDogs select dog.Replace("dirty","clean");  // LINQ创建查询
            dirtyDogs.Add("dirtyDogB"); // 加入另一只脏兮兮的狗B
            foreach(var cleanDog in cleanDogs)
            {
                Console.WriteLine($"{cleanDog} will get his food");
            }
        }
    }
}
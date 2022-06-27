using System;
using System.Threading.Tasks;

namespace Learn.Demo50.ConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Task.Run(async () =>
            {
                while (true)
                {
                    Console.WriteLine(DateTime.Now);
                    await Task.Delay(5000);
                }
            });

            Console.Read();
        }
    }
}

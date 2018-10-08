using System;

namespace Tpl.Dataflow
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("请输入管道并发数:");
            if (int.TryParse(Console.ReadLine(), out int max))
            {
                var pipeline = new Pipeline(max);
                var task = pipeline.Builder();
                pipeline.Process(new[] { "码", "农", "阿", "宇" });
                task.Wait();
                Console.ReadKey();
            }
        }
    }
}

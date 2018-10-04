using System;

namespace Tpl.Dataflow
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var pipeline = new Pipeline(1);
            var task = pipeline.Builder();
            pipeline.Process(new[] { "码", "农", "阿", "宇" });
            task.Wait();
        }
    }
}

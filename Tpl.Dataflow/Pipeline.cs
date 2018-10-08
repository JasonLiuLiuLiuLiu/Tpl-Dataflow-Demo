using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tpl.Dataflow
{
    public class Pipeline
    {
        IPropagatorBlock<string, Dictionary<int, string>> _startBlock;
        private int _maxDegreeOfParallelism;

        public Pipeline(int maxDegreeOfParallelism)
        {
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        public void Process(string[] inputs)
        {
            if (inputs == null)
                return;
            foreach (var input in inputs)
            {
                _startBlock.Post(input);
            }
            _startBlock.Complete();
        }

        public Task Builder()
        {
            _startBlock = Process1();
            var process2Block = Process2();
            var process3Block = Process3();

            _startBlock.LinkTo(process2Block, new DataflowLinkOptions() { PropagateCompletion = true });

            process2Block.LinkTo(process3Block, new DataflowLinkOptions() { PropagateCompletion = true });

            process3Block.Completion.ContinueWith(_ =>
            {
                Console.WriteLine($"Process3 Complete,State{_.Status}");
                Console.WriteLine("所有任务处理完成");
            });

            return process3Block.Completion;
        }

        private IPropagatorBlock<string, Dictionary<int, string>> Process1()
        {
            var bufferBlock = new BufferBlock<Dictionary<int, string>>();
            var actionBlock = new ActionBlock<string>(x =>
              {
                  Console.WriteLine($"Process1 处理中:{x}");
                  Thread.Sleep(5000);
                  var dic = new Dictionary<int, string> { { 0, x } };
                  dic.Add(1, "Process1");
                  bufferBlock.Post(dic);
              }, new ExecutionDataflowBlockOptions
              {
                  MaxDegreeOfParallelism = _maxDegreeOfParallelism
              });
            actionBlock.Completion.ContinueWith(_ =>
            {
                Console.WriteLine($"Process1 Complete,State{_.Status}");
                bufferBlock.Complete();
            });
            return DataflowBlock.Encapsulate(actionBlock, bufferBlock);

        }

        private IPropagatorBlock<Dictionary<int, string>, Dictionary<int, string>> Process2()
        {
            var block = new TransformBlock<Dictionary<int, string>, Dictionary<int, string>>(dic =>
                  {
                      Console.WriteLine($"Process2 处理中：{dic.First().Value}");
                      Thread.Sleep(5000);
                      dic.Add(2, "Process2");
                      return dic;
                  }, new ExecutionDataflowBlockOptions
                  {
                      MaxDegreeOfParallelism = _maxDegreeOfParallelism
                  }
               );

            block.Completion.ContinueWith(_ =>
            {
                Console.WriteLine($"Process2 Complete,State{_.Status}");
            });

            return block;
        }

        private ITargetBlock<Dictionary<int, string>> Process3()
        {
            var actionBlock = new ActionBlock<Dictionary<int, string>>(dic =>
               {
                   Console.WriteLine($"Process3 处理中：{dic.First().Value}");
                   Thread.Sleep(5000);
                   dic.Add(3, "Process3");
                   Console.WriteLine("Dic中的内容如下：");
                   foreach (var item in dic)
                   {
                       Console.Write($"{item.Key}:{item.Value}||");
                   }
                   Console.WriteLine();
               }, new ExecutionDataflowBlockOptions
               {
                   MaxDegreeOfParallelism = _maxDegreeOfParallelism
               });
            return actionBlock;
        }
    }
}

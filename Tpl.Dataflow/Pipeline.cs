using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Tpl.Dataflow
{
    public class Pipeline
    {
        private ITargetBlock<string> _startBlock;
        private int _maxMessagesPerTask;

        public Pipeline(int maxMessagesPerTask)
        {
            _maxMessagesPerTask = maxMessagesPerTask;
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
            var process1Block = Process1();
            var process2Block = Process2();
            var process3Block = Process3();
            process1Block.LinkTo(process2Block, new DataflowLinkOptions() { PropagateCompletion = true });

            process2Block.LinkTo(process3Block, new DataflowLinkOptions() { PropagateCompletion = true });

            process3Block.Completion.ContinueWith(_ =>
            {
                Console.WriteLine($"Process1 Complete,State{_.Status}");
                Console.WriteLine("所有任务处理完成");
            });

            return process3Block.Completion;
        }

        private IPropagatorBlock<string, Dictionary<int, string>> Process1()
        {
            var bufferBlock = new BufferBlock<Dictionary<int, string>>();
            _startBlock = new ActionBlock<string>(x =>
              {
                  Console.WriteLine($"Process1 Handing:{x}");
                  var dic = new Dictionary<int, string> { { 0, x } };
                  dic.Add(1, "Process1");
                  bufferBlock.Post(dic);
              });
            _startBlock.Completion.ContinueWith(_ =>
            {
                Console.WriteLine($"Process1 Complete,State{_.Status}");
            });
            return DataflowBlock.Encapsulate(_startBlock, bufferBlock);
        }

        private IPropagatorBlock<Dictionary<int, string>, Dictionary<int, string>> Process2()
        {
            var block = new TransformBlock<Dictionary<int, string>, Dictionary<int, string>>(dic =>
                  {
                      Console.WriteLine($"Process2 Handing{dic.First().Value}");
                      dic.Add(2, "Process2");
                      return dic;
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
                   Console.WriteLine($"Process3 Handing{dic.First().Value}");
                   dic.Add(3, "Process3");
                   Console.WriteLine("Dic中的内容如下：");
                   foreach (var item in dic)
                   {
                       Console.Write($"{item.Key}:{item.Value}||");
                   }
                   Console.WriteLine();
               });
            return actionBlock;
        }
    }
}

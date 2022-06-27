using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TplTopic.Dataflow.CustomizeDataflows
{
    /// <summary>
    /// 滑动窗口可以抽象成一种数据流块中的传播块的类型
    /// 我们可以
    /// 1. 利用Dataflow的Encapsulate方法将源跟目标连接起来形成一个传播块
    /// 2. 直接实现传播块的接口
    /// </summary>
    public static class SlidingWindowDemo
    {
        #region 通过Encapsulate实现传播块
        public static IPropagatorBlock<T, T[]> CreateSlidingWindow<T>(int windowSize)
        {
            // 队列保存窗口数据，接入新的，去掉旧的
            var queue = new Queue<T>();

            // 此源是针对于外面的块来说的，我们实现的传播块就是通过此源将数据发送给外面的块
            // 此传播块可以通过LinkTo(internal target/source/propagator block)的形式来与外部的块进行连接
            var source = new BufferBlock<T[]>();

            // 此目标是针对于外面的源来说的，我们实现的传播块就是通过此目标接收从外部块发过来的数据
            var target = new ActionBlock<T>(item =>
            {
                queue.Enqueue(item);

                if (queue.Count > windowSize)
                    queue.Dequeue();

                if (queue.Count == windowSize)
                    source.Post(queue.ToArray());
            });

            // 此传播块调用Complete()方法(传播块设置成Complete State)，其实会触发target的Complete
            // 针对target的Complete进行一些收尾工作：比如将在队列里的剩余的数据都发出去；调用source的Complete()方法，指示source完成
            target.Completion.ContinueWith(task =>
            {
                if (queue.Count > 0 && queue.Count < windowSize)
                    source.Post(queue.ToArray());
                source.Complete();
            });

            // 将 taget-source连接起来，形成传播块
            // 最终形如：外部source(或者自身Post) - [target-source] - 外部target
            return DataflowBlock.Encapsulate(target, source);
        }
        #endregion

        #region 实现IPropagatorBlock接口实现传播块
        public class SlidingWindowBlock<T> : IPropagatorBlock<T, T[]>, IReceivableSourceBlock<T[]>
        {
            // 核心逻辑跟CreateSlidingWindow方法的逻辑一致
            // 不同的是我们自己实现在CreateSlidingWindow里DataflowBlock默认实现的Encapsulate方法这部分
            // 这里面的targe和source的概念与CreateSlidingWindow标注的一致

            private readonly int windowSize;
            private readonly ITargetBlock<T> target;
            private readonly IReceivableSourceBlock<T[]> source;

            public SlidingWindowBlock(int windowSize)
            {
                var queue = new Queue<T>();

                var source = new BufferBlock<T[]>();

                var target = new ActionBlock<T>(item =>
                {
                    queue.Enqueue(item);

                    if (queue.Count > windowSize)
                        queue.Dequeue();

                    if (queue.Count == windowSize)
                        source.Post(queue.ToArray());
                });

                target.Completion.ContinueWith(delegate
                {
                    if (queue.Count > 0 && queue.Count < windowSize)
                        source.Post(queue.ToArray());
                    source.Complete();
                });

                this.windowSize = windowSize;
                this.target = target;
                this.source = source;
            }

            public int WindowSize => windowSize;

            #region IReceivableSourceBlock<TOutput>
            public bool TryReceive(Predicate<T[]> filter, out T[] item)
            {
                return source.TryReceive(filter, out item);
            }
            public bool TryReceiveAll(out IList<T[]> items)
            {
                return source.TryReceiveAll(out items);
            }
            #endregion

            #region ISourceBlock<T[]>
            // 接口定义说明：Called by a linked ITargetBlock<TInput> to accept and consume a DataflowMessageHeader previously offered by this ISourceBlock<TOutput>
            // 接口说明解释：由链接的ITargetBlock<TInput>调用，以接受和使用此ISourceBlock<TOutput>以前提供的DataflowMessageHeader
            T[] ISourceBlock<T[]>.ConsumeMessage(DataflowMessageHeader messageHeader, ITargetBlock<T[]> target, out bool messageConsumed)
            {
                return source.ConsumeMessage(messageHeader, target, out messageConsumed);
            }

            IDisposable ISourceBlock<T[]>.LinkTo(ITargetBlock<T[]> target, DataflowLinkOptions linkOptions)
            {
                return source.LinkTo(target, linkOptions);
            }

            void ISourceBlock<T[]>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T[]> target)
            {
                source.ReleaseReservation(messageHeader, target);
            }

            bool ISourceBlock<T[]>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T[]> target)
            {
                return source.ReserveMessage(messageHeader, target);
            }
            #endregion

            #region ITargetBlock<T>
            // 接口定义说明：Offers a message to the TargetBlock，giving the target the opportunity to consume or postpone the message
            // 接口定义解释：向TargetBlock提供消息, 使目标有机会使用或推迟消息。
            DataflowMessageStatus ITargetBlock<T>.OfferMessage(DataflowMessageHeader messageHeader, T messageValue, ISourceBlock<T> source, bool consumeToAccept)
            {
                return target.OfferMessage(messageHeader, messageValue, source, consumeToAccept);
            }
            #endregion

            #region IDataflowBlock
            public Task Completion => source.Completion;

            // 在构造里，target的Completion里调用了source的Complete()方法
            public void Complete() => target.Complete();

            public void Fault(Exception error) => target.Fault(error);
            #endregion
        }
        #endregion

        public static void DemonstrateSlidingWindow<T>(IPropagatorBlock<T, T[]> slidingWindow, IEnumerable<T> values)
        {
            var windowComma = string.Empty;
            var printWindow = new ActionBlock<T[]>(windowItems =>
            {
                Console.Write(windowComma);
                Console.Write("{");

                var comma = string.Empty;
                foreach (var item in windowItems)
                {
                    Console.Write(comma);
                    Console.Write(item);

                    comma = ", ";
                }

                Console.Write("}");
                Console.WriteLine();
            });

            slidingWindow.LinkTo(printWindow);

            var competitoin = printWindow.Completion.ContinueWith(delegate { Console.WriteLine("滑动窗口演示结束"); });

            slidingWindow.Completion.ContinueWith(delegate { printWindow.Complete(); });

            foreach (var value in values)
            {
                slidingWindow.Post(value);
            }

            slidingWindow.Complete();

            competitoin.Wait();
        }
    }
}

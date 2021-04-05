using System;
using AgbSharp.Core.Util;
using Xunit;

namespace AgbSharp.Core.Tests.Util
{
    public class UniqueQueue_Tests
    {
        [Fact]
        public void Enqueue_QueueString_Queued()
        {
            UniqueQueue<string> queue = new UniqueQueue<string>();

            queue.Enqueue("abc");

            Assert.Equal(1, queue.Count);
        }

        [Fact]
        public void Enqueue_QueueMultipleOfSameString_SecondQueueFailed()
        {
            UniqueQueue<string> queue = new UniqueQueue<string>();

            queue.Enqueue("abc");
            queue.Enqueue("abc");

            Assert.Equal(1, queue.Count);
        }

        [Fact]
        public void Enqueue_QueueMultipleNonSimilarStrings_AllQueuesSuccess()
        {
            UniqueQueue<string> queue = new UniqueQueue<string>();

            queue.Enqueue("abc");
            queue.Enqueue("def");
            queue.Enqueue("ghi");

            Assert.Equal(3, queue.Count);
        }

        [Fact]
        public void Enqueue_QueueAnotherOfSameStringAfterDequeue_BothQueuesSuccess()
        {
            UniqueQueue<string> queue = new UniqueQueue<string>();

            queue.Enqueue("abc");
            queue.Dequeue();
            queue.Enqueue("abc");

            Assert.Equal(1, queue.Count);
        }

        [Fact]
        public void Enqueue_Null_InvalidOperationException()
        {
            UniqueQueue<string> queue = new UniqueQueue<string>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                queue.Enqueue(null);
            });
        }

        [Fact]
        public void Dequeue_QueuedString_Dequeued()
        {
            UniqueQueue<string> queue = new UniqueQueue<string>();

            queue.Enqueue("abc");

            Assert.Equal("abc", queue.Dequeue());
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void Dequeue_WhileEmpty_InvalidOperationException()
        {
            UniqueQueue<string> queue = new UniqueQueue<string>();

            Assert.Throws<InvalidOperationException>(() =>
            {
                queue.Dequeue();
            });
        }

        [Fact]
        public void TryDequeue_WithOneQueued_DequeueSuccessful()
        {
            UniqueQueue<string> queue = new UniqueQueue<string>();

            queue.Enqueue("abc");

            string str;
            bool result = queue.TryDequeue(out str);

            Assert.Equal("abc", str);
            Assert.True(result);
            Assert.Equal(0, queue.Count);
        }

        [Fact]
        public void TryDequeue_WithNoneQueued_ReturnedFalse()
        {
            UniqueQueue<string> queue = new UniqueQueue<string>();

            string str;
            bool result = queue.TryDequeue(out str);

            Assert.Null(str);
            Assert.False(result);
        }

    }
}
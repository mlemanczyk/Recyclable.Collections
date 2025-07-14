using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
    public class RecyclablePriorityQueueTests
    {
        private static readonly int[] _testData = new[] { 4, 1, 3, 2, 5 };

        [Fact]
        public void DequeueShouldReturnItemsInSortedOrder()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            foreach (var item in _testData)
            {
                queue.Enqueue(item);
            }

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().BeInAscendingOrder();
        }

        [Fact]
        public void ShouldBeEmptyWhenNotInitialized()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            _ = queue.Should().BeEmpty();
            _ = queue.LongCount.Should().Be(0);
        }

        [Fact]
        public void ShouldBeEmptyAfterClear()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            foreach (var item in _testData)
            {
                queue.Enqueue(item);
            }

            queue.Clear();
            _ = queue.Should().BeEmpty();
        }

        [Fact]
        public void DequeueShouldThrowWhenNoElements()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => queue.Dequeue());
        }
    }
}

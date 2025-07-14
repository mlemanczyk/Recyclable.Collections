using FluentAssertions;
using Recyclable.Collections;
using System.Linq;

namespace Recyclable.CollectionsTests
{
    public class RecyclableLinkedListTests
    {
        private static readonly string[] _testData = new[] { "a", "d", "c", "b", "a" };

        [Fact]
        public void AddLastShouldPreserveOrder()
        {
            using var list = new RecyclableLinkedList<string>();
            foreach (var item in _testData)
            {
                list.AddLast(item);
            }

            var actual = new RecyclableLongList<string>();
            foreach (var item in list)
            {
                actual.Add(item);
            }

            _ = actual.Should().ContainInConsecutiveOrder(_testData)
                .And.BeEquivalentTo(_testData);
        }

        [Fact]
        public void AddFirstShouldReverseOrder()
        {
            using var list = new RecyclableLinkedList<string>();
            foreach (var item in _testData)
            {
                list.AddFirst(item);
            }

            var expected = _testData.Reverse().ToArray();
            var actual = new RecyclableLongList<string>();
            foreach (var item in list)
            {
                actual.Add(item);
            }

            _ = actual.Should().ContainInConsecutiveOrder(expected);
        }

        [Fact]
        public void RemoveFirstShouldReturnItemsInOrder()
        {
            using var list = new RecyclableLinkedList<string>(_testData);
            var actual = new RecyclableLongList<string>();
            while (list.LongCount > 0)
            {
                actual.Add(list.RemoveFirst());
            }

            _ = actual.Should().ContainInConsecutiveOrder(_testData);
        }

        [Fact]
        public void RemoveLastShouldReturnItemsInReverseOrder()
        {
            using var list = new RecyclableLinkedList<string>(_testData);
            var actual = new RecyclableLongList<string>();
            while (list.LongCount > 0)
            {
                actual.Add(list.RemoveLast());
            }

            _ = actual.Should().ContainInConsecutiveOrder(_testData.Reverse());
        }
    }
}

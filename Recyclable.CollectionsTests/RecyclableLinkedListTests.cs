using FluentAssertions;
using Recyclable.Collections;
using System.Linq;

namespace Recyclable.CollectionsTests
{
    public class RecyclableLinkedListTests
    {
        private static readonly string[] _testData = new[] { "a", "d", "c", "b", "a" };

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddLastShouldPreserveOrder(int itemsCount)
        {
            using var list = new RecyclableLinkedList<long>();
            var expected = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            foreach (var item in expected)
            {
                list.AddLast(item);
            }

            var actual = new RecyclableLongList<long>();
            foreach (var item in list)
            {
                actual.Add(item);
            }

            _ = actual.Should().ContainInConsecutiveOrder(expected)
                .And.BeEquivalentTo(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddFirstShouldReverseOrder(int itemsCount)
        {
            using var list = new RecyclableLinkedList<long>();
            var source = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            foreach (var item in source)
            {
                list.AddFirst(item);
            }

            var expected = source.Reverse().ToArray();
            var actual = new RecyclableLongList<long>();
            foreach (var item in list)
            {
                actual.Add(item);
            }

            _ = actual.Should().ContainInConsecutiveOrder(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void RemoveFirstShouldReturnItemsInOrder(int itemsCount)
        {
            var source = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var list = new RecyclableLinkedList<long>(source);
            var actual = new RecyclableLongList<long>();
            while (list.LongCount > 0)
            {
                actual.Add(list.RemoveFirst());
            }

            _ = actual.Should().ContainInConsecutiveOrder(source);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void RemoveLastShouldReturnItemsInReverseOrder(int itemsCount)
        {
            var source = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var list = new RecyclableLinkedList<long>(source);
            var actual = new RecyclableLongList<long>();
            while (list.LongCount > 0)
            {
                actual.Add(list.RemoveLast());
            }

            _ = actual.Should().ContainInConsecutiveOrder(source.Reverse());
        }
    }
}

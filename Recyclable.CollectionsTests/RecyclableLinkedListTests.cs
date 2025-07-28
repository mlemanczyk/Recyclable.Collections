using FluentAssertions;
using Recyclable.Collections;
using System.Linq;
using System.Collections;

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

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddListItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableList<long>(data);
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromList(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableList<long>(data);
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);
            list.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddLongListItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableLongList<long>(data);
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromLongList(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableLongList<long>(data);
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);
            list.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddHashSetItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableHashSet<long>();
            foreach (long item in data)
            {
                source.Add(item);
            }
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromHashSet(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableHashSet<long>();
            foreach (long item in data)
            {
                source.Add(item);
            }
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);
            list.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddStackItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableStack<long>(data);
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromStack(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableStack<long>(data);
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);
            list.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddSortedSetItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableSortedSet<long>();
            foreach (long item in data)
            {
                source.Add(item);
            }
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromSortedSet(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableSortedSet<long>();
            foreach (long item in data)
            {
                source.Add(item);
            }
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);
            list.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddLinkedListItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableLinkedList<long>(data);
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromLinkedList(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableLinkedList<long>(data);
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);
            list.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddPriorityQueueItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclablePriorityQueue<long>();
            foreach (long item in data)
            {
                source.Enqueue(item);
            }
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromPriorityQueue(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclablePriorityQueue<long>();
            foreach (long item in data)
            {
                source.Enqueue(item);
            }
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);
            list.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }


        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddQueueItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableQueue<long>(data);
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromQueue(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableQueue<long>(data);
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(source);
            list.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }


        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddDictionaryItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableDictionary<int, long>();
            for (int i = 0; i < data.Length; i++)
            {
                source.Add(i, data[i]);
            }
            using var list = new RecyclableLinkedList<KeyValuePair<int, long>>();

            list.AddRange(source);

            List<KeyValuePair<int, long>> expected = new(itemsCount);
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add(new KeyValuePair<int, long>(i, data[i]));
            }

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromDictionary(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableDictionary<int, long>();
            for (int i = 0; i < data.Length; i++)
            {
                source.Add(i, data[i]);
            }
            using var list = new RecyclableLinkedList<KeyValuePair<int, long>>();

            list.AddRange(source);
            list.AddRange(source);

            List<KeyValuePair<int, long>> expected = new(itemsCount * 2);
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add(new KeyValuePair<int, long>(i, data[i]));
            }
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add(new KeyValuePair<int, long>(i, data[i]));
            }

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddSortedListItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableSortedList<int, long>();
            for (int i = 0; i < data.Length; i++)
            {
                source.Add(i, data[i]);
            }
            using var list = new RecyclableLinkedList<(int Key, long Value)>();

            list.AddRange(source);

            List<(int Key, long Value)> expected = new(itemsCount);
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add((i, data[i]));
            }

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromSortedList(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableSortedList<int, long>();
            for (int i = 0; i < data.Length; i++)
            {
                source.Add(i, data[i]);
            }
            using var list = new RecyclableLinkedList<(int Key, long Value)>();

            list.AddRange(source);
            list.AddRange(source);

            List<(int Key, long Value)> expected = new(itemsCount * 2);
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add((i, data[i]));
            }
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add((i, data[i]));
            }

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddSortedDictionaryItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableSortedDictionary<int, long>();
            for (int i = 0; i < data.Length; i++)
            {
                source.Add(i, data[i]);
            }
            using var list = new RecyclableLinkedList<KeyValuePair<int, long>>();

            list.AddRange(source);

            List<KeyValuePair<int, long>> expected = new(itemsCount);
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add(new KeyValuePair<int, long>(i, data[i]));
            }

            _ = list.LongCount.Should().Be(itemsCount);
            _ = list.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromSortedDictionary(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableSortedDictionary<int, long>();
            for (int i = 0; i < data.Length; i++)
            {
                source.Add(i, data[i]);
            }
            using var list = new RecyclableLinkedList<KeyValuePair<int, long>>();

            list.AddRange(source);
            list.AddRange(source);

            List<KeyValuePair<int, long>> expected = new(itemsCount * 2);
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add(new KeyValuePair<int, long>(i, data[i]));
            }
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add(new KeyValuePair<int, long>(i, data[i]));
            }

            _ = list.LongCount.Should().Be(itemsCount * 2);
            _ = list.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeShouldAddArrayItems()
        {
            long[] data = { 1, 2, 3 };
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(data);

            _ = list.Should().Equal(data);
        }

        [Fact]
        public void AddRangeShouldAddCollectionItems()
        {
            ICollection collection = new long[] { 1, 2, 3 };
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(collection);

            _ = list.Should().Equal(collection.Cast<long>());
        }

        [Fact]
        public void AddRangeShouldAddEnumerableItems()
        {
            IEnumerable<long> enumerable = new long[] { 1, 2, 3 };
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(enumerable);

            _ = list.Should().Equal(enumerable);
        }

        [Fact]
        public void AddRangeShouldAddSpanItems()
        {
            long[] data = { 1, 2, 3 };
            Span<long> span = data;
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(span);

            _ = list.Should().Equal(data);
        }

        [Fact]
        public void AddRangeShouldAddListItems()
        {
            var data = new List<long> { 1, 2, 3 };
            using var list = new RecyclableLinkedList<long>();

            list.AddRange(data);

            _ = list.Should().Equal(data);
        }
    }
}

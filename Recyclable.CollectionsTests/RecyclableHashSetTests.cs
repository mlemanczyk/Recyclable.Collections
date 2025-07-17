using FluentAssertions;
using Recyclable.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Recyclable.CollectionsTests
{
    public class RecyclableHashSetTests
    {
        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddShouldStoreUniqueItems(int itemsCount)
        {
            using var set = new RecyclableHashSet<long>();
            foreach (var item in RecyclableLongListTestData.CreateTestData(itemsCount))
            {
                set.Add(item).Should().BeTrue();
            }

            foreach (var item in RecyclableLongListTestData.CreateTestData(itemsCount))
            {
                set.Add(item).Should().BeFalse();
            }

            _ = set.Should().HaveCount(itemsCount);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void RemoveShouldDeleteItem(int itemsCount)
        {
            using var set = new RecyclableHashSet<long>();
            foreach (var item in RecyclableLongListTestData.CreateTestData(itemsCount))
            {
                set.Add(item);
            }

            if (itemsCount > 0)
            {
                long value = (itemsCount + 1) / 2;
                set.Remove(value).Should().BeTrue();
                _ = set.Contains(value).Should().BeFalse();
                _ = set.Should().HaveCount(itemsCount - 1);
            }
            else
            {
                set.Remove(0).Should().BeFalse();
                _ = set.Should().BeEmpty();
            }
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void EnumeratorShouldYieldItems(int itemsCount)
        {
            using var set = new RecyclableHashSet<long>();
            foreach (var item in RecyclableLongListTestData.CreateTestData(itemsCount))
            {
                set.Add(item);
            }

            var actual = new List<long>();
            var enumerator = set.GetEnumerator();
            while (enumerator.MoveNext())
            {
                actual.Add(enumerator.Current);
            }

            var expected = RecyclableLongListTestData.CreateTestData(itemsCount).ToList();
            if (itemsCount == 0)
            {
                _ = actual.Should().BeEmpty();
            }
            else
            {
                _ = actual.Should().Contain(expected).And.HaveCount(itemsCount);
            }
        }

        [Fact]
        public void AddRangeShouldAddItemsFromSpan()
        {
            using var set = new RecyclableHashSet<int>();
            var data = new[] { 1, 2, 3 };

            set.AddRange(data);

            _ = set.Count.Should().Be(3);
            _ = set.Should().ContainInOrder(data);
        }

        [Fact]
        public void AddRangeShouldIgnoreDuplicates()
        {
            using var set = new RecyclableHashSet<int>();
            var data = new[] { 1, 2, 3 };

            set.AddRange(data);
            set.AddRange(data);

            _ = set.Count.Should().Be(3);
            _ = set.Should().ContainInOrder(data);
        }
        [Fact]
        public void AddRangeShouldAddItemsFromList()
        {
            using var set = new RecyclableHashSet<int>();
            var list = new List<int> { 1, 2, 3 };

            set.AddRange(list);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromRecyclableList()
        {
            using var set = new RecyclableHashSet<int>();
            using var list = new RecyclableList<int>(new[] { 1, 2, 3 });

            set.AddRange(list);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromLongList()
        {
            using var set = new RecyclableHashSet<int>();
            using var list = new RecyclableLongList<int>(new[] { 1, 2, 3 });

            set.AddRange(list);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromStack()
        {
            using var set = new RecyclableHashSet<int>();
            using var stack = new RecyclableStack<int>(new[] { 1, 2, 3 });

            set.AddRange(stack);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromQueue()
        {
            using var set = new RecyclableHashSet<int>();
            using var queue = new RecyclableQueue<int>(new[] { 1, 2, 3 });

            set.AddRange(queue);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromArray()
        {
            using var set = new RecyclableHashSet<int>();
            Array array = new int[] { 1, 2, 3 };

            set.AddRange(array);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromICollection()
        {
            using var set = new RecyclableHashSet<int>();
            System.Collections.ArrayList list = new System.Collections.ArrayList { 1, 2, 3 };

            set.AddRange(list);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromGenericICollection()
        {
            using var set = new RecyclableHashSet<int>();
            ICollection<int> list = new List<int> { 1, 2, 3 };

            set.AddRange(list);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromSortedSet()
        {
            using var source = new RecyclableSortedSet<int>();
            source.Add(1);
            source.Add(2);
            source.Add(3);
            using var set = new RecyclableHashSet<int>();

            set.AddRange(source);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromLinkedList()
        {
            using var source = new RecyclableLinkedList<int>(new[] { 1, 2, 3 });
            using var set = new RecyclableHashSet<int>();

            set.AddRange(source);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromPriorityQueue()
        {
            using var source = new RecyclablePriorityQueue<int>();
            source.Enqueue(1);
            source.Enqueue(2);
            source.Enqueue(3);
            using var set = new RecyclableHashSet<int>();

            set.AddRange(source);

            _ = set.Count.Should().Be(3);
            _ = set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromDictionary()
        {
            using var source = new RecyclableDictionary<int, string>();
            source.Add(1, "a");
            source.Add(2, "b");
            using var set = new RecyclableHashSet<KeyValuePair<int, string>>();

            set.AddRange(source);

            _ = set.Count.Should().Be(2);
            set.Should().Contain(new[]
            {
                new KeyValuePair<int, string>(1, "a"),
                new KeyValuePair<int, string>(2, "b")
            });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromSortedDictionary()
        {
            using var source = new RecyclableSortedDictionary<int, string>();
            source.Add(1, "a");
            source.Add(2, "b");
            using var set = new RecyclableHashSet<KeyValuePair<int, string>>();

            set.AddRange(source);

            _ = set.Count.Should().Be(2);
            set.Should().Contain(new[]
            {
                new KeyValuePair<int, string>(1, "a"),
                new KeyValuePair<int, string>(2, "b")
            });
        }

        [Fact]
        public void AddRangeShouldAddItemsFromSortedList()
        {
            using var source = new RecyclableSortedList<int, string>();
            source.Add(1, "a");
            source.Add(2, "b");
            using var set = new RecyclableHashSet<(int Key, string Value)>();

            set.AddRange(source);

            _ = set.Count.Should().Be(2);
            set.Should().Contain(new[] { (1, "a"), (2, "b") });
        }

    }
}

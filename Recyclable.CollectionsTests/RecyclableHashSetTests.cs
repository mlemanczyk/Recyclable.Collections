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

    }
}

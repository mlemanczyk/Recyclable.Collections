using FluentAssertions;
using Recyclable.Collections;
using System.Linq;

namespace Recyclable.CollectionsTests
{
    public class RecyclableSortedListTests
    {
        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddShouldStoreItemsSorted(int itemsCount)
        {
            using var list = new RecyclableSortedList<long, long>();
            foreach (var key in RecyclableLongListTestData.CreateTestData(itemsCount).Reverse())
            {
                list.Add(key, -key);
            }

            _ = list.Should().HaveCount(itemsCount)
                .And.BeInAscendingOrder(x => x.Key);
            for (int i = 0; i < itemsCount; i++)
            {
                long expectedKey = i + 1;
                list.GetKey(i).Should().Be(expectedKey);
                list.GetValue(i).Should().Be(-expectedKey);
            }
        }

        [Fact]
        public void IndexerShouldUpdateValue()
        {
            using var list = new RecyclableSortedList<int, string>();
            list.Add(1, "a");

            list[1] = "b";

            _ = list[1].Should().Be("b");
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void RemoveShouldDeleteItem(int itemsCount)
        {
            using var list = new RecyclableSortedList<long, long>();
            foreach (var key in RecyclableLongListTestData.CreateTestData(itemsCount))
            {
                list.Add(key, key);
            }

            if (itemsCount > 0)
            {
                long keyToRemove = (itemsCount + 1) / 2;
                list.Remove(keyToRemove).Should().BeTrue();
                _ = list.ContainsKey(keyToRemove).Should().BeFalse();
                _ = list.Should().HaveCount(itemsCount - 1);
            }
            else
            {
                list.Remove(0).Should().BeFalse();
                _ = list.Should().BeEmpty();
            }
        }

        [Fact]
        public void TryGetValueShouldReturnValue()
        {
            using var list = new RecyclableSortedList<int, string>();
            list.Add(1, "a");

            var found = list.TryGetValue(1, out var value);

            _ = found.Should().BeTrue();
            _ = value.Should().Be("a");
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void EnumeratorShouldYieldItemsSorted(int itemsCount)
        {
            using var list = new RecyclableSortedList<long, long>();
            foreach (var key in RecyclableLongListTestData.CreateTestData(itemsCount).Reverse())
            {
                list.Add(key, key);
            }

            var actual = new List<long>();
            var enumerator = list.GetEnumerator();
            while (enumerator.MoveNext())
            {
                actual.Add(enumerator.Current.Key);
            }

            var expected = RecyclableLongListTestData.CreateTestData(itemsCount).ToList();
            _ = actual.Should().ContainInConsecutiveOrder(expected);
        }
    }
}

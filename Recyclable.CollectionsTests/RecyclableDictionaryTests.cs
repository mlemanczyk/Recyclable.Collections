using FluentAssertions;
using Recyclable.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Recyclable.CollectionsTests
{
    public class RecyclableDictionaryTests
    {
        private static readonly (int, string)[] _testData = new[] { (1, "a"), (2, "b"), (3, "c"), (4, "d"), (5, "e") };

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddShouldStoreItems(int itemsCount)
        {
            using var dict = new RecyclableDictionary<long, long>();
            foreach (var key in RecyclableLongListTestData.CreateTestData(itemsCount))
            {
                dict.Add(key, -key);
            }

            _ = dict.Should().HaveCount(itemsCount);
            for (var i = 0; i < itemsCount; i++)
            {
                long expectedKey = i + 1;
                dict.GetKey(i).Should().Be(expectedKey);
                dict.GetValue(i).Should().Be(-expectedKey);
            }
        }

        [Fact]
        public void IndexerShouldUpdateValue()
        {
            using var dict = new RecyclableDictionary<int, string>();
            dict.Add(1, "a");

            dict[1] = "b";

            _ = dict[1].Should().Be("b");
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void RemoveShouldSwapWithLast(int itemsCount)
        {
            using var dict = new RecyclableDictionary<long, long>();
            foreach (var key in RecyclableLongListTestData.CreateTestData(itemsCount))
            {
                dict.Add(key, key);
            }

            if (itemsCount > 0)
            {
                long keyToRemove = (itemsCount + 1) / 2;
                dict.Remove(keyToRemove).Should().BeTrue();
                _ = dict.ContainsKey(keyToRemove).Should().BeFalse();
                _ = dict.Should().HaveCount(itemsCount - 1);
            }
            else
            {
                dict.Remove(0).Should().BeFalse();
                _ = dict.Should().BeEmpty();
            }
        }

        [Fact]
        public void TryGetValueShouldReturnValue()
        {
            using var dict = new RecyclableDictionary<int, string>();
            dict.Add(1, "a");

            var found = dict.TryGetValue(1, out var value);

            _ = found.Should().BeTrue();
            _ = value.Should().Be("a");
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void EnumeratorShouldYieldItems(int itemsCount)
        {
            using var dict = new RecyclableDictionary<long, long>();
            foreach (var key in RecyclableLongListTestData.CreateTestData(itemsCount))
            {
                dict.Add(key, key);
            }

            var actual = new List<long>();
            var enumerator = dict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                actual.Add(enumerator.Current.Key);
            }

            var expected = RecyclableLongListTestData.CreateTestData(itemsCount).ToList();
            _ = actual.Should().ContainInConsecutiveOrder(expected);
        }
    }
}

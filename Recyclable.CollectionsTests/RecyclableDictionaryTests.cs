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

        [Fact]
        public void AddRangeShouldAddItemsFromSpan()
        {
            using var dict = new RecyclableDictionary<int, string>();
            var data = new[]
            {
                new KeyValuePair<int, string>(1, "a"),
                new KeyValuePair<int, string>(2, "b"),
                new KeyValuePair<int, string>(3, "c"),
            };

            dict.AddRange(data);

            _ = dict.Count.Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                dict.GetKey(i).Should().Be(i + 1);
                dict.GetValue(i).Should().Be(data[i].Value);
            }
        }

        [Fact]
        public void AddRangeShouldAddItemsFromDictionary()
        {
            using var source = new RecyclableDictionary<int, string>();
            source.Add(1, "a");
            source.Add(2, "b");
            using var dict = new RecyclableDictionary<int, string>();

            dict.AddRange(source);

            _ = dict.Count.Should().Be(2);
            dict.GetKey(0).Should().Be(1);
            dict.GetValue(0).Should().Be("a");
            dict.GetKey(1).Should().Be(2);
            dict.GetValue(1).Should().Be("b");
        }

        [Fact]
        public void AddRangeShouldAddListItemsInCorrectOrder()
        {
            var data = new[]
            {
                new KeyValuePair<int, string>(1, "a"),
                new KeyValuePair<int, string>(2, "b"),
                new KeyValuePair<int, string>(3, "c"),
            };

            using var source = new RecyclableList<KeyValuePair<int, string>>(data);
            using var dict = new RecyclableDictionary<int, string>();

            dict.AddRange(source);

            _ = dict.Count.Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                dict.GetKey(i).Should().Be(data[i].Key);
                dict.GetValue(i).Should().Be(data[i].Value);
            }
        }

        [Fact]
        public void AddRangeShouldAddLongListItemsInCorrectOrder()
        {
            var data = new[]
            {
                new KeyValuePair<int, string>(1, "a"),
                new KeyValuePair<int, string>(2, "b"),
                new KeyValuePair<int, string>(3, "c"),
            };

            using var source = new RecyclableLongList<KeyValuePair<int, string>>(data);
            using var dict = new RecyclableDictionary<int, string>();

            dict.AddRange(source);

            _ = dict.Count.Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                dict.GetKey(i).Should().Be(data[i].Key);
                dict.GetValue(i).Should().Be(data[i].Value);
            }
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
            using var dict = new RecyclableDictionary<int, long>();

            dict.AddRange(source);

            _ = dict.Count.Should().Be(itemsCount);
            for (int i = 0; i < data.Length; i++)
            {
                dict.GetKey(i).Should().Be(i);
                dict.GetValue(i).Should().Be(data[i]);
            }
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
            using var dict = new RecyclableDictionary<int, long>();

            dict.AddRange(source);

            _ = dict.Count.Should().Be(itemsCount);
            for (int i = 0; i < data.Length; i++)
            {
                dict.GetKey(i).Should().Be(i);
                dict.GetValue(i).Should().Be(data[i]);
            }
        }

        [Fact]
        public void AddRangeShouldAddQueueItemsInCorrectOrder()
        {
            var data = new[]
            {
                new KeyValuePair<int, string>(1, "a"),
                new KeyValuePair<int, string>(2, "b"),
                new KeyValuePair<int, string>(3, "c"),
            };

            using var source = new RecyclableQueue<KeyValuePair<int, string>>(data);
            using var dict = new RecyclableDictionary<int, string>();

            dict.AddRange(source);

            _ = dict.Count.Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                dict.GetKey(i).Should().Be(data[i].Key);
                dict.GetValue(i).Should().Be(data[i].Value);
            }
        }

        [Fact]
        public void AddRangeShouldAddStackItemsInCorrectOrder()
        {
            var data = new[]
            {
                new KeyValuePair<int, string>(1, "a"),
                new KeyValuePair<int, string>(2, "b"),
                new KeyValuePair<int, string>(3, "c"),
            };

            using var source = new RecyclableStack<KeyValuePair<int, string>>(data);
            using var dict = new RecyclableDictionary<int, string>();

            dict.AddRange(source);

            _ = dict.Count.Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                dict.GetKey(i).Should().Be(data[i].Key);
                dict.GetValue(i).Should().Be(data[i].Value);
            }
        }

        [Fact]
        public void AddRangeShouldAddLinkedListItemsInCorrectOrder()
        {
            var data = new[]
            {
                new KeyValuePair<int, string>(1, "a"),
                new KeyValuePair<int, string>(2, "b"),
                new KeyValuePair<int, string>(3, "c"),
            };

            using var source = new RecyclableLinkedList<KeyValuePair<int, string>>(data);
            using var dict = new RecyclableDictionary<int, string>();

            dict.AddRange(source);

            _ = dict.Count.Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                dict.GetKey(i).Should().Be(data[i].Key);
                dict.GetValue(i).Should().Be(data[i].Value);
            }
        }

        [Fact]
        public void AddRangeShouldAddPriorityQueueItemsInCorrectOrder()
        {
            var data = new[]
            {
                new KeyValuePair<int, string>(1, "a"),
                new KeyValuePair<int, string>(2, "b"),
                new KeyValuePair<int, string>(3, "c"),
            };

            using var source = new RecyclablePriorityQueue<KeyValuePair<int, string>>(comparer: Comparer<KeyValuePair<int, string>>.Create((x, y) => x.Key.CompareTo(y.Key)));
            foreach (var kvp in data)
            {
                source.Enqueue(kvp);
            }
            using var dict = new RecyclableDictionary<int, string>();

            dict.AddRange(source);

            _ = dict.Count.Should().Be(3);
            for (int i = 0; i < 3; i++)
            {
                dict.GetKey(i).Should().Be(data[i].Key);
                dict.GetValue(i).Should().Be(data[i].Value);
            }
        }
    }
}

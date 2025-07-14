using FluentAssertions;
using Recyclable.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Recyclable.CollectionsTests
{
    public class RecyclableSortedDictionaryTests
    {
        [Fact]
        public void AddShouldStoreUniqueItemsInOrder()
        {
            using var dict = new RecyclableSortedDictionary<int, int>();
            var values = new[] { 4, 1, 3, 2, 5 };
            foreach (var v in values)
            {
                dict.Add(v, v);
            }

            var result = new List<int>();
            foreach (var kv in dict)
            {
                result.Add(kv.Key);
            }

            _ = result.Should().Equal(values.OrderBy(x => x));
        }

        [Fact]
        public void IndexerShouldUpdateValue()
        {
            using var dict = new RecyclableSortedDictionary<int, string>();
            dict.Add(1, "a");

            dict[1] = "b";

            _ = dict[1].Should().Be("b");
        }

        [Fact]
        public void RemoveShouldDeleteItem()
        {
            using var dict = new RecyclableSortedDictionary<int, string>();
            var values = new[] { 4, 1, 3, 2, 5 };
            foreach (var v in values)
            {
                dict.Add(v, v.ToString());
            }

            dict.Remove(3).Should().BeTrue();
            _ = dict.ContainsKey(3).Should().BeFalse();
            _ = dict.Count.Should().Be(values.Length - 1);
        }

        [Fact]
        public void EnumeratorShouldYieldItemsInOrder()
        {
            using var dict = new RecyclableSortedDictionary<int, int>();
            foreach (var v in new[] { 3, 1, 4, 2 })
            {
                dict.Add(v, v);
            }

            var result = new List<int>();
            foreach (var item in dict)
            {
                result.Add(item.Key);
            }

            _ = result.Should().Equal(new[] { 1, 2, 3, 4 });
        }

        [Fact]
        public void TryGetValueShouldReturnValue()
        {
            using var dict = new RecyclableSortedDictionary<int, string>();
            dict.Add(1, "a");

            var found = dict.TryGetValue(1, out var value);

            _ = found.Should().BeTrue();
            _ = value.Should().Be("a");
        }

        [Fact]
        public void ShouldBeEmptyAfterClear()
        {
            using var dict = new RecyclableSortedDictionary<int, int>();
            dict.Add(1, 1);
            dict.Add(2, 2);

            dict.Clear();
            _ = dict.Should().BeEmpty();
        }
    }
}

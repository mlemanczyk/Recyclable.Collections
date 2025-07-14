using FluentAssertions;
using Recyclable.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Recyclable.CollectionsTests
{
    public class RecyclableDictionaryTests
    {
        private static readonly (int, string)[] _testData = new[] { (1, "a"), (2, "b"), (3, "c"), (4, "d"), (5, "e") };

        [Fact]
        public void AddShouldStoreItems()
        {
            using var dict = new RecyclableDictionary<int, string>();
            foreach (var (key, value) in _testData)
            {
                dict.Add(key, value);
            }

            _ = dict.Should().HaveCount(_testData.Length);
            for (var i = 0; i < _testData.Length; i++)
            {
                dict.GetKey(i).Should().Be(_testData[i].Item1);
                dict.GetValue(i).Should().Be(_testData[i].Item2);
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

        [Fact]
        public void RemoveShouldSwapWithLast()
        {
            using var dict = new RecyclableDictionary<int, string>();
            foreach (var (key, value) in _testData)
            {
                dict.Add(key, value);
            }

            dict.Remove(3).Should().BeTrue();
            _ = dict.ContainsKey(3).Should().BeFalse();
            _ = dict.Should().HaveCount(_testData.Length - 1);
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

        [Fact]
        public void EnumeratorShouldYieldItems()
        {
            using var dict = new RecyclableDictionary<int, string>();
            foreach (var (key, value) in _testData)
            {
                dict.Add(key, value);
            }

            var actual = new List<KeyValuePair<int, string>>();
            var enumerator = dict.GetEnumerator();
            while (enumerator.MoveNext())
            {
                actual.Add(enumerator.Current);
            }

            _ = actual.Should().ContainInConsecutiveOrder(_testData.Select(x => new KeyValuePair<int, string>(x.Item1, x.Item2)));
        }
    }
}

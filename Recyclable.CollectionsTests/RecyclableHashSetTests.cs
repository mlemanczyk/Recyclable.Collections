using FluentAssertions;
using Recyclable.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Recyclable.CollectionsTests
{
    public class RecyclableHashSetTests
    {
        [Fact]
        public void AddShouldStoreUniqueItems()
        {
            using var set = new RecyclableHashSet<string>();
            set.Add("a").Should().BeTrue();
            set.Add("b").Should().BeTrue();
            set.Add("a").Should().BeFalse();

            _ = set.Should().HaveCount(2);
            _ = set.Contains("a").Should().BeTrue();
            _ = set.Contains("b").Should().BeTrue();
        }

        [Fact]
        public void RemoveShouldDeleteItem()
        {
            using var set = new RecyclableHashSet<int>();
            set.Add(1);
            set.Add(2);

            set.Remove(1).Should().BeTrue();

            _ = set.Contains(1).Should().BeFalse();
            _ = set.Should().HaveCount(1);
        }

        [Fact]
        public void EnumeratorShouldYieldItems()
        {
            using var set = new RecyclableHashSet<int>();
            for (int i = 0; i < 5; i++)
            {
                set.Add(i);
            }

            var actual = new List<int>();
            var enumerator = set.GetEnumerator();
            while (enumerator.MoveNext())
            {
                actual.Add(enumerator.Current);
            }

            _ = actual.Should().Contain(Enumerable.Range(0, 5)).And.HaveCount(5);
        }
    }
}

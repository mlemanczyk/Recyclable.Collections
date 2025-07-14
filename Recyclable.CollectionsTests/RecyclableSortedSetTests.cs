using FluentAssertions;
using Recyclable.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Recyclable.CollectionsTests
{
    public class RecyclableSortedSetTests
    {
        [Fact]
        public void AddShouldStoreUniqueItemsInOrder()
        {
            using var set = new RecyclableSortedSet<int>();
            var values = new[] { 4, 1, 3, 2, 5 };
            foreach (var v in values)
            {
                set.Add(v).Should().BeTrue();
            }

            set.Add(3).Should().BeFalse();
            _ = set.Should().Equal(values.OrderBy(x => x));
        }

        [Fact]
        public void RemoveShouldDeleteItem()
        {
            using var set = new RecyclableSortedSet<int>();
            var values = new[] { 4, 1, 3, 2, 5 };
            foreach (var v in values)
            {
                set.Add(v);
            }

            set.Remove(3).Should().BeTrue();
            _ = set.Contains(3).Should().BeFalse();
            _ = set.Should().Equal(values.Where(x => x != 3).OrderBy(x => x));
        }

        [Fact]
        public void EnumeratorShouldYieldItemsInOrder()
        {
            using var set = new RecyclableSortedSet<int>();
            foreach (var v in new[] { 3, 1, 4, 2 })
            {
                set.Add(v);
            }

            var result = new List<int>();
            foreach (var item in set)
            {
                result.Add(item);
            }

            _ = result.Should().Equal(new[] { 1, 2, 3, 4 });
        }

        [Fact]
        public void ShouldBeEmptyAfterClear()
        {
            using var set = new RecyclableSortedSet<int>();
            set.Add(1);
            set.Add(2);

            set.Clear();
            _ = set.Should().BeEmpty();
        }
    }
}

using FluentAssertions;
using Recyclable.Collections;
using System.Collections;

namespace Recyclable.CollectionsTests
{
    public class RecyclablePriorityQueueTests
    {
        private static readonly int[] _testData = new[] { 4, 1, 3, 2, 5 };

        [Fact]
        public void DequeueShouldReturnItemsInSortedOrder()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            foreach (var item in _testData)
            {
                queue.Enqueue(item);
            }

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().BeInAscendingOrder();
        }

        [Fact]
        public void ShouldBeEmptyWhenNotInitialized()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            _ = queue.Should().BeEmpty();
            _ = queue.LongCount.Should().Be(0);
        }

        [Fact]
        public void ShouldBeEmptyAfterClear()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            foreach (var item in _testData)
            {
                queue.Enqueue(item);
            }

            queue.Clear();
            _ = queue.Should().BeEmpty();
        }

        [Fact]
        public void DequeueShouldThrowWhenNoElements()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            _ = Assert.Throws<ArgumentOutOfRangeException>(() => queue.Dequeue());
        }

        [Fact]
        public void AddRangeSpanShouldAddItemsInSortedOrder()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            queue.AddRange((ReadOnlySpan<int>)_testData);

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(_testData.OrderBy(static value => value));
        }

        [Fact]
        public void AddRangeSpanShouldDoNothingWhenEmpty()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            queue.AddRange(ReadOnlySpan<int>.Empty);
            _ = queue.Should().BeEmpty();
        }

        [Fact]
        public void AddRangeSpanShouldNotOverrideItems()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            queue.AddRange((ReadOnlySpan<int>)_testData);
            queue.AddRange((ReadOnlySpan<int>)_testData);

            var expected = _testData.Concat(_testData).OrderBy(static value => value).ToArray();
            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeArrayShouldAddItemsInSortedOrder()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            queue.AddRange(_testData);

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(_testData.OrderBy(static value => value));
        }

        [Fact]
        public void AddRangeListShouldAddItemsInSortedOrder()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            queue.AddRange(new List<int>(_testData));

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(_testData.OrderBy(static value => value));
        }

        [Fact]
        public void AddRangeReadOnlyMemoryShouldAddItemsInSortedOrder()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            queue.AddRange((ReadOnlyMemory<int>)_testData);

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(_testData.OrderBy(static value => value));
        }

        [Fact]
        public void AddRangeMemoryShouldNotOverrideItems()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            queue.AddRange(_testData.AsMemory());
            queue.AddRange(_testData.AsMemory());

            var expected = _testData.Concat(_testData).OrderBy(static value => value).ToArray();
            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeArrayShouldNotOverrideItems()
        {
            using var queue = new RecyclablePriorityQueue<int>();
            queue.AddRange(_testData);
            queue.AddRange(_testData);

            var expected = _testData.Concat(_testData).OrderBy(static value => value).ToArray();
            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeShouldAcceptNulls()
        {
            using var queue = new RecyclablePriorityQueue<long?>();
            queue.AddRange(new long?[] { null, 1 });
            _ = queue.Should().HaveCount(2).And.Contain((long?)null);
        }

        [Fact]
        public void AddRangeQueueShouldAddItemsInSortedOrder()
        {
            using var source = new RecyclableQueue<int>(_testData);
            using var queue = new RecyclablePriorityQueue<int>();

            queue.AddRange(source);

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(_testData.OrderBy(static value => value));
        }

        [Fact]
        public void AddRangeQueueShouldNotOverrideItems()
        {
            using var source = new RecyclableQueue<int>(_testData);
            using var queue = new RecyclablePriorityQueue<int>();

            queue.AddRange(source);
            queue.AddRange(source);

            var expected = _testData.Concat(_testData).OrderBy(static value => value).ToArray();
            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeStackShouldAddItemsInSortedOrder()
        {
            using var source = new RecyclableStack<int>(_testData);
            using var queue = new RecyclablePriorityQueue<int>();

            queue.AddRange(source);

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(_testData.OrderBy(static value => value));
        }

        [Fact]
        public void AddRangeSortedSetShouldAddItemsInSortedOrder()
        {
            using var source = new RecyclableSortedSet<int>();
            foreach (int item in _testData)
            {
                source.Add(item);
            }

            using var queue = new RecyclablePriorityQueue<int>();

            queue.AddRange(source);

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(_testData.OrderBy(static value => value));
        }

        [Fact]
        public void AddRangeDictionaryShouldAddItemsInSortedOrder()
        {
            using var source = new RecyclableDictionary<int, int>();
            for (int i = 0; i < _testData.Length; i++)
            {
                source.Add(i, _testData[i]);
            }

            using var queue = new RecyclablePriorityQueue<KeyValuePair<int, int>>(comparer: Comparer<KeyValuePair<int, int>>.Create((a, b) => a.Key.CompareTo(b.Key)));

            queue.AddRange(source);

            var result = new List<KeyValuePair<int, int>>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            var expected = _testData.Select((v, i) => new KeyValuePair<int, int>(i, v)).ToList();
            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeDictionaryShouldNotOverrideItems()
        {
            using var source = new RecyclableDictionary<int, int>();
            for (int i = 0; i < _testData.Length; i++)
            {
                source.Add(i, _testData[i]);
            }

            using var queue = new RecyclablePriorityQueue<KeyValuePair<int, int>>(comparer: Comparer<KeyValuePair<int, int>>.Create((a, b) => a.Key.CompareTo(b.Key)));

            queue.AddRange(source);
            queue.AddRange(source);

            var result = new List<KeyValuePair<int, int>>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            var expected = _testData.Select((v, i) => new KeyValuePair<int, int>(i, v))
                .Concat(_testData.Select((v, i) => new KeyValuePair<int, int>(i, v)))
                .ToList();
            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeSortedListShouldAddItemsInSortedOrder()
        {
            using var source = new RecyclableSortedList<int, int>();
            for (int i = 0; i < _testData.Length; i++)
            {
                source.Add(i, _testData[i]);
            }

            using var queue = new RecyclablePriorityQueue<(int Key, int Value)>(comparer: Comparer<(int Key, int Value)>.Create((a, b) => a.Key.CompareTo(b.Key)));

            queue.AddRange(source);

            var result = new List<(int Key, int Value)>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            var expected = _testData.Select((v, i) => (i, v)).ToList();
            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeSortedListShouldNotOverrideItems()
        {
            using var source = new RecyclableSortedList<int, int>();
            for (int i = 0; i < _testData.Length; i++)
            {
                source.Add(i, _testData[i]);
            }

            using var queue = new RecyclablePriorityQueue<(int Key, int Value)>(comparer: Comparer<(int Key, int Value)>.Create((a, b) => a.Key.CompareTo(b.Key)));

            queue.AddRange(source);
            queue.AddRange(source);

            var result = new List<(int Key, int Value)>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            var expected = _testData.Select((v, i) => (i, v))
                .Concat(_testData.Select((v, i) => (i, v)))
                .OrderBy(p => p.Item1)
                .ToList();
            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeSortedDictionaryShouldAddItemsInSortedOrder()
        {
            using var source = new RecyclableSortedDictionary<int, int>();
            for (int i = 0; i < _testData.Length; i++)
            {
                source.Add(i, _testData[i]);
            }

            using var queue = new RecyclablePriorityQueue<KeyValuePair<int, int>>(comparer: Comparer<KeyValuePair<int, int>>.Create((a, b) => a.Key.CompareTo(b.Key)));

            queue.AddRange(source);

            var result = new List<KeyValuePair<int, int>>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            var expected = _testData.Select((v, i) => new KeyValuePair<int, int>(i, v)).ToList();
            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeSortedDictionaryShouldNotOverrideItems()
        {
            using var source = new RecyclableSortedDictionary<int, int>();
            for (int i = 0; i < _testData.Length; i++)
            {
                source.Add(i, _testData[i]);
            }

            using var queue = new RecyclablePriorityQueue<KeyValuePair<int, int>>(comparer: Comparer<KeyValuePair<int, int>>.Create((a, b) => a.Key.CompareTo(b.Key)));

            queue.AddRange(source);
            queue.AddRange(source);

            var result = new List<KeyValuePair<int, int>>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            var expected = _testData.Select((v, i) => new KeyValuePair<int, int>(i, v))
                .Concat(_testData.Select((v, i) => new KeyValuePair<int, int>(i, v)))
                .ToList();
            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeEnumerableShouldAddItemsInSortedOrder()
        {
            using var queue = new RecyclablePriorityQueue<int>();

            _ = queue.AddRange((System.Collections.IEnumerable)_testData);

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            var expected = _testData.OrderBy(v => v).ToList();
            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeEnumerableShouldNotOverrideItems()
        {
            using var queue = new RecyclablePriorityQueue<int>();

            _ = queue.AddRange((System.Collections.IEnumerable)_testData);
            _ = queue.AddRange((System.Collections.IEnumerable)_testData);

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            var expected = _testData.Concat(_testData).OrderBy(v => v).ToList();
            _ = result.Should().Equal(expected);
        }

        [Fact]
        public void AddRangeReadOnlyCollectionShouldAddItemsInSortedOrder()
        {
            var source = new TestReadOnlyCollection<int>(_testData);
            using var queue = new RecyclablePriorityQueue<int>();

            queue.AddRange(source);

            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(_testData.OrderBy(static value => value));
        }

        [Fact]
        public void AddRangeReadOnlyCollectionShouldNotOverrideItems()
        {
            var source = new TestReadOnlyCollection<int>(_testData);
            using var queue = new RecyclablePriorityQueue<int>();

            queue.AddRange(source);
            queue.AddRange(source);

            var expected = _testData.Concat(_testData).OrderBy(static value => value).ToArray();
            var result = new List<int>();
            while (queue.LongCount > 0)
            {
                result.Add(queue.Dequeue());
            }

            _ = result.Should().Equal(expected);
        }

        private sealed class TestReadOnlyCollection<T> : IReadOnlyCollection<T>
        {
            private readonly T[] _items;

            public TestReadOnlyCollection(T[] items)
            {
                _items = items;
            }

            public int Count => _items.Length;

            public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_items).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
        }
    }
}


using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableQueueTests
	{
		private static readonly string[] _testData = new[] { "a", "d", "c", "b", "a" };

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void ShouldNotBeSortedWhenCreated(int itemsCount)
                {
                        // Act
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount).Select(i => i.ToString()).ToArray();
                        using var list = new RecyclableQueue<string>(data, 2);

                        // Validate
                        _ = list.Should().HaveCount(itemsCount)
                                .And.ContainInConsecutiveOrder(data);
                        _ = list.LongCount.Should().Be(itemsCount);
                }

                [Fact]
                public void ShouldBeEmptyWhenNotInitialized()
                {
                        // Act
                        using var list = new RecyclableQueue<string>(2);

                        // Validate
                        _ = list.Should().BeEmpty();
                        _ = list.LongCount.Should().Be(0);
                }

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void ShouldNotBeSortedWhenInitialized(int itemsCount)
                {
                        // Act
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount).Select(i => i.ToString()).ToArray();
                        using var list = new RecyclableQueue<string>(2)
                        {
                                // populate via collection initializer
                        };
                        foreach (var item in data)
                        {
                                list.Enqueue(item);
                        }

                        // Validate
                        if (itemsCount == 0)
                        {
                                _ = list.Should().BeEmpty();
                        }
                        else
                        {
                                _ = list.Should().NotBeEmpty()
                                        .And.ContainInConsecutiveOrder(data)
                                        .And.BeEquivalentTo(data);
                        }
                }

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void ShouldBeEmptyAfterClear(int itemsCount)
                {
                        // Prepare
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount).Select(i => i.ToString());
                        using var list = new RecyclableQueue<string>(data);

                        // Act
                        list.Clear();

                        // Validate
                        _ = list.Should().BeEmpty();
                }

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void DequeueShouldReturnItemsInTheSameOrder(int itemsCount)
                {
                        // Prepare
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount).Select(i => i.ToString()).ToArray();
                        using var list = new RecyclableQueue<string>();

                        // Act
                        for (var itemIdx = 0; itemIdx < data.Length; itemIdx++)
                        {
                                list.Enqueue(data[itemIdx]);
                        }

                        var dequeuedItems = new RecyclableLongList<string>();
                        while (list.LongCount > 0)
                        {
                                dequeuedItems.Add(list.Dequeue());
                        }

                        // Validate
                        _ = dequeuedItems.Should().ContainInConsecutiveOrder(data)
                                .And.BeEquivalentTo(data);
                }

                [Fact]
                public void DequeueShoudRaiseArgumentOutOfRangeWhenNoMoreElementsFound()
                {
                        // Prepare
                        using var list = new RecyclableQueue<string>(_testData);

                        // Act
                        while (list.LongCount > 0)
                        {
                                _ = list.Dequeue();
                        }

                        _ = Assert.Throws<ArgumentOutOfRangeException>(() => list.Dequeue());
                        _ = list.LongCount.Should().Be(0);
                }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void GetEnumeratorShouldYieldAllItemsInTheSameOrder(int itemsCount)
        {
                        // Prepare
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount).Select(i => i.ToString()).ToArray();
                        using var list = new RecyclableQueue<string>(data);

                        // Act
                        var actual = new RecyclableLongList<string>();
                        using var enumerator = list.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                                actual.Add(enumerator.Current);
                        }

                        // Validate
                _ = actual.Should().ContainInConsecutiveOrder(data)
                        .And.BeEquivalentTo(data);
        }

        [Fact]
        public void AddRangeShouldAcceptNulls()
        {
            using var queue = new RecyclableQueue<long?>();
            queue.AddRange(new long?[] { null, default });
            _ = queue.Should().HaveCount(2).And.AllSatisfy(x => x.Should().BeNull());
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddItemsInCorrectOrder(string testCase, IEnumerable<long> testData, int itemsCount)
        {
            using var queue = new RecyclableQueue<long>();

            if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((Array)testData);
            }
            else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((System.Collections.ICollection)testData);
            }
            else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((ICollection<long>)testData);
            }
            else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
            {
                _ = queue.AddRange((System.Collections.IEnumerable)testData);
            }
            else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((IReadOnlyList<long>)testData);
            }
            else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange(new ReadOnlySpan<long>((long[])testData));
            }
            else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange(new Span<long>((long[])testData));
            }
            else if (testData is long[] array)
            {
                queue.AddRange(array);
            }
            else if (testData is List<long> list)
            {
                queue.AddRange(list);
            }
            else if (testData is RecyclableList<long> rList)
            {
                queue.AddRange(rList);
            }
            else if (testData is RecyclableLongList<long> rLongList)
            {
                queue.AddRange(rLongList);
            }
            else if (testData is IList<long> iList)
            {
                queue.AddRange((ICollection<long>)iList);
            }
            else if (testData is IEnumerable<long> enumerable)
            {
                queue.AddRange(enumerable);
            }
            else
            {
                throw new InvalidCastException("Unknown type of test data");
            }

            List<long> expected = new(itemsCount);
            expected.AddRange(testData);

            _ = queue.Count.Should().Be(itemsCount);
            _ = queue.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.EmptySourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldDoNothingWhenSourceIsEmpty(string testCase, IEnumerable<long> testData, int itemsCount)
        {
            using var queue = new RecyclableQueue<long>();

            if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((Array)testData);
            }
            else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((System.Collections.ICollection)testData);
            }
            else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((ICollection<long>)testData);
            }
            else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
            {
                _ = queue.AddRange((System.Collections.IEnumerable)testData);
            }
            else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((IReadOnlyList<long>)testData);
            }
            else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange(new ReadOnlySpan<long>((long[])testData));
            }
            else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange(new Span<long>((long[])testData));
            }
            else if (testData is long[] array)
            {
                queue.AddRange(array);
            }
            else if (testData is List<long> list)
            {
                queue.AddRange(list);
            }
            else if (testData is RecyclableList<long> rList)
            {
                queue.AddRange(rList);
            }
            else if (testData is RecyclableLongList<long> rLongList)
            {
                queue.AddRange(rLongList);
            }
            else if (testData is IList<long> iList)
            {
                queue.AddRange((ICollection<long>)iList);
            }
            else if (testData is IEnumerable<long> enumerable)
            {
                queue.AddRange(enumerable);
            }
            else
            {
                throw new InvalidCastException("Unknown type of test data");
            }

            List<long> expected = new(itemsCount);
            expected.AddRange(testData);

            _ = queue.Count.Should().Be(itemsCount);
            _ = queue.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItems(string testCase, IEnumerable<long> testData, int itemsCount)
        {
            using var queue = new RecyclableQueue<long>();
            itemsCount = checked(itemsCount << 1);

            if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((Array)testData);
                queue.AddRange((Array)testData);
            }
            else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((System.Collections.ICollection)testData);
                queue.AddRange((System.Collections.ICollection)testData);
            }
            else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((ICollection<long>)testData);
                queue.AddRange((ICollection<long>)testData);
            }
            else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
            {
                _ = queue.AddRange((System.Collections.IEnumerable)testData);
                _ = queue.AddRange((System.Collections.IEnumerable)testData);
            }
            else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
            {
                queue.AddRange((IReadOnlyList<long>)testData);
                queue.AddRange((IReadOnlyList<long>)testData);
            }
            else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
            {
                var span = new ReadOnlySpan<long>((long[])testData);
                queue.AddRange(span);
                queue.AddRange(span);
            }
            else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
            {
                var span = new Span<long>((long[])testData);
                queue.AddRange(span);
                queue.AddRange(span);
            }
            else if (testData is long[] array)
            {
                queue.AddRange(array);
                queue.AddRange(array);
            }
            else if (testData is List<long> list)
            {
                queue.AddRange(list);
                queue.AddRange(list);
            }
            else if (testData is RecyclableList<long> rList)
            {
                queue.AddRange(rList);
                queue.AddRange(rList);
            }
            else if (testData is RecyclableLongList<long> rLongList)
            {
                queue.AddRange(rLongList);
                queue.AddRange(rLongList);
            }
            else if (testData is IList<long> iList)
            {
                queue.AddRange((ICollection<long>)iList);
                queue.AddRange((ICollection<long>)iList);
            }
            else if (testData is IEnumerable<long> enumerable)
            {
                queue.AddRange(enumerable);
                queue.AddRange(enumerable);
            }
            else
            {
                throw new InvalidCastException("Unknown type of test data");
            }

            List<long> expected = new(itemsCount);
            expected.AddRange(testData);
            expected.AddRange(testData);

            _ = queue.Count.Should().Be(itemsCount);
            _ = queue.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddStackItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableStack<long>(data);
            using var queue = new RecyclableQueue<long>();

            queue.AddRange(source);

            _ = queue.Count.Should().Be(itemsCount);
            _ = queue.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromStack(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableStack<long>(data);
            using var queue = new RecyclableQueue<long>();

            queue.AddRange(source);
            queue.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = queue.Count.Should().Be(itemsCount * 2);
            _ = queue.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddSortedSetItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableSortedSet<long>();
            foreach (long item in data)
            {
                source.Add(item);
            }
            using var queue = new RecyclableQueue<long>();

            queue.AddRange(source);

            _ = queue.Count.Should().Be(itemsCount);
            _ = queue.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromSortedSet(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableSortedSet<long>();
            foreach (long item in data)
            {
                source.Add(item);
            }
            using var queue = new RecyclableQueue<long>();

            queue.AddRange(source);
            queue.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = queue.Count.Should().Be(itemsCount * 2);
            _ = queue.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddLinkedListItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableLinkedList<long>(data);
            using var queue = new RecyclableQueue<long>();

            queue.AddRange(source);

            _ = queue.Count.Should().Be(itemsCount);
            _ = queue.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromLinkedList(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableLinkedList<long>(data);
            using var queue = new RecyclableQueue<long>();

            queue.AddRange(source);
            queue.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = queue.Count.Should().Be(itemsCount * 2);
            _ = queue.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddPriorityQueueItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclablePriorityQueue<long>();
            foreach (long item in data)
            {
                source.Enqueue(item);
            }
            using var queue = new RecyclableQueue<long>();

            queue.AddRange(source);

            _ = queue.Count.Should().Be(itemsCount);
            _ = queue.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromPriorityQueue(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclablePriorityQueue<long>();
            foreach (long item in data)
            {
                source.Enqueue(item);
            }
            using var queue = new RecyclableQueue<long>();

            queue.AddRange(source);
            queue.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = queue.Count.Should().Be(itemsCount * 2);
            _ = queue.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddQueueItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableQueue<long>(data);
            using var queue = new RecyclableQueue<long>();

            queue.AddRange(source);

            _ = queue.Count.Should().Be(itemsCount);
            _ = queue.Should().Equal(data);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromQueue(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableQueue<long>(data);
            using var queue = new RecyclableQueue<long>();

            queue.AddRange(source);
            queue.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);

            _ = queue.Count.Should().Be(itemsCount * 2);
            _ = queue.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddDictionaryItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableDictionary<int, long>();
            for (int i = 0; i < data.Length; i++)
            {
                source.Add(i, data[i]);
            }
            using var queue = new RecyclableQueue<KeyValuePair<int, long>>();

            queue.AddRange(source);

            List<KeyValuePair<int, long>> expected = new(itemsCount);
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add(new KeyValuePair<int, long>(i, data[i]));
            }

            _ = queue.Count.Should().Be(itemsCount);
            _ = queue.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromDictionary(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableDictionary<int, long>();
            for (int i = 0; i < data.Length; i++)
            {
                source.Add(i, data[i]);
            }
            using var queue = new RecyclableQueue<KeyValuePair<int, long>>();

            queue.AddRange(source);
            queue.AddRange(source);

            List<KeyValuePair<int, long>> expected = new(itemsCount * 2);
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add(new KeyValuePair<int, long>(i, data[i]));
            }
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add(new KeyValuePair<int, long>(i, data[i]));
            }

            _ = queue.Count.Should().Be(itemsCount * 2);
            _ = queue.Should().Equal(expected);
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
            using var queue = new RecyclableQueue<(int Key, long Value)>();

            queue.AddRange(source);

            List<(int Key, long Value)> expected = new(itemsCount);
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add((i, data[i]));
            }

            _ = queue.Count.Should().Be(itemsCount);
            _ = queue.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromSortedList(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableSortedList<int, long>();
            for (int i = 0; i < data.Length; i++)
            {
                source.Add(i, data[i]);
            }
            using var queue = new RecyclableQueue<(int Key, long Value)>();

            queue.AddRange(source);
            queue.AddRange(source);

            List<(int Key, long Value)> expected = new(itemsCount * 2);
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add((i, data[i]));
            }
            for (int i = 0; i < data.Length; i++)
            {
                expected.Add((i, data[i]));
            }

            _ = queue.Count.Should().Be(itemsCount * 2);
            _ = queue.Should().Equal(expected);
        }
        }
}

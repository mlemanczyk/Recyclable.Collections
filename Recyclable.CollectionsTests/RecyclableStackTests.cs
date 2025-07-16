using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableStackTests
	{
		private static readonly string[] _testData = new[] { "a", "d", "c", "b", "a" };

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void GetEnumeratorShouldYieldAllItemsInReversedOrder(int itemsCount)
                {
                        // Prepare
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount).Select(i => i.ToString()).ToArray();
                        using var list = new RecyclableStack<string>(data);

                        // Act
                        var actual = new RecyclableLongList<string>();
                        using var enumerator = list.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                                actual.Add(enumerator.Current);
                        }

                        // Validate
                        _ = actual.Should().ContainInConsecutiveOrder(data.Reverse());
                }

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void ShouldNotBeSortedWhenCreated(int itemsCount)
                {
                        // Act
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount).Select(i => i.ToString()).ToArray();
                        using var sortedList = new RecyclableStack<string>(data, 2);

                        // Validate
                        _ = sortedList.Should().HaveCount(itemsCount)
                                .And.ContainInConsecutiveOrder(data.Reverse());
                }

                [Fact]
                public void ShouldBeEmptyWhenNotInitialized()
                {
                        // Act
                        using var sortedList = new RecyclableStack<string>(2);

                        // Validate
                        _ = sortedList.Should().BeEmpty();
                }

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void ShouldNotBeSortedWhenInitialized(int itemsCount)
                {
                        // Act
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount).Select(i => i.ToString()).ToArray();
                        using var sortedList = new RecyclableStack<string>(2);
                        foreach (var item in data)
                        {
                                sortedList.Push(item);
                        }

                        // Validate
                        if (itemsCount == 0)
                        {
                                _ = sortedList.Should().BeEmpty();
                        }
                        else
                        {
                                _ = sortedList.Should().NotBeEmpty()
                                        .And.ContainInConsecutiveOrder(data.Reverse())
                                        .And.BeEquivalentTo(data);
                        }
                }

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void ShouldBeEmptyAfterClear(int itemsCount)
                {
                        // Prepare
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount).Select(i => i.ToString()).ToArray();
                        using var sortedList = new RecyclableStack<string>(data);

                        // Act
                        sortedList.Clear();

                        // Validate
                        _ = sortedList.Should().BeEmpty();
                }

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void PopShouldReturnItemsInReversedOrder(int itemsCount)
                {
                        // Prepare
                        using var list = new RecyclableStack<string>();

                        // Act
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount).Select(i => i.ToString()).ToArray();
                        for (var itemIdx = 0; itemIdx < data.Length; itemIdx++)
                        {
                                list.Push(data[itemIdx]);
                        }

                        var dequeuedItems = new RecyclableLongList<string>();
                        while (list.LongCount > 0)
                        {
                                dequeuedItems.Add(list.Pop());
                        }

                        // Validate
                        _ = dequeuedItems.Should().ContainInConsecutiveOrder(data.Reverse());
                }

                [Fact]
                public void PopShouldRaiseArgumentOutOfRangeWhenNoMoreElementsFound()
                {
                        // Prepare
                        using var list = new RecyclableStack<string>(_testData);

                        // Act
                        while (list.LongCount > 0)
                        {
                                _ = list.Pop();
                        }

                        _ = Assert.Throws<ArgumentOutOfRangeException>(() => list.Pop());
                }
        [Fact]
        public void AddRangeShouldAcceptNulls()
        {
            using var stack = new RecyclableStack<long?>();
            stack.AddRange(new long?[] { null, default });
            _ = stack.Should().HaveCount(2).And.AllSatisfy(x => x.Should().BeNull());
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddItemsInCorrectOrder(string testCase, IEnumerable<long> testData, int itemsCount)
        {
            using var stack = new RecyclableStack<long>();

            if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((Array)testData);
            }
            else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((System.Collections.ICollection)testData);
            }
            else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((ICollection<long>)testData);
            }
            else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
            {
                _ = stack.AddRange((System.Collections.IEnumerable)testData);
            }
            else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((IReadOnlyList<long>)testData);
            }
            else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange(new ReadOnlySpan<long>((long[])testData));
            }
            else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange(new Span<long>((long[])testData));
            }
            else if (testData is long[] array)
            {
                stack.AddRange(array);
            }
            else if (testData is List<long> list)
            {
                stack.AddRange(list);
            }
            else if (testData is RecyclableList<long> rList)
            {
                stack.AddRange(rList);
            }
            else if (testData is RecyclableLongList<long> rLongList)
            {
                stack.AddRange(rLongList);
            }
            else if (testData is IList<long> iList)
            {
                stack.AddRange((ICollection<long>)iList);
            }
            else if (testData is IEnumerable<long> enumerable)
            {
                stack.AddRange(enumerable);
            }
            else
            {
                throw new InvalidCastException("Unknown type of test data");
            }

            List<long> expected = new(itemsCount);
            expected.AddRange(testData);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount);
            _ = stack.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.EmptySourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldDoNothingWhenSourceIsEmpty(string testCase, IEnumerable<long> testData, int itemsCount)
        {
            using var stack = new RecyclableStack<long>();

            if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((Array)testData);
            }
            else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((System.Collections.ICollection)testData);
            }
            else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((ICollection<long>)testData);
            }
            else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
            {
                _ = stack.AddRange((System.Collections.IEnumerable)testData);
            }
            else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((IReadOnlyList<long>)testData);
            }
            else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange(new ReadOnlySpan<long>((long[])testData));
            }
            else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange(new Span<long>((long[])testData));
            }
            else if (testData is long[] array)
            {
                stack.AddRange(array);
            }
            else if (testData is List<long> list)
            {
                stack.AddRange(list);
            }
            else if (testData is RecyclableList<long> rList)
            {
                stack.AddRange(rList);
            }
            else if (testData is RecyclableLongList<long> rLongList)
            {
                stack.AddRange(rLongList);
            }
            else if (testData is IList<long> iList)
            {
                stack.AddRange((ICollection<long>)iList);
            }
            else if (testData is IEnumerable<long> enumerable)
            {
                stack.AddRange(enumerable);
            }
            else
            {
                throw new InvalidCastException("Unknown type of test data");
            }

            List<long> expected = new(itemsCount);
            expected.AddRange(testData);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount);
            _ = stack.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItems(string testCase, IEnumerable<long> testData, int itemsCount)
        {
            using var stack = new RecyclableStack<long>();
            itemsCount = checked(itemsCount << 1);

            if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((Array)testData);
                stack.AddRange((Array)testData);
            }
            else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((System.Collections.ICollection)testData);
                stack.AddRange((System.Collections.ICollection)testData);
            }
            else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((ICollection<long>)testData);
                stack.AddRange((ICollection<long>)testData);
            }
            else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
            {
                _ = stack.AddRange((System.Collections.IEnumerable)testData);
                _ = stack.AddRange((System.Collections.IEnumerable)testData);
            }
            else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
            {
                stack.AddRange((IReadOnlyList<long>)testData);
                stack.AddRange((IReadOnlyList<long>)testData);
            }
            else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
            {
                var span = new ReadOnlySpan<long>((long[])testData);
                stack.AddRange(span);
                stack.AddRange(span);
            }
            else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
            {
                var span = new Span<long>((long[])testData);
                stack.AddRange(span);
                stack.AddRange(span);
            }
            else if (testData is long[] array)
            {
                stack.AddRange(array);
                stack.AddRange(array);
            }
            else if (testData is List<long> list)
            {
                stack.AddRange(list);
                stack.AddRange(list);
            }
            else if (testData is RecyclableList<long> rList)
            {
                stack.AddRange(rList);
                stack.AddRange(rList);
            }
            else if (testData is RecyclableLongList<long> rLongList)
            {
                stack.AddRange(rLongList);
                stack.AddRange(rLongList);
            }
            else if (testData is IList<long> iList)
            {
                stack.AddRange((ICollection<long>)iList);
                stack.AddRange((ICollection<long>)iList);
            }
            else if (testData is IEnumerable<long> enumerable)
            {
                stack.AddRange(enumerable);
                stack.AddRange(enumerable);
            }
            else
            {
                throw new InvalidCastException("Unknown type of test data");
            }

            List<long> expected = new(itemsCount);
            expected.AddRange(testData);
            expected.AddRange(testData);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount);
            _ = stack.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddListItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableList<long>(data);
            using var stack = new RecyclableStack<long>();

            stack.AddRange(source);

            List<long> expected = new(itemsCount);
            expected.AddRange(data);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount);
            _ = stack.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromList(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableList<long>(data);
            using var stack = new RecyclableStack<long>();

            stack.AddRange(source);
            stack.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount * 2);
            _ = stack.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddLongListItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableLongList<long>(data);
            using var stack = new RecyclableStack<long>();

            stack.AddRange(source);

            List<long> expected = new(itemsCount);
            expected.AddRange(data);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount);
            _ = stack.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromLongList(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableLongList<long>(data);
            using var stack = new RecyclableStack<long>();

            stack.AddRange(source);
            stack.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount * 2);
            _ = stack.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddHashSetItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableHashSet<long>();
            foreach (long item in data)
            {
                source.Add(item);
            }
            using var stack = new RecyclableStack<long>();

            stack.AddRange(source);

            List<long> expected = new(itemsCount);
            expected.AddRange(data);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount);
            _ = stack.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromHashSet(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableHashSet<long>();
            foreach (long item in data)
            {
                source.Add(item);
            }
            using var stack = new RecyclableStack<long>();

            stack.AddRange(source);
            stack.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount * 2);
            _ = stack.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldAddStackItemsInCorrectOrder(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableStack<long>(data);
            using var stack = new RecyclableStack<long>();

            stack.AddRange(source);

            List<long> expected = new(itemsCount);
            expected.AddRange(data);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount);
            _ = stack.Should().Equal(expected);
        }

        [Theory]
        [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
        public void AddRangeShouldNotOverrideItemsFromStack(int itemsCount)
        {
            long[] data = RecyclableLongListTestData.CreateTestData(itemsCount).ToArray();
            using var source = new RecyclableStack<long>(data);
            using var stack = new RecyclableStack<long>();

            stack.AddRange(source);
            stack.AddRange(source);

            List<long> expected = new(itemsCount * 2);
            expected.AddRange(data);
            expected.AddRange(data);
            expected.Reverse();

            _ = stack.Count.Should().Be(itemsCount * 2);
            _ = stack.Should().Equal(expected);
        }

        }
}

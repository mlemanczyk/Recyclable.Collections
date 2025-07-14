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
	}
}

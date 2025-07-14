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
	}
}

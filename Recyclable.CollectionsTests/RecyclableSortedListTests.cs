using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableSortedListTests
	{
		private static readonly (int, string)[] _testData = new[] { (5, "a"), (4, "d"), (3, "c"), (2, "b"), (1, "a") };

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void ShouldBeSortedWhenCreated(int itemsCount)
                {
                        // Act
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount)
                                .Select(x => (itemsCount - x + 1, x))
                                .ToArray();
                        using var sortedList = new RecyclableSortedList<long, long>(data, 2);

                        // Validate
                        _ = sortedList.Should().HaveCount(itemsCount)
                                .And.BeInAscendingOrder(item => item.Key);
                }

		[Fact]
		public void ShouldBeEmptyWhenNotInitialized()
		{
			// Act
			using var sortedList = new RecyclableSortedList<int, string>(2);

			// Validate
			_ = sortedList.Should().BeEmpty();
		}

		[Fact]
		public void ShouldNotBeSortedWhenInitialized()
		{
			// Act
			using var sortedList = new RecyclableSortedList<int, string>(2)
			{
				_testData[0],
				_testData[1],
				_testData[2],
				_testData[3],
				_testData[4]
			};

			// Validate
			_ = sortedList.Should().NotBeEmpty()
				.And.ContainInConsecutiveOrder(_testData);
		}

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void ShouldBeSortedWhenUpdateEnds(int itemsCount)
                {
                        // Prepare
                        using var sortedList = new RecyclableSortedList<long, long>(2);

                        // Act
                        sortedList.BeginUpdate();
                        foreach (var value in RecyclableLongListTestData.CreateTestData(itemsCount))
                        {
                                sortedList.Add((itemsCount - value + 1, value));
                        }

                        // Validate
                        _ = sortedList.Should().BeInDescendingOrder(x => x.Key);

                        // Act
                        sortedList.EndUpdate();

                        // Validate
                        _ = sortedList.Should().BeInAscendingOrder(x => x.Key);
                }

                [Theory]
                [MemberData(nameof(RecyclableLongListTestData.ItemsCountTestCases), MemberType = typeof(RecyclableLongListTestData))]
                public void ShouldBeEmptyAfterClear(int itemsCount)
                {
                        // Prepare
                        var data = RecyclableLongListTestData.CreateTestData(itemsCount)
                                .Select(x => (x, x));
                        using var sortedList = new RecyclableSortedList<long, long>(data);

                        // Act
                        sortedList.Clear();

                        // Validate
                        _ = sortedList.Should().BeEmpty();
                }
        }
}
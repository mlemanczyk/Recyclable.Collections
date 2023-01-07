using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableSortedListTests
	{
		private static readonly (int, string)[] _testData = new[] { (5, "a"), (4, "d"), (3, "c"), (2, "b"), (1, "a") };

		[Fact]
		public void ShouldBeSortedWhenCreated()
		{
			// Act
			using var sortedList = new RecyclableSortedList<int, string>(_testData, 2);

			// Validate
			_ = sortedList.Should().HaveCount(_testData.Length)
				.And.BeEquivalentTo(_testData)
				.And.BeInAscendingOrder((item) => item.Key);
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

		[Fact]
		public void ShouldBeSortedWhenUpdateEnds()
		{
			// Prepare
			using var sortedList = new RecyclableSortedList<int, string>(2);

			// Act
			sortedList.BeginUpdate();
			foreach (var testItem in _testData)
			{
				sortedList.Add(testItem);
			}

			// Validate
			_ = sortedList.Should().BeInDescendingOrder(x => x.Key);

			// Act
			sortedList.EndUpdate();

			// Validate
			_ = sortedList.Should().BeInAscendingOrder(x => x.Key);
		}

		[Fact]
		public void ShouldBeEmptyAfterClear()
		{
			// Prepare
			using var sortedList = new RecyclableSortedList<int, string>(_testData);

			// Act
			sortedList.Clear();

			// Validate
			_ = sortedList.Should().BeEmpty();
		}
	}
}
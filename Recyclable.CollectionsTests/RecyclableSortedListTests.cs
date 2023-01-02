using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	[TestClass()]
	public class RecyclableSortedListTests
	{
		private static readonly (int, string)[] _testData = new[] { (5, "a"), (4, "d"), (3, "c"), (2, "b"), (1, "a") };

		[TestMethod]
		public void ShouldBeSortedWhenCreated()
		{
			// Act
			using var sortedList = new RecyclableSortedList<int, string>(_testData);

			// Validate
			_ = sortedList.Should().HaveCount(_testData.Length)
				.And.BeEquivalentTo(_testData)
				.And.BeInAscendingOrder((item) => item.Key);
		}

		[TestMethod]
		public void ShouldBeEmptyWhenNotInitialized()
		{
			// Act
			using var sortedList = new RecyclableSortedList<int, string>();

			// Validate
			_ = sortedList.Should().BeEmpty();
		}

		[TestMethod]
		public void ShouldBeSortedWhenInitialized()
		{
			// Act
			using var sortedList = new RecyclableSortedList<int, string>()
			{
				_testData[0],
				_testData[1],
				_testData[2],
				_testData[3],
				_testData[4]
			};

			// Validate
			_ = sortedList.Should().NotBeEmpty()
				.And.BeInAscendingOrder(x => x.Key)
				.And.BeEquivalentTo(_testData);
		}

		[TestMethod]
		public void ShouldBeSortedWhenUpdateEnds()
		{
			// Prepare
			using var sortedList = new RecyclableSortedList<int, string>();

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

		[TestMethod]
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
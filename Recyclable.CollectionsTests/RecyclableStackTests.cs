using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableStackTests
	{
		private static readonly string[] _testData = new[] { "a", "d", "c", "b", "a" };

		[Fact]
		public void GetEnumeratorShouldYieldAllItemsInReversedOrder()
		{
			// Prepare
			using var list = new RecyclableStack<string>(_testData);

			// Act
			var actual = new RecyclableLongList<string>();
			using var enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				actual.Add(enumerator.Current);
			}

			// Validate
			_ = actual.Should().ContainInConsecutiveOrder(_testData.Reverse());
		}

		[Fact]
		public void ShouldNotBeSortedWhenCreated()
		{
			// Act
			using var sortedList = new RecyclableStack<string>(_testData, 2);

			// Validate
			_ = sortedList.Should().HaveCount(_testData.Length)
				.And.ContainInConsecutiveOrder(_testData);
		}

		[Fact]
		public void ShouldBeEmptyWhenNotInitialized()
		{
			// Act
			using var sortedList = new RecyclableStack<string>(2);

			// Validate
			_ = sortedList.Should().BeEmpty();
		}

		[Fact]
		public void ShouldNotBeSortedWhenInitialized()
		{
			// Act
			using var sortedList = new RecyclableStack<string>(2)
			{
				_testData[0],
				_testData[1],
				_testData[2],
				_testData[3],
				_testData[4]
			};

			// Validate
			_ = sortedList.Should().NotBeEmpty()
				.And.ContainInConsecutiveOrder(_testData)
				.And.BeEquivalentTo(_testData);
		}

		[Fact]
		public void ShouldBeEmptyAfterClear()
		{
			// Prepare
			using var sortedList = new RecyclableStack<string>(_testData);

			// Act
			sortedList.Clear();

			// Validate
			_ = sortedList.Should().BeEmpty();
		}

		[Fact]
		public void PopShouldReturnItemsInReversedOrder()
		{
			// Prepare
			using var list = new RecyclableStack<string>();

			// Act
			for (var itemIdx = 0; itemIdx < _testData.Length; itemIdx++)
			{
				list.Push(_testData[itemIdx]);
			}

			var dequeuedItems = new RecyclableLongList<string>();
			while (list.LongCount > 0)
			{
				dequeuedItems.Add(list.Pop());
			}

			// Validate
			_ = dequeuedItems.Should().ContainInConsecutiveOrder(_testData.Reverse());
		}

		[Fact]
		public void PopShouldRaiseArgumentOutOfRangeWhenNoMoreElementsFound()
		{
			// Prepare
			using var list = new RecyclableQueue<string>(_testData);

			// Act
			while (list.LongCount > 0)
			{
				_ = list.Dequeue();
			}

			_ = Assert.Throws<ArgumentOutOfRangeException>(() => list.Dequeue());
		}
	}
}

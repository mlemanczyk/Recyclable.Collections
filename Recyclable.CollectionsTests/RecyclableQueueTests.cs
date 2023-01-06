using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	[TestClass]
	public class RecyclableQueueTests
	{
		private static readonly string[] _testData = new[] { "a", "d", "c", "b", "a" };

		[TestMethod]
		public void ShouldNotBeSortedWhenCreated()
		{
			// Act
			using var list = new RecyclableQueue<string>(_testData, 2);

			// Validate
			_ = list.Should().HaveCount(_testData.Length)
				.And.ContainInConsecutiveOrder(_testData);
		}

		[TestMethod]
		public void ShouldBeEmptyWhenNotInitialized()
		{
			// Act
			using var list = new RecyclableQueue<string>(2);

			// Validate
			_ = list.Should().BeEmpty();
		}

		[TestMethod]
		public void ShouldNotBeSortedWhenInitialized()
		{
			// Act
			using var list = new RecyclableQueue<string>(2)
			{
				_testData[0],
				_testData[1],
				_testData[2],
				_testData[3],
				_testData[4]
			};

			// Validate
			_ = list.Should().NotBeEmpty()
				.And.ContainInConsecutiveOrder(_testData)
				.And.BeEquivalentTo(_testData);
		}

		[TestMethod]
		public void ShouldBeEmptyAfterClear()
		{
			// Prepare
			using var list = new RecyclableQueue<string>(_testData);

			// Act
			list.Clear();

			// Validate
			_ = list.Should().BeEmpty();
		}

		[TestMethod]
		public void DequeueShouldReturnItemsInTheSameOrder()
		{
			// Prepare
			using var list = new RecyclableQueue<string>();

			// Act
			for (var itemIdx = 0; itemIdx < _testData.Length; itemIdx++)
			{
				list.Enqueue(_testData[itemIdx]);
			}

			var dequeuedItems = new RecyclableList<string>();
			while (list.LongCount > 0)
			{
				dequeuedItems.Add(list.Dequeue());
			}

			// Validate
			_ = dequeuedItems.Should().ContainInConsecutiveOrder(_testData);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void DequeueShoudRaiseArgumentOutOfRangeWhenNoMoreElementsFound()
		{
			// Prepare
			using var list = new RecyclableQueue<string>(_testData);

			// Act
			while (list.LongCount > 0)
			{
				_ = list.Dequeue();
			}

			_ = list.Dequeue();
		}

		[TestMethod]
		public void GetEnumeratorShouldYieldAllItemsInTheSameOrder()
		{
			// Prepare
			using var list = new RecyclableQueue<string>(_testData);

			// Act
			var actual = new RecyclableList<string>();
			var enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				actual.Add(enumerator.Current);
			}

			// Validate
			_ = actual.Should().ContainInConsecutiveOrder(_testData);
		}
	}
}

using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	[TestClass()]
	public class RecyclableListTests
	{
		private const int _totalObjectCount = 5_000;
		private static RecyclableList<int> NewRecyclableList => CreateReversedRecyclableList(_testData);
		private static readonly IEnumerable<int> _testData = Enumerable.Range(1, _totalObjectCount);
		private static RecyclableList<int> CreateReversedRecyclableList(IEnumerable<int> source) => new(source);

		[TestMethod]
		public void AddShouldAddItems()
		{
			// Prepare
			using var list = new RecyclableList<int>();

			// Act
			const int itemsCount = 5;
			foreach (var index in Enumerable.Range(1, itemsCount))
			{
				list.Add(index);
			}

			// Validate
			_ = list.LongCount.Should().Be(itemsCount, "we added so many items");
			for (var index = 0; index < itemsCount; index++)
			{
				var actual = list[index];
				_ = actual.Should().Be(index + 1);
			}
		}

		[TestMethod]
		public void AddShouldAcceptDuplicates()
		{
			// Prepare
			int[] testNumbers = new[] { 1, 2, 2, 3 };

			// Act
			using var list = new RecyclableList<int>();
			foreach (var index in testNumbers)
			{
				list.Add(index);
			}

			// Validate
			_ = list.LongCount.Should().Be(testNumbers.Length);
			_ = list.Should().BeEquivalentTo(testNumbers);
		}

		[TestMethod]
		public void ConstructorShouldAcceptDuplicates()
		{
			// Prepare
			int[] testNumbers = new[] { 1, 2, 2, 3 };

			// Act
			using var list = new RecyclableList<int>(testNumbers);

			// Validate
			_ = list.LongCount.Should().Be(testNumbers.Length);
			_ = list.Should().BeEquivalentTo(testNumbers);
		}

		[TestMethod]
		public void InitializeSortClearShouldSucceed()
		{
			var expectedList = _testData;

			// Act
			using RecyclableList<int> list = NewRecyclableList;
			list.QuickSort();

			// Validate
			_ = list.LongCount.Should().Be(_totalObjectCount);
			_ = list.Should().Equal(expectedList);
			_ = expectedList.Should().AllSatisfy(expected => list.Contains(expected));

			// Act
			list.Clear();

			// Validate
			_ = list.LongCount.Should().Be(0);
		}

		[TestMethod]
		public void CopyToShouldCopyAllItems()
		{
			// Prepare
			using var list = NewRecyclableList;
			
			// Act
			var actual = new int[list.LongCount];
			list.CopyTo(actual, 0);
			_ = actual.Should().BeEquivalentTo(_testData);
		}

		[TestMethod]
		public void GetEnumeratorShouldYieldAllItems()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act
			var actual = new List<int>();
			var enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				actual.Add(enumerator.Current);
			}

			// Validate
			_ = actual.Should().BeEquivalentTo(_testData);
		}

		[TestMethod]
		public void IndexOfShouldFindTheIndexes()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act & Validate
			foreach(var index in _testData)
			{
				var actual = list.IndexOf(index);
				_ = actual.Should().Be(index - 1);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void InsertShouldRaiseNotSupportedException()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act
			list.Insert(0, -1);
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void RemoveShouldRaiseNotSupportedException()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act
			bool removed = list.Remove(_testData.First());
			_ = removed.Should().BeFalse();
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void RemoveAtShouldRaiseNotSupportedException()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act
			list.RemoveAt(0);
		}

		[TestMethod]
		public void DisposeShouldSucceed()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act
			list.Dispose();

			// Validate
			_ = list.LongCount.Should().Be(0L);
		}

		[TestMethod]
		public void ConsecutiveDisposeShouldSucceed()
		{
			// Prepare
			using var list = NewRecyclableList;
			list.Dispose();

			// Act
			list.Dispose();
		}
	}
}
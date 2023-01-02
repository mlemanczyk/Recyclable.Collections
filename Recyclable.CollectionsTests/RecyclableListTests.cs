using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	[TestClass()]
	public class RecyclableListTests
	{
		private const int _totalObjectCount = 5000;
		private static RecyclableList<int> NewRecyclableList => CreateReversedRecyclableList(_testData);
		private static readonly IEnumerable<int> _testData = Enumerable.Range(1, _totalObjectCount);
		private static RecyclableList<int> CreateReversedRecyclableList(IEnumerable<int> source) => new(source);

		[TestMethod]
		public void AddShouldAddItems()
		{
			// Prepare
			using var sut = new RecyclableList<int>();

			// Act
			const int itemsCount = 5;
			foreach (var index in Enumerable.Range(1, itemsCount))
			{
				sut.Add(index);
			}

			// Validate
			_ = sut.Count.Should().Be(itemsCount, "we added so many items");
			for (var index = 0; index < itemsCount; index++)
			{
				var actual = sut[index];
				_ = actual.Should().Be(index + 1);
			}
		}

		[TestMethod]
		public void AddShouldAcceptDuplicates()
		{
			// Prepare
			int[] testNumbers = new[] { 1, 2, 2, 3 };

			// Act
			using var sut = new RecyclableList<int>();
			foreach (var index in testNumbers)
			{
				sut.Add(index);
			}

			// Validate
			_ = sut.LongCount.Should().Be(testNumbers.Length);
			_ = sut.Should().BeEquivalentTo(testNumbers);
		}

		[TestMethod]
		public void ConstructorShouldAcceptDuplicates()
		{
			// Prepare
			int[] testNumbers = new[] { 1, 2, 2, 3 };

			// Act
			using var sut = new RecyclableList<int>(testNumbers);

			// Validate
			_ = sut.LongCount.Should().Be(testNumbers.Length);
			_ = sut.Should().BeEquivalentTo(testNumbers);
		}

		[TestMethod]
		public void InitializeSortClearShouldSucceed()
		{
			var expectedList = _testData;

			// Act
			using RecyclableList<int> actualList = NewRecyclableList;
			actualList.QuickSort();

			// Validate
			_ = actualList.LongCount.Should().Be(_totalObjectCount);
			_ = actualList.Count.Should().Be(_totalObjectCount);
			_ = actualList.Should().Equal(expectedList);
			_ = expectedList.Should().AllSatisfy(expected => actualList.Contains(expected));

			// Act
			actualList.Clear();

			// Validate
			_ = actualList.LongCount.Should().Be(0);
			_ = actualList.Count.Should().Be(0);
		}

		[TestMethod]
		public void CopyToShouldCopyAllItems()
		{
			// Prepare
			using var sut = NewRecyclableList;
			
			// Act
			var actual = new int[sut.Count];
			sut.CopyTo(actual, 0);
			_ = actual.Should().BeEquivalentTo(_testData);
		}

		[TestMethod]
		public void GetEnumeratorShouldYieldAllItems()
		{
			// Prepare
			using var sut = NewRecyclableList;

			// Act
			var actual = new List<int>();
			var enumerator = sut.GetEnumerator();
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
			using var sut = NewRecyclableList;

			// Act & Validate
			foreach(var index in _testData)
			{
				var actual = sut.IndexOf(index);
				_ = actual.Should().Be(index - 1);
			}
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void InsertShouldRaiseNotSupportedException()
		{
			// Prepare
			using var sut = NewRecyclableList;

			// Act
			sut.Insert(0, -1);
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void RemoveShouldRaiseNotSupportedException()
		{
			// Prepare
			using var sut = NewRecyclableList;

			// Act
			bool removed = sut.Remove(_testData.First());
			_ = removed.Should().BeFalse();
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public void RemoveAtShouldRaiseNotSupportedException()
		{
			// Prepare
			using var sut = NewRecyclableList;

			// Act
			sut.RemoveAt(0);
		}

		[TestMethod]
		public void DisposeShouldSucceed()
		{
			// Prepare
			using var sut = NewRecyclableList;

			// Act
			sut.Dispose();

			// Validate
			_ = sut.LongCount.Should().Be(0L);
		}

		[TestMethod]
		public void ConsecutiveDisposeShouldSucceed()
		{
			// Prepare
			using var sut = NewRecyclableList;
			sut.Dispose();

			// Act
			sut.Dispose();
		}
	}
}
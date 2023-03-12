using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableListTests
	{
		private const int _totalObjectCount = 100_000;
		private static RecyclableList<int> NewRecyclableList => CreateReversedRecyclableList(_testData);
		private static readonly IEnumerable<int> _testData = Enumerable.Range(1, _totalObjectCount);
		private static RecyclableList<int> CreateReversedRecyclableList(IEnumerable<int> source) => new(source, 1024);

		[Fact]
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

		[Fact]
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

		[Fact]
		public void AddShouldAddItemWhenAfterAddRange()
		{
			// Prepare
			int[] testNumbers = new[] { 1, 2, 2, 3 };

			// Act
			using var list = new RecyclableList<int>();
			list.AddRange(_testData);
			foreach (var index in testNumbers)
			{
				list.Add(index);
			}

			// Validate
			_ = list.LongCount.Should().Be(_totalObjectCount + testNumbers.Length);
			_ = list.Should().BeEquivalentTo(_testData.Concat(testNumbers));
		}

		[Fact]
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

		[Fact]
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

		[Fact]
		public void ContainsShouldFindAllItems()
		{
			// Prepare
			var testData = _testData;
			using var list = NewRecyclableList;
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act
			foreach (var item in testData)
			{
				// Validate
				_ = list.Contains(item).Should().BeTrue($"we searched for {item}");
			}
		}

		[Fact]
		public void ContainsShouldNotFindNonExistingItems()
		{
			// Prepare
			var testData = _testData;
			using var list = NewRecyclableList;
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act
			foreach (var item in testData)
			{
				// Validate
				_ = list.Contains(-item).Should().BeFalse();
			}

			// Validate
			_ = list.Contains(0).Should().BeFalse();
		}

		[Fact]
		public void ContainsShouldNotFindAnythingWhenListEmpty()
		{
			// Prepare
			using var list = new RecyclableList<int>();

			// Validate
			_ = list.Contains(0).Should().BeFalse();
			_ = list.Contains(1).Should().BeFalse();
		}

		[Fact]
		public void CopyToShouldCopyAllItems()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act
			var actual = new int[list.LongCount];
			list.CopyTo(actual, 0);
			_ = actual.Should().BeEquivalentTo(_testData);
		}

		[Fact]
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

		[Fact]
		public void IndexOfShouldReturnCorrectIndexes()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act & Validate
			foreach (var item in _testData)
			{
				var actual = list.IndexOf(item);
				_ = actual.Should().Be(item - 1);
			}
		}

		[Fact]
		public void IndexOfShouldNotFindNonExistingItems()
		{
			// Prepare
			var testData = _testData;
			using var list = NewRecyclableList;
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act
			foreach (var item in testData)
			{
				// Validate
				_ = list.IndexOf(-item).Should().Be(-1);
			}

			// Validate
			_ = list.IndexOf(0).Should().Be(-1);
		}

		[Fact]
		public void IndexOfShouldNotFindAnythingWhenListEmpty()
		{
			// Prepare
			using var list = new RecyclableList<int>();

			// Validate
			_ = list.IndexOf(0).Should().Be(-1);
			_ = list.IndexOf(1).Should().Be(-1);
		}

		[Fact]
		public void InsertShouldRaiseNotSupportedException()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act
			_ = Assert.Throws<NotSupportedException>(() => list.Insert(0, -1));
		}

		[Fact]
		public void RemoveShouldRemoveLastItem()
		{
			// Prepare
			using var list = NewRecyclableList;

			int removedCount = 0;
			var testData = _testData.ToArray();
			foreach (var item in _testData.Reverse())
			{
				// Act & Validate
				_ = list.Remove(item).Should().BeTrue($"we search for {item} which should exist");
				removedCount++;
				var expectedList = testData[0..(testData.Length - removedCount)];
				_ = list.LongCount.Should().Be(expectedList.Length);
				_ = list.Should().ContainInConsecutiveOrder(expectedList);
			}

			_ = list.Should().BeEmpty();
		}

		[Fact]
		public void RemoveShouldRaiseNotSupportedException()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act
			_ = Assert.Throws<ArgumentOutOfRangeException>(() => list.Remove(_testData.First()));
		}

		[Fact]
		public void RemoveAtShouldRaiseNotSupportedException()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act
			_ = Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(0));
		}

		[Fact]
		public void DisposeShouldSucceed()
		{
			// Prepare
			using var list = NewRecyclableList;

			// Act
			list.Dispose();

			// Validate
			_ = list.LongCount.Should().Be(0L);
		}

		[Fact]
		public void ConsecutiveDisposeShouldSucceed()
		{
			// Prepare
			using var list = NewRecyclableList;
			list.Dispose();

			// Act
			list.Dispose();
		}

		[Fact]
		public void ClearShouldRemoveAllItems()
		{
			// Prepare
			using var list = NewRecyclableList;
			_ = list.LongCount.Should().NotBe(0);

			// Act		
			list.Clear();

			// Validate			
			_ = list.LongCount.Should().Be(0);
		}

		public static IEnumerable<object[]> AddRangeCollectionTypes => new[]
		{
			new object[] { "int[]", _testData.ToArray() },
			new object[] { "List<int>", _testData.ToList() },
			new object[] { "RecyclableArrayList<int>", _testData.ToRecyclableArrayList() },
			new object[] { "RecyclableList<int>", _testData.ToRecyclableList() },
			new object[] { "IList<int>", new CustomIList<int>(_testData) },
			new object[] { "IEnumerable<int> with non-enumerated count", _testData },
			new object[] { "IEnumerable<int> without non-enumerated count", new EnumerableWithoutCount<int>(_testData) }
		};

		public static IEnumerable<object[]> AddRangeEmptyCollectionTypes => new[]
		{
			new object[] { "int[]", Array.Empty<int>() },
			new object[] { "List<int>", Array.Empty<int>().ToList() },
			new object[] { "RecyclableArrayList<int>", Array.Empty<int>().ToRecyclableArrayList() },
			new object[] { "RecyclableList<int>", Array.Empty<int>().ToRecyclableList() },
			new object[] { "IList<int>", new CustomIList<int>(Array.Empty<int>()) },
			new object[] { "IEnumerable<int> with non-enumerated count", Array.Empty<int>().AsEnumerable() },
			new object[] { "IEnumerable<int> without non-enumerated count", new EnumerableWithoutCount<int>(Array.Empty<int>()) }
		};

		[Theory]
		[MemberData(nameof(AddRangeCollectionTypes))]
		public void AddRangeShouldAddItemsInCorrectOrder(string testCase, IEnumerable<int> testData)
		{
			try
			{
				// Prepare
				using var list = new RecyclableList<int>();

				// Act
				if (testData is int[] testDataArray)
				{
					list.AddRange(testDataArray);
				}
				else if (testData is List<int> testDataList)
				{
					list.AddRange(testDataList);
				}
				else if (testData is RecyclableArrayList<int> testDataRecyclableArrayList)
				{
					list.AddRange(testDataRecyclableArrayList);
				}
				else if (testData is RecyclableList<int> testDataRecyclableList)
				{
					list.AddRange(testDataRecyclableList);
				}
				else if (testData is IList<int> testDataIList)
				{
					list.AddRange(testDataIList);
				}
				else if (testData is IEnumerable<int> testDataIEnumerable)
				{
					list.AddRange(testDataIEnumerable);
				}
				else
				{
					throw new InvalidCastException("Unknown type of test data");
				}

				// Validate
				int expectedItemsCount = testData.Count();
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo(expectedItemsCount, "when capacity == 0, then we should allocate as much memory as needed, only");
				_ = list.Should().HaveCount(expectedItemsCount)
					.And.ContainInConsecutiveOrder(testData);
			}
			finally
			{
				if (testData is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}

		[Theory]
		[MemberData(nameof(AddRangeCollectionTypes))]
		public void AddRangeShouldNotOverrideItems(string testCase, IEnumerable<int> testData)
		{
			// Prepare
			var list = new RecyclableList<int>();

			// Act
			if (testData is int[] testDataArray)
			{
				list.AddRange(testDataArray);
				list.AddRange(testDataArray);
			}
			else if (testData is List<int> testDataList)
			{
				list.AddRange(testDataList);
				list.AddRange(testDataList);
			}
			else if (testData is RecyclableArrayList<int> testDataRecyclableArrayList)
			{
				list.AddRange(testDataRecyclableArrayList);
				list.AddRange(testDataRecyclableArrayList);
			}
			else if (testData is RecyclableList<int> testDataRecyclableList)
			{
				list.AddRange(testDataRecyclableList);
				list.AddRange(testDataRecyclableList);
			}
			else if (testData is IList<int> testDataIList)
			{
				list.AddRange(testDataIList);
				list.AddRange(testDataIList);
			}
			else if (testData is IEnumerable<int> testDataIEnumerable)
			{
				list.AddRange(testDataIEnumerable);
				list.AddRange(testDataIEnumerable);
			}
			else
			{
				throw new InvalidCastException("Unknown type of test data");
			}

			// Validate
			var expectedData = testData.Concat(testData).ToArray();
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(expectedData.Length)
				.And.BeLessThan((int)(expectedData.Length * 1.5));
			_ = list.Should().HaveCount(expectedData.Length)
				.And.ContainInConsecutiveOrder(expectedData)
				.And.BeEquivalentTo(expectedData);
		}

		[Theory]
		[MemberData(nameof(AddRangeEmptyCollectionTypes))]
		public void AddRangeShouldDoNothingWhenSourceIsEmpty(string testCase, IEnumerable<int> testData)
		{
			// Prepare
			var list = new RecyclableList<int>();

			// Act
			if (testData is int[] testDataArray)
			{
				list.AddRange(testDataArray);
			}
			else if (testData is List<int> testDataList)
			{
				list.AddRange(testDataList);
			}
			else if (testData is RecyclableArrayList<int> testDataRecyclableArrayList)
			{
				list.AddRange(testDataRecyclableArrayList);
			}
			else if (testData is RecyclableList<int> testDataRecyclableList)
			{
				list.AddRange(testDataRecyclableList);
			}
			else if (testData is IList<int> testDataIList)
			{
				list.AddRange(testDataIList);
			}
			else if (testData is IEnumerable<int> testDataIEnumerable)
			{
				list.AddRange(testDataIEnumerable);
			}
			else
			{
				throw new InvalidCastException("Unknown type of test data");
			}

			// Validate
			_ = list.Capacity.Should().Be(0);
			_ = list.Should().BeEmpty().And.HaveCount(0);
		}

		[Fact]
		public void InsertAtTheBeginningShouldMoveItems()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableList<int>();

			// Act
			_ = Assert.Throws<NotSupportedException>(() => list.Insert(0, -1));
		}

		[Fact]
		public void LongIndexOfShouldReturnCorrectIndexes()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableList<int>(testData);

			// Act & Validate
			foreach (var item in testData)
			{
				// Act
				var index = list.LongIndexOf(item);

				// Validate
				_ = index.Should().Be(item - 1);
			}
		}

		[Fact]
		public void LongIndexOfShouldNotFindNonExistingItems()
		{
			// Prepare
			var testData = _testData;
			using var list = NewRecyclableList;
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act
			foreach (var item in testData)
			{
				// Validate
				_ = list.LongIndexOf(-item).Should().Be(-1);
			}

			// Validate
			_ = list.LongIndexOf(0).Should().Be(-1);
		}

		[Fact]
		public void LongIndexOfShouldNotFindAnythingWhenListEmpty()
		{
			// Prepare
			using var list = new RecyclableList<int>();

			// Validate
			_ = list.LongIndexOf(0).Should().Be(-1);
			_ = list.LongIndexOf(1).Should().Be(-1);
		}

		[Fact]
		public void ConstructorSourceShouldInitializeList()
		{
			// Prepare
			var testData = _testData.ToArray();

			// Act
			var list = new RecyclableList<int>(testData);

			// Validate
			_ = list.Should().HaveCount(testData.Length)
			.And.ContainInConsecutiveOrder(testData)
			.And.BeEquivalentTo(testData);
		}
	}
}
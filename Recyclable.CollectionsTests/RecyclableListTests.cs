using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableListTests
	{
		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldAddItemsInCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();

			// Act
			if (testData is long[] testDataArray)
			{
				list.AddRange(testDataArray);
			}
			else if (testData is List<long> testDataList)
			{
				list.AddRange(testDataList);
			}
			else if (testData is RecyclableList<long> testDataRecyclableList)
			{
				list.AddRange(testDataRecyclableList);
			}
			else if (testData is RecyclableLongList<long> testDataRecyclableLongList)
			{
				list.AddRange(testDataRecyclableLongList);
			}
			else if (testData is IList<long> testDataIList)
			{
				list.AddRange(testDataIList);
			}
			else if (testData is IEnumerable<long> testDataIEnumerable)
			{
				list.AddRange(testDataIEnumerable);
			}
			else
			{
				throw new InvalidCastException("Unknown type of test data");
			}

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount, "when capacity == 0, then we should allocate as much memory as needed, only");
			_ = list.Count.Should().Be((int)itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.EmptySourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldDoNothingWhenSourceIsEmpty(string testCase, IEnumerable<long> testData)
		{
			// Prepare
			using var list = new RecyclableList<long>();

			// Act
			if (testData is long[] testDataArray)
			{
				list.AddRange(testDataArray);
			}
			else if (testData is List<long> testDataList)
			{
				list.AddRange(testDataList);
			}
			else if (testData is RecyclableList<long> testDataRecyclableList)
			{
				list.AddRange(testDataRecyclableList);
			}
			else if (testData is RecyclableLongList<long> testDataRecyclableLongList)
			{
				list.AddRange(testDataRecyclableLongList);
			}
			else if (testData is IList<long> testDataIList)
			{
				list.AddRange(testDataIList);
			}
			else if (testData is IEnumerable<long> testDataIEnumerable)
			{
				list.AddRange(testDataIEnumerable);
			}
			else
			{
				throw new InvalidCastException("Unknown type of test data");
			}

			// Validate
			_ = list.Capacity.Should().Be(0);
			_ = list.Count.Should().Be(0);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldNotOverrideItems(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();

			// Act
			if (testData is long[] testDataArray)
			{
				list.AddRange(testDataArray);
				list.AddRange(testDataArray);
			}
			else if (testData is List<long> testDataList)
			{
				list.AddRange(testDataList);
				list.AddRange(testDataList);
			}
			else if (testData is RecyclableList<long> testDataRecyclableList)
			{
				list.AddRange(testDataRecyclableList);
				list.AddRange(testDataRecyclableList);
			}
			else if (testData is RecyclableLongList<long> testDataRecyclableLongList)
			{
				list.AddRange(testDataRecyclableLongList);
				list.AddRange(testDataRecyclableLongList);
			}
			else if (testData is IList<long> testDataIList)
			{
				list.AddRange(testDataIList);
				list.AddRange(testDataIList);
			}
			else if (testData is IEnumerable<long> testDataIEnumerable)
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

			if (itemsCount > 0)
			{
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount * 2);
			}

			_ = list.Count.Should().Be(expectedData.Length);
			_ = list.Should().ContainInConsecutiveOrder(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			testData = testData.Concat(testData).ToArray();

			// Act
			using var list = new RecyclableList<long>();
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			_ = list.Count.Should().Be((int)(itemsCount * 2));
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddShouldAddItems(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();

			// Act
			foreach (var item in testData)
			{
				list.Add(item);
				_ = list.Count.Should().Be((int)item);
			}

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount);
			_ = list.Count.Should().Be((int)itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddShouldAddItemWhenAfterAddRange(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Act
			using var list = new RecyclableList<long>();
			list.AddRange(testData);
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			_ = list.Count.Should().Be((int)(itemsCount * 2));
			_ = list.Should().ContainInConsecutiveOrder(testData.Concat(testData));
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ClearShouldRemoveAllItems(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData);
			_ = list.Count.Should().Be((int)itemsCount);

			// Act		
			list.Clear();
			list.Clear();

			// Validate			
			_ = list.Count.Should().Be(0);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConsecutiveDisposeShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData);
			list.Dispose();

			// Act
			list.Dispose();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Act
			using var list = new RecyclableList<long>(testData.Concat(testData), initialCapacity: (int)itemsCount * 2);

			// Validate
			_ = list.Count.Should().Be((int)itemsCount * 2);
			_ = list.Should().ContainInConsecutiveOrder(testData.Concat(testData));
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorShouldAddItemsInCorrectOrderWhenSourceIsIEnumerable(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Act
			using var list = new RecyclableList<long>(testData);

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount, "when we succeed in taking count without enumerating, then we allocate as much memory as needed, only");
			_ = list.Count.Should().Be((int)itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorSourceShouldInitializeList(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Act
			using var list = new RecyclableList<long>(testData);

			// Validate
			_ = list.Count.Should().Be((int)itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldFindAllItems(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			if (itemsCount == 0)
			{
				return;
			}

			// Prepare
			using var list = new RecyclableList<long>(testData);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act
			foreach (var item in testData)
			{
				// Validate
				_ = list.Contains(item).Should().BeTrue();
			}
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

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			if (itemsCount == 0)
			{
				return;
			}

			// Prepare
			using var list = new RecyclableList<long>(testData);
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

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void CopyToShouldCopyAllItemsInTheCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData);
			long[] copiedItems = new long[itemsCount];

			// Act
			list.CopyTo(copiedItems, 0);

			// Validate
			_ = copiedItems.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void EnumerateShouldYieldAllItemsInCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData);

			// Act
			var yieldedItems = new List<long>((int)itemsCount);
			using var enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				yieldedItems.Add(enumerator.Current);
			}

			// Validate
			_ = yieldedItems.Count.Should().Be((int)itemsCount);
			_ = yieldedItems.Should().ContainInConsecutiveOrder(testData);
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

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void IndexOfShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			if (itemsCount == 0)
			{
				// There is nothing to search for when our lists are empty
				return;
			}

			// Prepare
			using var list = new RecyclableList<long>(testData);
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

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void IndexOfShouldReturnCorrectIndexes(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData);

			// Act & Validate
			foreach (var index in testData)
			{
				var actual = list.IndexOf(index);
				_ = actual.Should().Be((int)(index - 1));
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void InitializeSortClearAddRangeRemoveShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			if (itemsCount == 0)
			{
				return;
			}

			// Act
			using RecyclableList<long> list = new(testData);
			RecyclableListHelpers<long>.QuickSort(list);

			// Validate
			_ = list.Count.Should().Be((int)itemsCount);
			_ = list.Should().Equal(testData);

			// Act
			list.Clear();

			// Validate
			_ = list.Count.Should().Be(0);
			_ = list.Should().BeEmpty();

			// Act
			list.AddRange(testData);

			// Validate
			_ = list.Count.Should().Be((int)itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);

			// Act
			foreach (var item in testData.Reverse())
			{
				_ = list.Remove(item).Should().BeTrue();
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void InsertAtTheBeginningShouldMoveItems(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();

			// Act
			foreach (var item in testData)
			{
				list.Insert(0, item);
				_ = list.Count.Should().Be((int)item);
			}

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount);
			_ = list.Count.Should().Be((int)itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData.Reverse());
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveAtFromTheBeginningShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData);

			// Act & Validate
			for (var deleted = 1; deleted <= itemsCount; deleted++)
			{
				// Act
				list.RemoveAt(0);

				// Validate
				_ = list.Count.Should().Be((int)(itemsCount - deleted));
				_ = list.Should().ContainInConsecutiveOrder(testData.Skip(deleted));
			}

			_ = list.Should().BeEmpty();
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveAtFromTheEndShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData);

			// Act & Validate
			for (var deleted = 1; deleted <= itemsCount; deleted++)
			{
				// Act
				list.RemoveAt(list.Count - 1);

				// Validate
				_ = list.Count.Should().Be((int)(itemsCount - deleted));
				_ = list.Should().ContainInConsecutiveOrder(testData.Take((int)(itemsCount - deleted)));
			}

			_ = list.Should().BeEmpty();
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveFromTheBeginningShouldRemoveTheCorrectItem(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData);

			// Act & Validate
			for (var deleted = 1; deleted <= itemsCount; deleted++)
			{
				// Act
				_ = list.Remove(deleted).Should().BeTrue();

				// Validate
				_ = list.Count.Should().Be((int)(itemsCount - deleted));
				_ = list.Should().ContainInConsecutiveOrder(testData.Skip(deleted));
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveFromTheEndShouldRemoveTheCorrectItem(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Act & Validate
			using var list = new RecyclableList<long>(testData);
			long deleted = 0;
			foreach (var item in testData.Reverse())
			{
				// Act
				_ = list.Remove(item).Should().BeTrue();
				deleted++;

				// Validate
				_ = list.Count.Should().Be((int)(itemsCount - deleted));
				_ = list.Should().ContainInConsecutiveOrder(testData.Take((int)(itemsCount - deleted)));
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void VersionedEnumerateShouldYieldAllItemsInCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData);

			// Act
			var yieldedItems = new List<long>((int)itemsCount);
			foreach (var item in (IRecyclableVersionedList<long>)list)
			{
				yieldedItems.Add(item);
			}

			// Validate
			_ = yieldedItems.Should().ContainInConsecutiveOrder(testData);
		}
	}
}
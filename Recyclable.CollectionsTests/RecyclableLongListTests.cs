using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableLongListTests
	{
		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldAddItemsInCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(minBlockSize: targetBlockSize);

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
			int expectedItemsCount = testData.Count();
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(expectedItemsCount, "when capacity == 0, then we should allocate as much memory as needed, only");
			_ = list.LongCount.Should().Be(expectedItemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.EmptySourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldDoNothingWhenSourceIsEmpty(string testCase, IEnumerable<long> testData)
		{
			// Prepare
			using var list = new RecyclableLongList<long>();

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
			_ = list.LongCount.Should().Be(0);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldNotOverrideItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(minBlockSize: targetBlockSize);

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
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo(targetBlockSize);
			}

			_ = list.LongCount.Should().Be(expectedData.Length);
			_ = list.Should().ContainInConsecutiveOrder(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			testData = testData.Concat(testData).ToArray();

			// Act
			using var list = new RecyclableLongList<long>(minBlockSize: targetBlockSize);
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			_ = list.LongCount.Should().Be(itemsCount * 2);
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddShouldAddItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(minBlockSize: targetBlockSize);

			// Act
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			_ = list.LongCount.Should().Be(itemsCount, "we added so many items");
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void GetByIndexShouldReturnCorrectItem(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Act & Validate
			foreach (var item in testData)
			{
				_ = list[item - 1].Should().Be(item);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		//[InlineData("Debugging case", new long[1] { 1 }, 1, 2)]
		public void AddShouldAddItemWhenAfterAddRange(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Act
			using var list = new RecyclableLongList<long>(minBlockSize: targetBlockSize);
			list.AddRange(testData);
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			_ = list.LongCount.Should().Be(itemsCount * 2);
			_ = list.Should().ContainInConsecutiveOrder(testData.Concat(testData));
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ClearShouldRemoveAllItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, expectedItemsCount: itemsCount);
			_ = list.LongCount.Should().Be(itemsCount);

			// Act		
			list.Clear();
			list.Clear();

			// Validate			
			_ = list.LongCount.Should().Be(0);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConsecutiveDisposeShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);
			list.Dispose();

			// Act
			list.Dispose();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			testData = testData.Concat(testData).ToArray();

			// Act
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Validate
			_ = list.LongCount.Should().Be(itemsCount * 2);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorShouldAddItemsInCorrectOrderWhenSourceIsIEnumerable(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Act
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount, "when we succeed in taking count without enumerating, then we allocate as much memory as needed, only");
			_ = list.LongCount.Should().Be(itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorSourceShouldInitializeList(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Act
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Validate
			_ = list.LongCount.Should().Be(itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldFindAllItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			if (itemsCount == 0)
			{
				// There is nothing to search for when our lists are empty
				return;
			}

			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act
			foreach (var item in testData)
			{
				// Validate
				_ = list.Contains(item).Should().BeTrue($"we searched for {item}");
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.TargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldNotFindAnythingWhenListEmpty(long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<int>(minBlockSize: targetBlockSize, expectedItemsCount: itemsCount);

			// Validate
			_ = list.Contains(0).Should().BeFalse();
			_ = list.Contains(1).Should().BeFalse();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			if (itemsCount == 0)
			{
				// There is nothing to search for when our lists are empty
				return;
			}

			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act
			foreach (var item in testData)
			{
				// Validate
				_ = list.Contains(-item).Should().BeFalse();
			}

			// Validate - this case is special, because this is the default item value
			_ = list.Contains(0).Should().BeFalse();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		//[InlineData("Debuging case", new[] { 1L, 2, }, 2, 2)]
		public void CopyToShouldCopyAllItemsInTheCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Act
			var actual = new long[itemsCount];
			list.CopyTo(actual, 0);
			_ = actual.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void DisposeShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Act
			list.Dispose();

			// Validate
			_ = list.LongCount.Should().Be(0L);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void EnumerateShouldYieldAllItemsInCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Act
			var actual = new List<long>();
			using var enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				actual.Add(enumerator.Current);
			}

			// Validate
			_ = actual.Should().ContainInConsecutiveOrder(testData);
		}

		[Fact]
		public void IndexOfShouldNotFindAnythingWhenListEmpty()
		{
			// Prepare
			using var list = new RecyclableLongList<int>();

			// Validate
			_ = list.IndexOf(0).Should().Be(-1);
			_ = list.IndexOf(1).Should().Be(-1);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void IndexOfShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			if (itemsCount == 0)
			{
				// There is nothing to search for when our lists are empty
				return;
			}

			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);
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
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void IndexOfShouldReturnCorrectIndexes(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Act & Validate
			foreach (var item in testData)
			{
				var actual = list.IndexOf(item);
				_ = actual.Should().Be((int)item - 1);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void InitializeSortClearAddRangeRemoveShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			if (itemsCount == 0)
			{
				return;
			}

			// Act
			using RecyclableLongList<long> list = new(testData, minBlockSize: targetBlockSize);
			list.QuickSort();

			// Validate
			_ = list.LongCount.Should().Be(itemsCount);
			_ = list.Should().Equal(testData);

			// Act
			list.Clear();

			// Validate
			_ = list.LongCount.Should().Be(0);
			_ = list.Should().BeEmpty();

			// Act
			list.AddRange(testData);

			// Validate
			_ = list.LongCount.Should().Be(itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);

			// Act
			foreach (var item in testData.Reverse())
			{
				_ = list.Remove(item).Should().BeTrue();
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void InsertAtTheBeginningShouldMoveItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Act
			_ = Assert.Throws<NotSupportedException>(() => list.Insert(0, -1));
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void InsertShouldRaiseNotSupportedException(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Act
			_ = Assert.Throws<NotSupportedException>(() => list.Insert(0, -1));
		}

		[Fact]
		public void LongIndexOfShouldNotFindAnythingWhenListEmpty()
		{
			// Prepare
			using var list = new RecyclableLongList<long>();

			// Validate
			_ = list.LongIndexOf(0).Should().Be(-1);
			_ = list.LongIndexOf(1).Should().Be(-1);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void LongIndexOfShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			if (itemsCount == 0)
			{
				return;
			}

			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);
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

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void LongIndexOfShouldReturnCorrectIndexes(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Act & Validate
			foreach (var item in testData)
			{
				// Act
				var index = list.LongIndexOf(item);

				// Validate
				_ = index.Should().Be(item - 1);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveAtFromTheBeginningShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

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
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveAtFromTheEndShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

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
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveFromTheBeginningShouldRemoveTheCorrectItem(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

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
		[MemberData(nameof(RecyclableLongListTestData.SourceTargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveFromTheEndShouldRemoveTheCorrectItem(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);
			var expectedData = testData.ToArray();
			int removedCount = 0;
			foreach (var item in testData.Reverse())
			{
				// Act & Validate
				_ = list.Remove(item).Should().BeTrue($"we search for {item} which should exist");
				removedCount++;
				var expectedList = expectedData[0..((int)itemsCount - removedCount)];
				_ = list.LongCount.Should().Be(expectedList.Length);
				_ = list.Should().ContainInConsecutiveOrder(expectedList);
			}

			_ = list.Should().BeEmpty();
		}
	}
}
using FluentAssertions;
using Recyclable.Collections;
using Recyclable.Collections.Linq;
using System.Numerics;

#pragma warning disable xUnit1026, RCS1163, IDE0060, RCS1235

namespace Recyclable.CollectionsTests
{
	public class RecyclableLongListTests
	{
		private static long CalculateExpectedCapacity(string testCase, long itemsCount, int targetBlockSize)
		{
			var expectedCapacity = (long)BitOperations.RoundUpToPowerOf2((uint)itemsCount + 1);
			if (testCase.Contains("non-enumerated"))
			{
				expectedCapacity = (long)BitOperations.RoundUpToPowerOf2((ulong)itemsCount + RecyclableDefaults.BlockSize);
				expectedCapacity = expectedCapacity >= RecyclableDefaults.InitialCapacity ? expectedCapacity : RecyclableDefaults.InitialCapacity;
			}
			else if (targetBlockSize > itemsCount)
			{
				expectedCapacity = ((itemsCount / targetBlockSize) + (itemsCount % targetBlockSize > 0 ? 1 : 0)) * targetBlockSize;
			}

			return expectedCapacity;
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount / list.BlockSize) - (itemsCount % list.BlockSize != 0 ? 0 : 1));
			_ = list.LongCount.Should().Be(expectedItemsCount);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.EmptySourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldDoNothingWhenSourceIsEmpty(string testCase, IEnumerable<long> testData, long itemsCount)
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
			_ = list.LastBlockWithData.Should().Be(-1);
			_ = list.LongCount.Should().Be(0);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount * 2 / list.BlockSize) - (itemsCount * 2 % list.BlockSize != 0 ? 0 : 1));
			_ = list.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount * 2 / list.BlockSize) - (itemsCount * 2 % list.BlockSize != 0 ? 0 : 1));
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount / list.BlockSize) - (itemsCount % list.BlockSize == 0 ? 1 : 0));
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		//[InlineData("Debugging case", new long[1] { 1 }, 1, 2)]
		public void AddShouldAddItemWhenAfterAddRange(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Act
			using var list = new RecyclableLongList<long>(minBlockSize: targetBlockSize, itemsCount);
			list.AddRange(testData);
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			_ = list.LongCount.Should().Be(itemsCount * 2);
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount * 2 / list.BlockSize) - (itemsCount * 2 % list.BlockSize != 0 ? 0 : 1));
			_ = list.Should().Equal(testData.Concat(testData));
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ClearShouldRemoveAllItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			_ = list.LongCount.Should().Be(itemsCount);

			// Act		
			list.Clear();
			list.Clear();

			// Validate			
			_ = list.LongCount.Should().Be(0);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConsecutiveDisposeShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			list.Dispose();

			// Act
			list.Dispose();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			testData = testData.Concat(testData).ToArray();

			// Act
			using var list = new RecyclableLongList<long>(expectedData, minBlockSize: targetBlockSize, itemsCount);

			// Validate
			_ = list.LongCount.Should().Be(itemsCount * 2);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorSourceShouldInitializeList(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Act
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount, "when we succeed in taking count without enumerating, then we allocate as much memory as needed, only");
			_ = list.LongCount.Should().Be(itemsCount);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldFindAllItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			// Prepare
			if (itemsCount == 0)
			{
				// There is nothing to search for when our lists are empty
				return;
			}

			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.Contains(list[itemIndex]).Should().BeTrue($"we searched for {list[itemIndex]}");
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.TargetDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldNotFindAnythingWhenListEmpty(long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<int>(minBlockSize: targetBlockSize, itemsCount);

			// Validate
			_ = list.Contains(0).Should().BeFalse();
			_ = list.Contains(1).Should().BeFalse();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			// Prepare
			if (itemsCount == 0)
			{
				// There is nothing to search for when our lists are empty
				return;
			}

			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.Contains(-list[itemIndex]).Should().BeFalse();
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		//[InlineData("Debugging case", new[] { 1L, 2, }, 2, 2)]
		public void CopyToShouldCopyAllItemsInTheCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act
			var actual = new long[itemsCount];
			list.CopyTo(actual, 0);
			_ = actual.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void DisposeShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act
			list.Dispose();

			// Validate
			_ = list.LongCount.Should().Be(0L);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void EnumerateShouldYieldAllItemsInCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act
			var actual = new List<long>((int)itemsCount);
			using var enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				actual.Add(enumerator.Current);
			}

			// Validate
			_ = actual.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void GetByIndexShouldReturnCorrectItem(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act & Validate
			foreach (var item in testData)
			{
				_ = list[item - 1].Should().Be(item);
			}
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void IndexOfShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			// Prepare
			if (itemsCount == 0)
			{
				// There is nothing to search for when our lists are empty
				return;
			}

			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.IndexOf(-list[itemIndex]).Should().Be(-1);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void IndexOfShouldReturnCorrectIndexes(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.IndexOf(list[itemIndex]).Should().Be((int)itemIndex);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void InitializeSortClearAddRangeRemoveShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			if (itemsCount == 0)
			{
				return;
			}

			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				using var testDataList = testData.ToRecyclableList();
				var item = testDataList[(int)itemIndex];

				// Act
				using RecyclableLongList<long> list = new(testData, minBlockSize: targetBlockSize);
				QuickSortExtensions<long>.QuickSort(list);

				// Validate
				_ = list.LongCount.Should().Be(itemsCount);
				_ = list.Should().Equal(testDataList);

				// Act
				list.Clear();

				// Validate
				_ = list.LongCount.Should().Be(0);
				_ = list.Should().BeEmpty();

				// Act
				list.AddRange(testData);

				// Validate
				_ = list.LongCount.Should().Be(itemsCount);
				_ = list.Should().Equal(testDataList);

				// Prepare
				testDataList.RemoveAt((int)itemIndex);

				// Act
				_ = list.Remove(item).Should().BeTrue();
				_ = list.LongIndexOf(item).Should().Be(RecyclableDefaults.ItemNotFoundIndexLong);
				_ = list.Contains(item).Should().BeFalse();
				_ = list.LongCount.Should().Be(itemsCount - 1);
				_ = list.Should().Equal(testDataList);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void InsertShouldMoveItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				using var list = new RecyclableLongList<long>(testData, targetBlockSize, itemsCount);
				long expectedCapacity = CalculateExpectedCapacity(testCase, itemsCount, targetBlockSize);

				var item = list[itemIndex];
				var expectedItems = list.ToList();
				expectedItems.Insert((int)itemIndex, item);

				// Act
				list.Insert(itemIndex, item);

				// Validate

				_ = list[itemIndex].Should().Be(item);
				_ = list[itemIndex + 1].Should().Be(item);
				_ = list.Capacity.Should().Be(expectedCapacity);
				_ = list.Count.Should().Be((int)itemsCount + 1);
				_ = list.Should().Equal(expectedItems);
			}
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void LongIndexOfShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			if (itemsCount == 0)
			{
				return;
			}

			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.LongIndexOf(-list[itemIndex]).Should().Be(-1);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void LongIndexOfShouldReturnCorrectIndexes(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.LongIndexOf(list[itemIndex]).Should().Be(itemIndex);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveAtShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);
				var expectedItems = list.ToList();
				expectedItems.RemoveAt((int)itemIndex);

				// Act
				list.RemoveAt(itemIndex);

				// Validate
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount);
				_ = list.LastBlockWithData.Should().Be((int)(list.LongCount >> list.BlockSizePow2BitShift) - ((list.LongCount & list.BlockSizeMinus1) != 0 ? 0 : 1));
				_ = list.LongCount.Should().Be(itemsCount - 1);
				_ = list.Should().Equal(expectedItems);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveShouldRemoveTheCorrectItem(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);
				var expectedItems = list.ToList();
				var item = expectedItems[(int)itemIndex];
				expectedItems.RemoveAt((int)itemIndex);

				// Act
				_ = list.Remove(item).Should().BeTrue();

				// Validate
				_ = list.LongCount.Should().Be(itemsCount - 1);
				_ = list.LastBlockWithData.Should().Be((int)(list.LongCount >> list.BlockSizePow2BitShift) - ((list.LongCount & list.BlockSizeMinus1) != 0 ? 0 : 1));
				_ = list.Should().Equal(expectedItems);
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void VersionedEnumerateShouldYieldAllItemsInCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act
			using var actual = new RecyclableList<long>((int)itemsCount);
			foreach (var item in (IRecyclableVersionedLongList<long>)list)
			{
				actual.Add(item);
			}

			// Validate
			_ = actual.Should().Equal(testData);
		}
	}
}
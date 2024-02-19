using FluentAssertions;
using Recyclable.Collections;
using Recyclable.Collections.TestData;
using Recyclable.Collections.Linq;
using System.Collections;
using System.Numerics;
using Recyclable.Collections.TestData.xUnit;
using Open.Collections;

#pragma warning disable xUnit1026, RCS1163, IDE0060, RCS1235

namespace Recyclable.CollectionsTests
{
	public class RecyclableLongListTests
	{
		private static long CalculateExpectedCapacity(string testCase, long itemsCount, int targetBlockSize)
		{
			var expectedCapacity = (long)BitOperations.RoundUpToPowerOf2((uint)itemsCount + 1);
			if (targetBlockSize > itemsCount)
			{
				expectedCapacity = ((itemsCount / targetBlockSize) + (itemsCount % targetBlockSize > 0 ? 1 : 0)) * targetBlockSize;
			}

			return expectedCapacity;
		}

		[Fact]
		public void AddRangeShouldAcceptNulls()
		{
			// Prepare
			using var list = new RecyclableLongList<long?>();

			// Act
			list.AddRange(new long?[] { null, default });

			// Validate
			_ = list.Should().HaveCount(2).And.AllSatisfy(x => x.Should().BeNull());
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void AddRangeShouldAddItemsInCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(minBlockSize: targetBlockSize);

			// Act
			if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((Array)testData);
			}
			else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((ICollection)testData);
			}
			else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((ICollection<long>)testData);
			}
			else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((IEnumerable)testData);
			}
			else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((IReadOnlyList<long>)testData);
			}
			else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange(new ReadOnlySpan<long>((long[])testData));
			}
			else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange(new Span<long>((long[])testData));
			}
			else if (testData is long[] testDataArray)
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
			long expectedItemsCount = itemsCount;
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(expectedItemsCount);
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount / targetBlockSize) - (itemsCount % targetBlockSize != 0 ? 0 : 1));
			_ = list.LongCount.Should().Be(expectedItemsCount);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[ClassData(typeof(EmptySourceDataTheoryData))]
		public void AddRangeShouldDoNothingWhenSourceIsEmpty(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableLongList<long>();

			// Act
			if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((Array)testData);
			}
			else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((ICollection)testData);
			}
			else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((ICollection<long>)testData);
			}
			else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((IEnumerable)testData);
			}
			else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((IReadOnlyList<long>)testData);
			}
			else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange(new ReadOnlySpan<long>((long[])testData));
			}
			else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange(new Span<long>((long[])testData));
			}
			else if (testData is long[] testDataArray)
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
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(RecyclableDefaults.InitialCapacity);
			_ = list.LastBlockWithData.Should().Be(-1);
			_ = list.LongCount.Should().Be(itemsCount);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void AddRangeShouldNotOverrideItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using RecyclableLongList<long> list = new(minBlockSize: targetBlockSize);

			// Act
			if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((Array)testData);
				list.AddRange((Array)testData);
			}
			else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((ICollection)testData);
				list.AddRange((ICollection)testData);
			}
			else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((ICollection<long>)testData);
				list.AddRange((ICollection<long>)testData);
			}
			else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((IEnumerable)testData);
				list.AddRange((IEnumerable)testData);
			}
			else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange((IReadOnlyList<long>)testData);
				list.AddRange((IReadOnlyList<long>)testData);
			}
			else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
			{
				var readOnlySpan = new ReadOnlySpan<long>((long[])testData);
				list.AddRange(readOnlySpan);
				list.AddRange(readOnlySpan);
			}
			else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
			{
				var readOnlySpan = new Span<long>((long[])testData);
				list.AddRange(readOnlySpan);
				list.AddRange(readOnlySpan);
			}
			else if (testData is long[] testDataArray)
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
			var expectedData = testData.Concat(testData);
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount * 2);
			_ = list.LongCount.Should().Be(itemsCount * 2);
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount * 2 / list.BlockSize) - (itemsCount * 2 % list.BlockSize != 0 ? 0 : 1));
			_ = list.Should().Equal(expectedData);
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void AddShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			testData = testData.Concat(testData);

			// Act
			using var list = new RecyclableLongList<long>(minBlockSize: targetBlockSize);
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount * 2);
			_ = list.LongCount.Should().Be(itemsCount * 2);
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount * 2 / list.BlockSize) - (itemsCount * 2 % list.BlockSize != 0 ? 0 : 1));
			_ = list.Should().Equal(testData);
		}

		[Fact]
		public void AddShouldAcceptNulls()
		{
			// Prepare
#pragma warning disable IDE0028
			using var list = new RecyclableLongList<long?>();

			// Act
			list.Add(null);
			list.Add(default);
#pragma warning restore IDE0028

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(RecyclableDefaults.BlockSize);
			_ = list.Should().AllSatisfy(x => x.Should().BeNull());
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void AddShouldAddItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(minBlockSize: targetBlockSize);

			// Act
			foreach (var index in testData)
			{
				list.Add(index);
				_ = list.LongCount.Should().Be(index);
			}

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.LongCount.Should().Be(itemsCount, "we added so many items");
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount / list.BlockSize) - (itemsCount % list.BlockSize == 0 ? 1 : 0));
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
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
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount * 2);
			_ = list.LongCount.Should().Be(itemsCount * 2);
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount * 2 / list.BlockSize) - (itemsCount * 2 % list.BlockSize != 0 ? 0 : 1));
			_ = list.Should().Equal(testData.Concat(testData));
		}

		[Theory]
		[ClassData(typeof(SourceRefDataWithBlockSizeTheoryData))]
		public void ClearShouldClearItemsWhenReferenceType(string testCase, IEnumerable<object> testData, long itemsCount, int targetBlockSize)
		{
			if (itemsCount == 0)
			{
				/// Clearing empty list is tested in <see cref="ClearShouldSucceed"/>.
				return;
			}

			// Arrange
			using var list = new RecyclableLongList<object>(testData, minBlockSize: targetBlockSize, itemsCount);
			var blocks = list.AsArray.ToArray();
			_ = blocks.Should().AllBeOfType<object[]>();
			var beforeItems = blocks.SelectMany(x => x.Take(targetBlockSize)).Take((int)itemsCount).ToArray();
			_ = beforeItems.Should().AllSatisfy(x => x.Should().NotBeNull());

			// Act
			list.Clear();

			// Assert
			var afterItems = blocks.SelectMany(x => x.Take(targetBlockSize)).Take((int)itemsCount).ToArray();
			_ = afterItems.Should().AllSatisfy(x => x.Should().BeNull());
			_ = list.AsArray.Should().AllSatisfy(x => x.Should().BeNull());
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void ClearShouldRemoveAllItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			_ = list.LongCount.Should().Be(itemsCount);

			// Act		
			list.Clear();
			list.Clear();

			// Validate			
			_ = list.Capacity.Should().Be(0);
			_ = list.LastBlockWithData.Should().Be(-1);
			_ = list.LongCount.Should().Be(0);
			_ = list.Should().BeEmpty();
		}

		[Fact]
		public void ClearShouldReturnPooledArraysOnlyOnce()
		{
			// Arrange
			var testData = RecyclableLongListTestData.CreateTestData(RecyclableDefaults.MinPooledArrayLength * 10);
			using RecyclableLongList<long> firstInstances = new(testData, RecyclableDefaults.MinPooledArrayLength);
			var firstArrays = firstInstances.AsArray.ToArray();

			// Act
			firstInstances.Clear();

			// Assert
			using RecyclableLongList<long> sameInstances = new(testData, RecyclableDefaults.MinPooledArrayLength);
			_ = sameInstances.AsArray.Should().BeEquivalentTo(firstArrays.Reverse());

			using RecyclableLongList<long> otherInstances = new(testData, RecyclableDefaults.MinPooledArrayLength);
			_ = otherInstances.AsArray.Should().NotIntersectWith(firstArrays);
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void ConsecutiveDisposeShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			list.Dispose();

			// Act
			list.Dispose();
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void ConstructorShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			testData = testData.Concat(testData);

			// Act
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount * 2);
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount * 2 / targetBlockSize) - (itemsCount * 2 % targetBlockSize > 0 ? 0 : 1));
			_ = list.LongCount.Should().Be(itemsCount * 2);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void ConstructorSourceShouldInitializeList(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Act
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount / targetBlockSize) - (itemsCount % targetBlockSize > 0 ? 0 : 1));
			_ = list.LongCount.Should().Be(itemsCount);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void ContainsShouldFindAllItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.Contains(list[itemIndex]).Should().BeTrue($"we searched for {list[itemIndex]}");
			}
		}

		[Theory]
		[ClassData(typeof(ItemsCountWithBlockSizeTheoryData))]
		public void ContainsShouldNotFindAnythingWhenListEmpty(long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<int>(minBlockSize: targetBlockSize, itemsCount);

			// Validate
			_ = list.Contains(0).Should().BeFalse();
			_ = list.Contains(1).Should().BeFalse();
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void ContainsShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.Contains(-list[itemIndex]).Should().BeFalse();
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		//[InlineData("Debugging case", new[] { 1L, 2, }, 2, 2)]
		public void CopyToShouldCopyAllItemsInTheCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			long[] copiedItems = new long[itemsCount];

			// Act
			list.CopyTo(copiedItems, 0);
			
			// Validate
			_ = copiedItems.Should().Equal(testData);
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void CopyToShouldCopyAllItemsInTheCorrectOrderWhenConstrainedRange(string testCase, IEnumerable<long> testData, int itemsCount, int minBlockSize, in long[] itemIndexes)
		{
			// Prepare
			var expectedData = testData.ToList();
			TestCopyTo(0);

			foreach (var itemIndex in itemIndexes)
			{
				TestCopyTo((int)itemIndex);
				TestCopyTo((int)itemIndex);
			}

			void TestCopyTo(int itemIndex)
			{
				using var list = new RecyclableLongList<long>(testData, minBlockSize, initialCapacity: itemsCount);
				long[] expectedItems = new long[itemsCount + itemIndex];
				expectedData.CopyTo(expectedItems, itemIndex);
				long[] actualItems = new long[itemsCount + itemIndex];

				// Act
				list.CopyTo(actualItems, itemIndex);

				// Validate
				_ = actualItems.Should().Equal(expectedItems);
			}
		}

		[Theory]
		[ClassData(typeof(SourceRefDataWithBlockSizeTheoryData))]
		public void DisposeShouldClearItemsWhenReferenceType(string testCase, IEnumerable<object> testData, long itemsCount, int targetBlockSize)
		{
			if (itemsCount == 0)
			{
				/// Disposing empty list is tested in <see cref="DisposeShouldSucceed"/>.
				return;
			}	

			// Arrange
			using var list = new RecyclableLongList<object>(testData, targetBlockSize, itemsCount);
			var blocks = list.AsArray.ToArray();
			_ = blocks.Should().AllBeOfType<object[]>();

			_ = blocks.SelectMany(x => x.Take(targetBlockSize)).Take((int)itemsCount).Should().AllSatisfy(x => x.Should().NotBeNull());

			// Act
			list.Dispose();

			// Assert
			_ = blocks.SelectMany(x => x.Take(targetBlockSize)).Take((int)itemsCount).Should().AllSatisfy(x => x.Should().BeNull());
			_ = list.AsArray.Should().AllSatisfy(x => x.Should().BeNull());
		}

		[Fact]
		public void DisposeShouldReturnPooledArrayOnlyOnce()
		{
			// Arrange
			var testData = RecyclableLongListTestData.CreateTestData(RecyclableDefaults.MinPooledArrayLength * 10);
			using RecyclableLongList<long> firstInstances = new(testData, RecyclableDefaults.MinPooledArrayLength);
			var firstArrays = firstInstances.AsArray.ToArray();

			// Act
			firstInstances.Dispose();

			// Assert
			using RecyclableLongList<long> sameInstances = new(testData, RecyclableDefaults.MinPooledArrayLength);
			_ = sameInstances.AsArray.Should().BeEquivalentTo(firstArrays);

			using RecyclableLongList<long> otherInstances = new(testData, RecyclableDefaults.MinPooledArrayLength);
			_ = otherInstances.AsArray.Should().NotIntersectWith(firstArrays);
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void DisposeShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act
			list.Dispose();

			// Validate
			_ = list.Capacity.Should().Be(0);
			_ = list.LastBlockWithData.Should().Be(-1);
			_ = list.LongCount.Should().Be(0);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void EnumerateShouldYieldAllItemsInCorrectOrder(string testCase, IEnumerable<long> testData, int itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act
			var yieldedItems = new List<long>(itemsCount);
			using var enumerator = list.GetEnumerator();
			while (enumerator.MoveNext())
			{
				yieldedItems.Add(enumerator.Current);
			}

			// Validate
			_ = yieldedItems.Should().Equal(testData);
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void GetByIndexShouldReturnCorrectItem(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act & Validate
			foreach (long item in testData)
			{
				_ = list[(int)item - 1].Should().Be(item);
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void GetByIndexShouldReturnCorrectItemWhenLong(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act & Validate
			foreach (long item in testData)
			{
				_ = list[item - 1].Should().Be(item);
			}
		}

		[Theory]
		[ClassData(typeof(BigSourceDataWithOutOfRangeItemIndexesWithRangeTheoryData))]
		public void IndexOfParallelShouldNotFindAnythingWhenBeyondRange(string testCase, IEnumerable<long> testData, long itemsCount, int blockSize, IEnumerable<(long itemIndex, long rangeStartItemIndex, long rangeItemsCount)> itemIndexesWithRanges)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: blockSize, initialCapacity: itemsCount);
			using var expectedData = testData.ToRecyclableLongList();

			foreach (var (itemIndex, rangeStartItemIndex, rangedItemsCount) in itemIndexesWithRanges)
			{
				// Act & Validate
				_ = list.IndexOf(expectedData[itemIndex], (int)rangeStartItemIndex, (int)rangedItemsCount).Should().Be(-1);
			}
		}

		[Theory]
		[ClassData(typeof(BlockSizeTheoryData))]
		public void IndexOfParallelShouldNotFindNonExistingItems(int blockSize)
		{			
			const long ItemsCount = RecyclableDefaults.MinItemsCountForParallelization * 3;

			// Prepare
			var testData = RecyclableLongListTestData.CreateTestData(ItemsCount).ToList();
			using var list = new RecyclableLongList<long>(testData, minBlockSize: blockSize, initialCapacity: ItemsCount);

			// Act & Validate
			_ = list.IndexOf(0).Should().Be(-1);

			var itemIndexes = RecyclableLongListTestData.CreateItemIndexVariants(ItemsCount, blockSize);
			foreach (var itemIndex in itemIndexes)
			{
				var item = -testData[(int)itemIndex];

				// Act & Validate
				_ = list.IndexOf(item).Should().Be(-1);
				_ = list.IndexOf(item, (int)itemIndex).Should().Be(-1);
				_ = list.IndexOf(-item, (int)itemIndex, 0).Should().Be(-1);
				_ = list.IndexOf(0, (int)itemIndex).Should().Be(-1);
				_ = list.IndexOf(0, (int)itemIndex, 0).Should().Be(-1);
				_ = list.IndexOf(item, (int)ItemsCount, 0).Should().Be(-1);

				if (itemIndex > 0)
				{
					_ = list.IndexOf(testData[(int)itemIndex - 1], (int)itemIndex).Should().Be(-1);
				}
			}
		}

		[Theory]
		[ClassData(typeof(BlockSizeTheoryData))]
		public void IndexOfParallelShouldNotFindNonExistingItemsWhenConstrainedCount(int blockSize)
		{			
			const long ItemsCount = RecyclableDefaults.MinItemsCountForParallelization * 3;

			// Prepare
			var testData = RecyclableLongListTestData.CreateTestData(ItemsCount).ToArray();
			using var list = new RecyclableLongList<long>(testData, minBlockSize: blockSize, initialCapacity: ItemsCount);

			var itemIndexes = RecyclableLongListTestData.CreateItemIndexVariants(ItemsCount, blockSize);
			foreach (var itemIndexForGenerator in itemIndexes)
			{
				var itemRanges = RecyclableLongListTestData.CombineItemIndexWithRange(itemIndexForGenerator, ItemsCount);
				foreach (var (itemIndex, rangedItemsCount) in itemRanges)
				{
					// Act & Validate
					_ = list.IndexOf(-testData[itemIndex], (int)itemIndex, (int)rangedItemsCount).Should().Be(-1);
					_ = list.IndexOf(0, (int)itemIndex, (int)rangedItemsCount).Should().Be(-1);
				}
			}
		}

		[Theory]
		[ClassData(typeof(BlockSizeTheoryData))]
		public void IndexOfParallelShouldReturnCorrectIndexes(int blockSize)
		{			
			const long ItemsCount = RecyclableDefaults.MinItemsCountForParallelization * 3;

			// Prepare
			var testData = RecyclableLongListTestData.CreateTestData(ItemsCount).ToArray();
			using var list = new RecyclableLongList<long>(testData, minBlockSize: blockSize, initialCapacity: ItemsCount);

			// Act & Validate
			IEnumerable<long> itemIndexes = RecyclableLongListTestData.CreateItemIndexVariants(ItemsCount, blockSize);
			foreach (var itemIndex in itemIndexes)
			{
				var actual = list.IndexOf(testData[itemIndex]);
				_ = actual.Should().Be((int)itemIndex);
			}
		}

		[Theory]
		[ClassData(typeof(BlockSizeTheoryData))]
		public void IndexOfParallelShouldReturnCorrectIndexesWhenConstrainedCount(int blockSize)
		{
			const long ItemsCount = RecyclableDefaults.MinItemsCountForParallelization * 3;

			// Prepare
			var testData = RecyclableLongListTestData.CreateTestData(ItemsCount).ToArray();
			using var list = new RecyclableLongList<long>(testData, minBlockSize: blockSize, initialCapacity: ItemsCount);

			var itemIndexes = RecyclableLongListTestData.CreateItemIndexVariants(ItemsCount, blockSize);
			foreach (var itemIndexForGenerator in itemIndexes)
			{
				var itemRanges = RecyclableLongListTestData.CombineItemIndexWithRange(itemIndexForGenerator, ItemsCount);
				foreach (var (itemIndex, rangedItemsCount) in itemRanges)
				{
					// Act & Validate
					if (rangedItemsCount > 0)
					{
						// Find first item in range
						_ = list.IndexOf(testData[itemIndex], (int)itemIndex, (int)rangedItemsCount).Should().Be((int)itemIndex);

						// Find last item in range
						_ = list.IndexOf(testData[itemIndex + rangedItemsCount - 1], (int)itemIndex, (int)rangedItemsCount).Should().Be((int)(itemIndex + rangedItemsCount - 1));
					}
				}
			}
		}

		[Theory]
		[ClassData(typeof(BlockSizeTheoryData))]
		public void IndexOfParallelShouldReturnCorrectIndexesWhenConstrainedIndex(int blockSize)
		{
			const long ItemsCount = RecyclableDefaults.MinItemsCountForParallelization * 3;

			// Prepare
			var testData = RecyclableLongListTestData.CreateTestData(ItemsCount).ToArray();
			using var list = new RecyclableLongList<long>(testData, minBlockSize: blockSize, initialCapacity: ItemsCount);

			var itemIndexes = RecyclableLongListTestData.CreateItemIndexVariants(ItemsCount, blockSize);
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.IndexOf(testData[itemIndex], (int)itemIndex).Should().Be((int)itemIndex);
				_ = list.IndexOf(testData[^1], (int)itemIndex).Should().Be(list.Count - 1);
			}
		}

		[Theory]
		[ClassData(typeof(BlockSizeTheoryData))]
		public void IndexOfParallelShouldThrowWhenInvalidRange(int blockSize)
		{
			const long ItemsCount = RecyclableDefaults.MinItemsCountForParallelization * 3;

			// Prepare
			var testData = RecyclableLongListTestData.CreateTestData(ItemsCount).ToList();
			using var list = new RecyclableLongList<long>(testData, minBlockSize: blockSize, initialCapacity: ItemsCount);

			var item = testData[0];
			Assert.Throws<ArgumentOutOfRangeException>(() => testData.IndexOf(item, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(item, -1));

			Assert.Throws<ArgumentOutOfRangeException>(() => testData.IndexOf(item, (int)ItemsCount + 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(item, (int)ItemsCount + 1));

			Assert.Throws<ArgumentOutOfRangeException>(() => testData.IndexOf(item, (int)ItemsCount, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => list.IndexOf(item, (int)ItemsCount, 1));
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexWithRangeTheoryData))]
		public void IndexOfShouldNotFindAnythingWhenBeyondRange(string testCase, IEnumerable<long> testData, int itemsCount, int minBlockSize, in (long ItemIndex, long RangedItemsCount)[] itemRanges)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: minBlockSize, initialCapacity: itemsCount);
			var expectedData = testData.ToArray();

			foreach (var (itemIndex, rangedItemsCount) in itemRanges)
			{
				// Try finding the item prior the given itemIndex
				if (itemIndex > 0)
				{
					_ = list.IndexOf(expectedData[(int)(itemIndex - 1)], (int)itemIndex, (int)rangedItemsCount).Should().Be(-1);
				}

				// Try finding the item after the given item range
				if (itemIndex + rangedItemsCount < itemsCount)
				{
					_ = list.IndexOf(expectedData[(int)(itemIndex + rangedItemsCount)], (int)itemIndex, (int)rangedItemsCount).Should().Be(-1);
				}
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
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void IndexOfShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");
			var expectedData = testData.ToArray();

			// Act & Validate
			_ = list.IndexOf(0).Should().Be(-1);

			foreach (var itemIndex in itemIndexes)
			{
				var item = -expectedData[itemIndex];

				_ = list.IndexOf(item).Should().Be(-1);
				_ = list.IndexOf(item, (int)itemIndex).Should().Be(-1);
				_ = list.IndexOf(-item, (int)itemIndex, 0).Should().Be(-1);
				_ = list.IndexOf(0, (int)itemIndex).Should().Be(-1);
				_ = list.IndexOf(0, (int)itemIndex, 0).Should().Be(-1);

				if (itemIndex > 0)
				{
					_ = list.IndexOf(list[itemIndex - 1], (int)itemIndex).Should().Be(-1);
				}
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void IndexOfShouldReturnCorrectIndexes(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				int actual = list.IndexOf(list[itemIndex]);
				_ = actual.Should().Be((int)itemIndex);
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexWithRangeTheoryData))]
		public void IndexOfShouldReturnCorrectIndexesWhenConstrainedCount(string testCase, IEnumerable<long> testData, int itemsCount, int minBlockSize, in (long ItemIndex, long RangedItemsCount)[] itemRanges)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: minBlockSize, itemsCount);
			var expectedData = testData.ToList();

			// Act & Validate
			foreach (var (itemIndex, rangedItemsCount) in itemRanges)
			{
				// Act & Validate
				if (rangedItemsCount > 0)
				{
					// Find first item in range
					_ = list.IndexOf(expectedData[(int)itemIndex], (int)itemIndex, (int)rangedItemsCount).Should().Be((int)itemIndex);

					// Find last item in range
					_ = list.IndexOf(expectedData[(int)(itemIndex + rangedItemsCount - 1)], (int)itemIndex, (int)rangedItemsCount).Should().Be((int)(itemIndex + rangedItemsCount - 1));
				}
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void IndexOfShouldReturnCorrectIndexesWhenConstrainedIndex(string testCase, IEnumerable<long> testData, long itemsCount, int blockSize, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: blockSize, initialCapacity: itemsCount);
			var expectedData = testData.ToList();

			foreach (var itemIndex in itemIndexes)
			{
				_ = list.IndexOf(expectedData[(int)itemIndex], (int)itemIndex).Should().Be((int)itemIndex);
				_ = list.IndexOf(expectedData[^1], (int)itemIndex).Should().Be(list.Count - 1);
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void InitializeSortClearAddRangeRemoveShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				var testDataList = testData.ToList();
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
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void InsertShouldMoveItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{

			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				using var list = new RecyclableLongList<long>(testData, targetBlockSize, itemsCount);
				long expectedCapacity = CalculateExpectedCapacity(testCase, itemsCount, targetBlockSize);
				var expectedItems = testData.ToList();

				var existingItem = expectedItems[(int)itemIndex];
				var newItem = existingItem + 1;
				expectedItems.Insert((int)itemIndex, newItem);

				// Act
				list.Insert((int)itemIndex, newItem);

				// Validate
				_ = list[itemIndex].Should().Be(newItem);
				_ = list[itemIndex + 1].Should().Be(existingItem);
				_ = list.Capacity.Should().Be(expectedCapacity);
				_ = list.Count.Should().Be((int)itemsCount + 1);
				_ = list.Should().Equal(expectedItems);
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexWithRangeTheoryData))]
		public void InsertRangeShouldMoveItems(string testCase, IEnumerable<long> testData, int itemsCount, int minBlockSize, in (long ItemIndex, long RangedItemsCount)[] itemRanges)
		{
			// Prepare
			var expectedData = testData.Reverse().ToArray();

			foreach ((var itemIndex, var rangedItemsCount) in itemRanges)
			{
				using var list = new RecyclableLongList<long>(testData.Reverse(), minBlockSize, itemsCount);
				var expected = expectedData.ToList();
				var rangedTestData = expected.GetRange((int)itemIndex, (int)rangedItemsCount).ToArray();
				expected.InsertRange((int)itemIndex, rangedTestData);

				if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
				{
					list.InsertRange((int)itemIndex, (Array)rangedTestData);
				}
				else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
				{
					list.InsertRange((int)itemIndex, (ICollection)rangedTestData);
				}
				else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
				{
					list.InsertRange((int)itemIndex, (ICollection<long>)rangedTestData);
				}
				else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
				{
					list.InsertRange((int)itemIndex, (IEnumerable)rangedTestData);
				}
				else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
				{
					list.InsertRange((int)itemIndex, (IReadOnlyList<long>)rangedTestData);
				}
				else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
				{
					list.InsertRange((int)itemIndex, new ReadOnlySpan<long>(rangedTestData));
				}
				else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
				{
					list.InsertRange((int)itemIndex, new Span<long>(rangedTestData));
				}
				else if (testData is long[] testDataArray)
				{
					list.InsertRange((int)itemIndex, rangedTestData);
				}
				else if (testData is List<long> testDataList)
				{
					list.InsertRange((int)itemIndex, rangedTestData.ToList());
				}
				else if (testData is RecyclableList<long> testDataRecyclableList)
				{
					using RecyclableList<long> rangedTestDataRecyclableList = rangedTestData.ToRecyclableList();
					list.InsertRange((int)itemIndex, rangedTestDataRecyclableList);
				}
				else if (testData is RecyclableLongList<long> testDataRecyclableLongList)
				{
					using RecyclableLongList<long> rangedTestDataRecyclableList = rangedTestData.ToRecyclableLongList();
					list.InsertRange((int)itemIndex, rangedTestDataRecyclableList);
				}
				else if (testData is IList<long> testDataIList)
				{
					list.InsertRange((int)itemIndex, (IList<long>)rangedTestData);
				}
				else if (testData is IEnumerable<long> testDataIEnumerable)
				{
					list.InsertRangeEnumerated((int)itemIndex, (IEnumerable<long>)rangedTestData);
				}
				else
				{
					throw new InvalidCastException("Unknown type of test data");
				}

				// Validate
				list.Count.Should().Be(expected.Count);
				_ = list.Should().Equal(expected);
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void InsertShouldMoveItemsWhenLong(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{

			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				using var list = new RecyclableLongList<long>(testData, targetBlockSize, itemsCount);
				long expectedCapacity = CalculateExpectedCapacity(testCase, itemsCount, targetBlockSize);
				var expectedItems = testData.ToList();

				var existingItem = expectedItems[(int)itemIndex];
				var newItem = existingItem + 1;
				expectedItems.Insert((int)itemIndex, newItem);

				// Act
				list.Insert(itemIndex, newItem);

				// Validate
				_ = list[itemIndex].Should().Be(newItem);
				_ = list[itemIndex + 1].Should().Be(existingItem);
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
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void LongIndexOfShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
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
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
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
		[ClassData(typeof(SourceRefDataWithBlockSizeWithItemIndexTheoryData))]
		public void RemoveAtShouldClearItemWhenReferenceType(string testCase, IEnumerable<object> testData, long itemsCount, int targetBlockSize, in long[] indexesToRemove)
		{
			foreach (var indexToRemove in indexesToRemove)
			{
				// Arrange
				using var list = new RecyclableLongList<object>(testData, targetBlockSize, itemsCount);
				_ = list[^1].Should().NotBeNull();

				// Act
				list.RemoveAt(indexToRemove);

				// Assert
				_ = list[list.LongCount].Should().BeNull();
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void RemoveAtShouldRemoveTheCorrectItem(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
		{
			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize);
				var expectedItems = list.ToList();
				expectedItems.RemoveAt((int)itemIndex);

				// Act
				list.RemoveAt((int)itemIndex);

				// Validate
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
				_ = list.LastBlockWithData.Should().Be((int)(list.LongCount >> list.BlockSizePow2BitShift) - ((list.LongCount & list.BlockSizeMinus1) != 0 ? 0 : 1));
				_ = list.LongCount.Should().Be(itemsCount - 1);
				_ = list.Should().Equal(expectedItems);
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
		public void RemoveAtShouldRemoveTheCorrectItemWhenLong(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize, in long[] itemIndexes)
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
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
				_ = list.LastBlockWithData.Should().Be((int)(list.LongCount >> list.BlockSizePow2BitShift) - ((list.LongCount & list.BlockSizeMinus1) != 0 ? 0 : 1));
				_ = list.LongCount.Should().Be(itemsCount - 1);
				_ = list.Should().Equal(expectedItems);
			}
		}

		[Theory]
		[ClassData(typeof(SourceRefDataWithBlockSizeWithItemIndexTheoryData))]
		public void RemoveShouldClearItemWhenReferenceType(string testCase, IEnumerable<object> testData, long itemsCount, int targetBlockSize, in long[] indexesToRemove)
		{
			var expectedItems = testData.ToArray();

			foreach (var indexToRemove in indexesToRemove)
			{
				// Arrange
				var itemToRemove = expectedItems[(int)indexToRemove];
				using var list = new RecyclableLongList<object>(testData, targetBlockSize, itemsCount);
				_ = list[^1].Should().NotBeNull();

				// Act
				list.Remove(itemToRemove);

				// Assert
				_ = list[list.LongCount].Should().BeNull();
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexTheoryData))]
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
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
				_ = list.LastBlockWithData.Should().Be((int)(list.LongCount >> list.BlockSizePow2BitShift) - ((list.LongCount & list.BlockSizeMinus1) != 0 ? 0 : 1));
				_ = list.LongCount.Should().Be(itemsCount - 1);
				_ = list.Should().Equal(expectedItems);
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void SetByIndexShouldReturnCorrectItem(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			var expectedData = testData.ToList();

			// Act & Validate
			foreach (long item in testData)
			{
				// Act
				list[(int)item - 1] = item + 1;
				
				// Validate
				expectedData[(int)item - 1] = item + 1;
				list[item - 1].Should().Be(item + 1);
				list.Should().Equal(expectedData);
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void SetByIndexShouldReturnCorrectItemWhenLong(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);
			var expectedData = testData.ToList();

			// Act & Validate
			foreach (long item in testData)
			{
				// Act
				list[item - 1] = item + 1;

				// Validate
				expectedData[(int)item - 1] = item + 1;
				list[item - 1].Should().Be(item + 1);
				list.Should().Equal(expectedData);
			}
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void VersionedEnumerateShouldRaiseExceptionWhenCollectionIsModified(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, targetBlockSize, itemsCount)
			{
				-1
			};

			// Act
			_ = Assert.Throws<InvalidOperationException>(() =>
			{
				foreach (var item in (IRecyclableVersionedLongList<long>)list)
				{
					list.Add(item);
				}
			});
		}

		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeTheoryData))]
		public void VersionedEnumerateShouldYieldAllItemsInCorrectOrder(string testCase, IEnumerable<long> testData, int itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, minBlockSize: targetBlockSize, itemsCount);

			// Act
			var yieldedItems = new List<long>(itemsCount);
			foreach (var item in (IRecyclableVersionedLongList<long>)list)
			{
				yieldedItems.Add(item);
			}

			// Validate
			_ = yieldedItems.Should().Equal(testData);
		}
	}
}
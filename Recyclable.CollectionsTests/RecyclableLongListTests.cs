using FluentAssertions;
using Recyclable.Collections;
using Recyclable.Collections.TestData;
using Recyclable.Collections.Linq;
using System.Collections;
using System.Numerics;

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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.EmptySourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldNotOverrideItems(string testCase, IEnumerable<long> testData, long itemsCount, int targetBlockSize)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(minBlockSize: targetBlockSize);

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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount * 2);
			_ = list.LongCount.Should().Be(itemsCount * 2);
			_ = list.LastBlockWithData.Should().Be((int)(itemsCount * 2 / list.BlockSize) - (itemsCount * 2 % list.BlockSize != 0 ? 0 : 1));
			_ = list.Should().Equal(testData.Concat(testData));
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceRefDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
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
			long[] copiedItems = new long[itemsCount];

			// Act
			list.CopyTo(copiedItems, 0);
			
			// Validate
			_ = copiedItems.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceRefDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceRefDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceRefDataWithBlockSizeWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
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
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
				_ = list.LastBlockWithData.Should().Be((int)(list.LongCount >> list.BlockSizePow2BitShift) - ((list.LongCount & list.BlockSizeMinus1) != 0 ? 0 : 1));
				_ = list.LongCount.Should().Be(itemsCount - 1);
				_ = list.Should().Equal(expectedItems);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithBlockSizeVariants), MemberType = typeof(RecyclableLongListTestData))]
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
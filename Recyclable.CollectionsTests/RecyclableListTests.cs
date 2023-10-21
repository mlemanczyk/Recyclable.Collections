using FluentAssertions;
using Recyclable.Collections;
using Recyclable.Collections.TestData;
using Recyclable.Collections.Linq;
using System.Collections;

#pragma warning disable xUnit1026, RCS1163, IDE0060, RCS1235

namespace Recyclable.CollectionsTests
{
	public class RecyclableListTests
	{
		[Fact]
		public void AddRangeShouldAcceptNulls()
		{
			// Prepare
			using var list = new RecyclableList<long?>();
			long?[] testData = new long?[] { null, default };

			// Act
			list.AddRange(testData);

			// Validate
			List<long?> expectedData = new();
			expectedData.AddRange(testData);

			_ = list.Count.Should().Be(testData.Length);
			_ = list.Should().HaveCount(testData.Length).And.AllSatisfy(x => x.Should().BeNull()).And.Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldAddItemsInCorrectOrder(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();

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
				_ = list.AddRange((IEnumerable)testData);
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
			List<long> expectedData = new(itemsCount);
			expectedData.AddRange(testData);

			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.EmptySourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldDoNothingWhenSourceIsEmpty(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();

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
				_ = list.AddRange((IEnumerable)testData);
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
			List<long> expectedData = new(itemsCount);
			expectedData.AddRange(testData);

			_ = list.Capacity.Should().Be(RecyclableDefaults.InitialCapacity);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldNotOverrideItems(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();
			itemsCount = checked(itemsCount << 1);

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
				_ = list.AddRange((IEnumerable)testData);
				_ = list.AddRange((IEnumerable)testData);
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
			List<long> expectedData = new(itemsCount);
			expectedData.AddRange(testData);
			expectedData.AddRange(testData);

			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();
			testData = testData.Concat(testData.Reverse());
			itemsCount = checked(itemsCount << 1);

			// Act
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			List<long> expectedData = new(itemsCount);
			expectedData.AddRange(testData);

			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(expectedData);
		}

		[Fact]
		public void AddShouldAcceptNulls()
		{
			// Prepare
			long?[] testData = new long?[] { null, default };
			using var list = new RecyclableList<long?>();

			// Act
			foreach (var item in testData)
			{
				list.Add(item);
			}

			// Validate
			List<long?> expectedData = new(testData);

			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(RecyclableDefaults.InitialCapacity);
			_ = list.Should().AllSatisfy(x => x.Should().BeNull()).And.Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddShouldAddItems(string testCase, IEnumerable<long> testData, int itemsCount)
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
			var expectedData = testData.ToList();

			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddShouldAddItemWhenAfterAddRange(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();
			itemsCount = checked(itemsCount << 1);

			// Act
			list.AddRange(testData);
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			List<long> expectedData = new(itemsCount);
			expectedData.AddRange(testData);
			expectedData.AddRange(testData);

			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceRefDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ClearShouldClearItemsWhenReferenceType(string testCase, IEnumerable<object> testData, int itemsCount)
		{
			//* Clearing empty list is tested in <see cref="ClearShouldSucceed"/>.
			if (itemsCount == 0)
			{
				return;
			}

			// Prepare
			using var list = new RecyclableList<object>(testData, itemsCount);
			_ = list.AsArray[..itemsCount].Should().AllSatisfy(x => x.Should().NotBeNull());

			// Act
			list.Clear();

			// Validate
			_ = list.AsArray[..itemsCount].Should().AllSatisfy(x => x.Should().BeNull());
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ClearShouldRemoveAllItems(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			_ = list.Count.Should().Be(itemsCount);

			// Act		
			list.Clear();
			list.Clear();

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(0);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConsecutiveDisposeShouldSucceed(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			list.Dispose();

			// Act
			list.Dispose();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			testData = testData.Concat(testData.Reverse());
			itemsCount = checked(itemsCount << 1);

			// Act
			using var list = new RecyclableList<long>(testData, itemsCount);

			// Validate
			List<long> expectedData = new(testData);

			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorSourceShouldInitializeList(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Act
			using var list = new RecyclableList<long>(testData, itemsCount);

			// Validate
			List<long> expectedData = new(testData);

			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldFindAllItems(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			var expectedData = testData.ToList();
			
			foreach (var itemIndex in itemIndexes)
			{
				long expectedItem = expectedData[(int)itemIndex];
				bool expected = expectedData.Contains(expectedItem);

				// Act
				var actual = list.Contains(expectedItem);
				
				// Validate
				_ = actual.Should().Be(expected);
			}
		}

		[Fact]
		public void ContainsShouldNotFindAnythingWhenListEmpty()
		{
			// Prepare
			using var list = new RecyclableList<long>();

			// Validate
			_ = list.Contains(0).Should().BeFalse();
			_ = list.Contains(1).Should().BeFalse();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			var expectedData = testData.ToList();

			foreach (var itemIndex in itemIndexes)
			{
				long expectedItem = -expectedData[(int)itemIndex];
				var expected = expectedData.Contains(expectedItem);

				// Act
				var actual = list.Contains(expectedItem);
				
				// Validate
				_ = actual.Should().Be(expected);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void CopyToShouldCopyAllItemsInTheCorrectOrder(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			long[] copiedItems = new long[itemsCount];

			// Act
			list.CopyTo(copiedItems, 0);

			// Validate
			long[] expectedData = new long[itemsCount];
			testData.ToList().CopyTo(expectedData, 0);

			_ = copiedItems.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceRefDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void DisposeShouldClearItemsWhenReferenceType(string testCase, IEnumerable<object> testData, int itemsCount)
		{
			//* Disposing empty list is tested in <see cref="DisposeShouldSucceed"/>.
			if (itemsCount == 0)
			{
				return;
			}

			// Prepare
			using var list = new RecyclableList<object>(testData, itemsCount);
			var listArray = list.AsArray;
			_ = list.AsArray[..itemsCount].Should().NotBeNull();
			_ = listArray[..itemsCount].Should().AllSatisfy(x => x.Should().NotBeNull());

			// Act
			list.Dispose();

			// Validate
			_ = listArray[..itemsCount].Should().AllSatisfy(x => x.Should().BeNull());
		}

		[Fact]
		public void DisposeShouldReturnPooledArrayOnlyOnce()
		{
			// Prepare
			var testData = RecyclableLongListTestData.CreateTestData(RecyclableDefaults.MinPooledArrayLength);
			using RecyclableList<long> firstInstance = new(testData);
			var firstArray = firstInstance.AsArray;

			// Act
			firstInstance.Dispose();

			// Validate
			using RecyclableList<long> sameInstance = new(testData);
			_ = sameInstance.AsArray.Should().BeSameAs(firstArray);

			using RecyclableList<long> anotherInstance = new(testData);
			_ = anotherInstance.AsArray.Should().NotBeSameAs(firstArray);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void DisposeShouldSucceed(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);

			// Act
			list.Dispose();

			// Validate
			_ = list.Capacity.Should().Be(0);
			_ = list.Count.Should().Be(0);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void EnumerateShouldYieldAllItemsInCorrectOrder(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			using var enumerator = list.GetEnumerator();
			var yieldedItems = new List<long>(itemsCount);

			// Act
			while (enumerator.MoveNext())
			{
				yieldedItems.Add(enumerator.Current);
			}

			// Validate
			List<long> expectedData = new(testData);

			_ = yieldedItems.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void GetByIndexShouldReturnCorrectItem(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			var expectedData = testData.ToList();

			foreach (var item in testData)
			{
				var expected = expectedData[(int)item - 1];

				// Act
				var actual = list[(int)item - 1];
				
				// Validate
				_ = actual.Should().Be(item).And.Be(expected);
			}
		}

		[Fact]
		public void IndexOfShouldNotFindAnythingWhenListEmpty()
		{
			// Prepare
			using var list = new RecyclableList<long>();

			// Act & Validate
			_ = list.IndexOf(-1).Should().Be(-1);
			_ = list.IndexOf(0).Should().Be(-1);
			_ = list.IndexOf(1).Should().Be(-1);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void IndexOfShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");
		
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			var expectedData = testData.ToList();

			foreach (var itemIndex in itemIndexes)
			{
				var expected = expectedData.IndexOf(-expectedData[(int)itemIndex]);

				// Act
				var actual = list.IndexOf(-expectedData[(int)itemIndex]);
				
				// Validate
				_ = actual.Should().Be(-1).And.Be(expected);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void IndexOfShouldReturnCorrectIndexes(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			var expectedData = testData.ToList();

			foreach (var itemIndex in itemIndexes)
			{
				var expected = expectedData.IndexOf(expectedData[(int)itemIndex]);

				// Act
				var actual = list.IndexOf(expectedData[(int)itemIndex]);

				// Validate
				_ = actual.Should().Be((int)itemIndex).And.Be(expected);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void InitializeSortClearAddRangeRemoveShouldSucceed(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				var expectedData = testData.ToList();
				var item = expectedData[(int)itemIndex];

				// Act
				using RecyclableList<long> list = new(testData);
				QuickSortExtensions<long>.QuickSort(list);

				// Validate
				_ = list.Count.Should().Be(itemsCount);
				_ = list.Should().Equal(expectedData);

				// Act
				list.Clear();

				// Validate
				_ = list.Count.Should().Be(0);
				_ = list.Should().BeEmpty();

				// Act
				list.AddRange(testData);

				// Validate
				_ = list.Count.Should().Be(itemsCount);
				_ = list.Should().Equal(expectedData);

				// Prepare
				expectedData.RemoveAt((int)itemIndex);

				// Act
				_ = list.Remove(item).Should().BeTrue();
				_ = list.IndexOf(item).Should().Be(RecyclableDefaults.ItemNotFoundIndex);
				_ = list.Contains(item).Should().BeFalse();
				_ = list.Count.Should().Be(itemsCount - 1);
				_ = list.Should().Equal(expectedData);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void InsertShouldMoveItems(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				using var list = new RecyclableList<long>(testData, itemsCount);
				var expectedItems = list.ToList();
				var item = expectedItems[(int)itemIndex];
				expectedItems.Insert((int)itemIndex, item);

				// Act
				list.Insert((int)itemIndex, item);

				// Validate
				_ = list[(int)itemIndex].Should().Be(item);
				_ = list[(int)itemIndex + 1].Should().Be(item);
				_ = list.Count.Should().Be(itemsCount + 1);
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
				_ = list.Should().Equal(expectedItems);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceRefDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveAtShouldClearItemWhenReferenceType(string testCase, IEnumerable<object> testData, int itemsCount, in long[] indexesToRemove)
		{
			// Prepare
			foreach (long indexToRemove in indexesToRemove)
			{
				using var list = new RecyclableList<object>(testData, itemsCount);
				_ = list[^1].Should().NotBeNull();

				// Act
				list.RemoveAt((int)indexToRemove);

				// Validate
				_ = list[list.Count].Should().BeNull();
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveAtShouldRemoveTheCorrectItem(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				using var list = new RecyclableList<long>(testData, itemsCount);
				var expectedItems = testData.ToList();
				expectedItems.RemoveAt((int)itemIndex);

				// Act
				list.RemoveAt((int)itemIndex);

				// Validate
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
				_ = list.Count.Should().Be(itemsCount - 1);
				_ = list.Should().Equal(expectedItems);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceRefDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveShouldClearItemWhenReferenceType(string testCase, IEnumerable<object> testData, int itemsCount, in long[] indexesToRemove)
		{

			foreach (long indexToRemove in indexesToRemove)
			{
				// Prepare
				using var list = new RecyclableList<object>(testData, itemsCount);
				var expectedItems = testData.ToArray();
				var item = expectedItems[indexToRemove];
				_ = list[^1].Should().NotBeNull();

				// Act
				list.Remove(item);

				// Validate
				_ = list[list.Count].Should().BeNull();
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void RemoveShouldRemoveTheCorrectItem(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				using var list = new RecyclableList<long>(testData, itemsCount);
				var expectedItems = testData.ToList();
				var item = expectedItems[(int)itemIndex];
				var expected = expectedItems.Remove(item);

				// Act
				var actual = list.Remove(item);

				// Validate
				_ = actual.Should().BeTrue().And.Be(expected);
				_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
				_ = list.Count.Should().Be(itemsCount - 1);
				_ = list.Should().Equal(expectedItems);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void SetByIndexShouldReturnCorrectItem(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			var expectedData = testData.ToList();

			// Act & Validate
			foreach (long item in testData)
			{
				// Act
				list[(int)item - 1] = item + 1;
				
				// Validate
				expectedData[(int)item - 1] = item + 1;
				_ = list[(int)item - 1].Should().Be(item + 1);
				_ = list.Should().Equal(expectedData);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ToArrayShouldReturnAllItemsInCorrectOrder(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			var expectedData = testData.ToList().ToArray();

			// Act
			var returnedItems = list.ToArray();

			// Validate
			_ = returnedItems.Length.Should().Be(itemsCount);
			_ = returnedItems.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void VersionedEnumerateShouldRaiseExceptionWhenCollectionIsModified(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);

			// Act
			_ = Assert.Throws<InvalidOperationException>(() =>
			{
				list.Add(-1);
				foreach (var item in (IRecyclableVersionedList<long>)list)
				{
					list.Add(item);
				}
			});
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void VersionedEnumerateShouldYieldAllItemsInCorrectOrder(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);

			// Act
			var yieldedItems = new List<long>(itemsCount);
			foreach (var item in (IRecyclableVersionedList<long>)list)
			{
				yieldedItems.Add(item);
			}

			// Validate
			_ = yieldedItems.Should().Equal(testData);
		}
	}
}
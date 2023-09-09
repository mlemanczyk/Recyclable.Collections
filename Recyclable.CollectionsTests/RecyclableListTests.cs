using FluentAssertions;
using Recyclable.Collections;
using Recyclable.Collections.Linq;

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

			// Act
			list.AddRange(new long?[] { null, default });

			// Validate
			_ = list.Should().HaveCount(2).And.AllSatisfy(x => x.Should().BeNull());
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldAddItemsInCorrectOrder(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();

			// Act
			if (testCase.Contains("ReadOnlySpan[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange(new ReadOnlySpan<long>((long[])testData));
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
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.EmptySourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldDoNothingWhenSourceIsEmpty(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();

			// Act
			if (testCase.Contains("ReadOnlySpan[", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange(new ReadOnlySpan<long>((long[])testData));
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
			_ = list.Capacity.Should().Be(RecyclableDefaults.InitialCapacity);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().BeEmpty();
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddRangeShouldNotOverrideItems(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>();

			// Act
			if (testCase.Contains("ReadOnlySpan[", StringComparison.OrdinalIgnoreCase))
			{
				var readOnlySpan = new ReadOnlySpan<long>((long[])testData);
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
			_ = list.Count.Should().Be(itemsCount * 2);
			_ = list.Should().Equal(expectedData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			testData = testData.Concat(testData);

			// Act
			using var list = new RecyclableList<long>();
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount * 2);
			_ = list.Count.Should().Be(itemsCount * 2);
			_ = list.Should().Equal(testData);
		}

		[Fact]
		public void AddShouldAcceptNulls()
		{
			// Prepare
#pragma warning disable IDE0028
			using var list = new RecyclableList<long?>();

			// Act
			list.Add(null);
			list.Add(default);
#pragma warning restore IDE0028

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(RecyclableDefaults.InitialCapacity);
			_ = list.Should().AllSatisfy(x => x.Should().BeNull());
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
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void AddShouldAddItemWhenAfterAddRange(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Act
			using var list = new RecyclableList<long>();
			list.AddRange(testData);
			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount * 2);
			_ = list.Count.Should().Be(itemsCount * 2);
			_ = list.Should().Equal(testData.Concat(testData));
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceRefDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ClearShouldClearItemsWhenReferenceType(string testCase, IEnumerable<object> testData, int itemsCount)
		{
			if (itemsCount == 0)
			{
				/// Clearing empty list is tested in <see cref="ClearShouldSucceed"/>.
				return;
			}

			// Arrange
			using var list = new RecyclableList<object>(testData, itemsCount);
			_ = list.AsArray[..itemsCount].Should().AllSatisfy(x => x.Should().NotBeNull());

			// Act
			list.Clear();

			// Assert
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
			testData = testData.Concat(testData);

			// Act
			using var list = new RecyclableList<long>(testData, itemsCount * 2);

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount * 2);
			_ = list.Count.Should().Be(itemsCount * 2);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ConstructorSourceShouldInitializeList(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Act
			using var list = new RecyclableList<long>(testData, itemsCount);

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo(itemsCount);
			_ = list.Count.Should().Be(itemsCount);
			_ = list.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldFindAllItems(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.Contains(list[(int)itemIndex]).Should().BeTrue($"we searched for {list[(int)itemIndex]}");
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void ContainsShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.Contains(-list[(int)itemIndex]).Should().BeFalse();
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
			_ = copiedItems.Should().Equal(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceRefDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void DisposeShouldClearItemsWhenReferenceType(string testCase, IEnumerable<object> testData, int itemsCount)
		{
			if (itemsCount == 0)
			{
				/// Disposing empty list is tested in <see cref="DisposeShouldSucceed"/>.
				return;
			}

			// Arrange
			using var list = new RecyclableList<object>(testData, itemsCount);
			var listArray = list.AsArray;
			_ = list.AsArray[..itemsCount].Should().NotBeNull();
			_ = listArray[..itemsCount].Should().AllSatisfy(x => x.Should().NotBeNull());

			// Act
			list.Dispose();

			// Assert
			_ = listArray[..itemsCount].Should().AllSatisfy(x => x.Should().BeNull());
		}

		[Fact]
		public void DisposeShouldReturnPooledArrayOnlyOnce()
		{
			// Arrange
			var testData = RecyclableLongListTestData.CreateTestData(RecyclableDefaults.MinPooledArrayLength);
			RecyclableList<long> firstInstance = new(testData);
			var firstArray = firstInstance.AsArray;

			// Act
			firstInstance.Dispose();

			// Assert
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
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void GetByIndexShouldReturnCorrectItem(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableLongList<long>(testData, itemsCount);

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
			using var list = new RecyclableList<int>();

			// Validate
			_ = list.IndexOf(0).Should().Be(-1);
			_ = list.IndexOf(1).Should().Be(-1);
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void IndexOfShouldNotFindNonExistingItems(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.IndexOf(-list[(int)itemIndex]).Should().Be(-1);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void IndexOfShouldReturnCorrectIndexes(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);

			// Act & Validate
			foreach (var itemIndex in itemIndexes)
			{
				_ = list.IndexOf(list[(int)itemIndex]).Should().Be((int)itemIndex);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataWithItemIndexVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void InitializeSortClearAddRangeRemoveShouldSucceed(string testCase, IEnumerable<long> testData, int itemsCount, in long[] itemIndexes)
		{
			foreach (var itemIndex in itemIndexes)
			{
				// Prepare
				var testDataList = testData.ToList();
				var item = testDataList[(int)itemIndex];

				// Act
				using RecyclableList<long> list = new(testData);
				QuickSortExtensions<long>.QuickSort(list);

				// Validate
				_ = list.Count.Should().Be(itemsCount);
				_ = list.Should().Equal(testDataList);

				// Act
				list.Clear();

				// Validate
				_ = list.Count.Should().Be(0);
				_ = list.Should().BeEmpty();

				// Act
				list.AddRange(testData);

				// Validate
				_ = list.Count.Should().Be(itemsCount);
				_ = list.Should().Equal(testDataList);

				// Prepare
				testDataList.RemoveAt((int)itemIndex);

				// Act
				_ = list.Remove(item).Should().BeTrue();
				_ = list.IndexOf(item).Should().Be(RecyclableDefaults.ItemNotFoundIndex);
				_ = list.Contains(item).Should().BeFalse();
				_ = list.Count.Should().Be(itemsCount - 1);
				_ = list.Should().Equal(testDataList);
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
				var item = list[(int)itemIndex];
				var expectedItems = list.ToList();
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
			// Arrange
			foreach (int indexToRemove in indexesToRemove)
			{
				using var list = new RecyclableList<object>(testData, itemsCount);
				_ = list[^1].Should().NotBeNull();

				// Act
				list.RemoveAt(indexToRemove);

				// Assert
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
			var expectedItems = testData.ToArray();

			foreach (int indexToRemove in indexesToRemove)
			{
				// Arrange
				var itemToRemove = expectedItems[indexToRemove];
				using var list = new RecyclableList<object>(testData, itemsCount);
				_ = list[^1].Should().NotBeNull();

				// Act
				list.Remove(itemToRemove);

				// Assert
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
				var item = list[(int)itemIndex];
				expectedItems.RemoveAt((int)itemIndex);

				// Act
				_ = list.Remove(item).Should().BeTrue();

				// Validate
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
				list[(int)item - 1].Should().Be(item + 1);
				list.Should().Equal(expectedData);
			}
		}

		[Theory]
		[MemberData(nameof(RecyclableLongListTestData.SourceDataVariants), MemberType = typeof(RecyclableLongListTestData))]
		public void VersionedEnumerateShouldRaiseExceptionWhenCollectionIsModified(string testCase, IEnumerable<long> testData, int itemsCount)
		{
			// Prepare
			using var list = new RecyclableList<long>(testData, itemsCount);

			// Act
			Assert.Throws<InvalidOperationException>(() =>
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
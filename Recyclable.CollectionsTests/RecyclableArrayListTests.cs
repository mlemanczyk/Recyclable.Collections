﻿using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableArrayListTests
	{
		private readonly static IEnumerable<int> _testData = Enumerable.Range(1, 20);

		[Theory]
		[MemberData(nameof(RecyclableListTestData.SourceDataVariants), MemberType = typeof(RecyclableListTestData))]
		public void AddCountShouldSucceed(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			var list = new RecyclableArrayList<long>();

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
		[MemberData(nameof(RecyclableListTestData.SourceDataVariants), MemberType = typeof(RecyclableListTestData))]
		public void AddRangeShouldAddItemsInCorrectOrder(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			using var list = new RecyclableArrayList<long>();

			// Act
			if (testData is long[] testDataArray)
			{
				list.AddRange(testDataArray);
			}
			else if (testData is List<long> testDataList)
			{
				list.AddRange(testDataList);
			}
			else if (testData is RecyclableArrayList<long> testDataRecyclableArrayList)
			{
				list.AddRange(testDataRecyclableArrayList);
			}
			else if (testData is RecyclableList<long> testDataRecyclableList)
			{
				list.AddRange(testDataRecyclableList);
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
		[MemberData(nameof(RecyclableListTestData.SourceDataVariants), MemberType = typeof(RecyclableListTestData))]
		public void AddShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Act
			using var list = new RecyclableArrayList<long>((int)itemsCount * 2);
			foreach (var index in testData)
			{
				list.Add(index);
			}

			foreach (var index in testData)
			{
				list.Add(index);
			}

			// Validate
			_ = list.Count.Should().Be((int)itemsCount * 2);
			_ = list.Should().ContainInConsecutiveOrder(testData.Concat(testData));
		}

		[Theory]
		[MemberData(nameof(RecyclableListTestData.SourceDataVariants), MemberType = typeof(RecyclableListTestData))]
		public void ClearShouldRemoveAllItems(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Prepare
			var list = new RecyclableArrayList<long>(testData);
			_ = list.Count.Should().Be((int)itemsCount);

			// Act		
			list.Clear();

			// Validate			
			_ = list.Count.Should().Be(0);
			_ = list.Should().BeEmpty();

		}

		[Theory]
		[MemberData(nameof(RecyclableListTestData.SourceDataVariants), MemberType = typeof(RecyclableListTestData))]
		public void ConstructorShouldAcceptDuplicates(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Act
			using var list = new RecyclableArrayList<long>(testData.Concat(testData), initialCapacity: (int)itemsCount * 2);

			// Validate
			_ = list.Count.Should().Be((int)itemsCount * 2);
			_ = list.Should().ContainInConsecutiveOrder(testData.Concat(testData));
		}

		[Theory]
		[MemberData(nameof(RecyclableListTestData.SourceDataVariants), MemberType = typeof(RecyclableListTestData))]
		public void ConstructorShouldAddItemsInCorrectOrderWhenSourceIsIEnumerable(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Act
			var list = new RecyclableArrayList<long>(testData);

			// Validate
			_ = list.Capacity.Should().BeGreaterThanOrEqualTo((int)itemsCount, "when we succeed in taking count without enumerating, then we allocate as much memory as needed, only");
			_ = list.Count.Should().Be((int)itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableListTestData.SourceDataVariants), MemberType = typeof(RecyclableListTestData))]
		public void ConstructorSourceShouldInitializeList(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			// Act
			var list = new RecyclableArrayList<long>(testData);

			// Validate
			_ = list.Count.Should().Be((int)itemsCount);
			_ = list.Should().ContainInConsecutiveOrder(testData);
		}

		[Theory]
		[MemberData(nameof(RecyclableListTestData.SourceDataVariants), MemberType = typeof(RecyclableListTestData))]
		public void ContainsShouldFindAllItems(string testCase, IEnumerable<long> testData, long itemsCount)
		{
			if (itemsCount == 0)
			{
				return;
			}

			// Prepare
			using var list = new RecyclableArrayList<long>(testData);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act
			foreach (var item in testData)
			{
				// Validate
				_ = list.Contains(item).Should().BeTrue();
			}
		}

		[Fact]
		public void InsertAtTheBeginningShouldMoveItems()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableArrayList<int>();

			// Act
			foreach (var item in testData)
			{
				list.Insert(0, item);
				_ = list.Count.Should().Be(item);
			}

			// Validate
			_ = list.Capacity.Should().Be(32);
			_ = list.Should().HaveCount(testData.Length)
				.And.ContainInConsecutiveOrder(testData.Reverse())
				.And.BeEquivalentTo(testData);
		}

		[Fact]
		public void LongIndexOfShouldReturnCorrectIndexes()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableArrayList<int>(testData);

			// Act & Validate
			foreach (var item in testData)
			{
				// Act
				var index = list.IndexOf(item);

				// Validate
				_ = index.Should().Be(item - 1);
			}
		}

		[Fact]
		public void RemoveFromTheBeginningShouldRemoveTheCorrectItem()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableArrayList<int>(testData);

			// Act & Validate
			for (var deleted = 1; deleted <= testData.LongLength; deleted++)
			{
				// Act
				_ = list.Remove(deleted).Should().BeTrue();

				// Validate
				_ = list.Should().HaveCount(testData.Length - deleted)
					.And.ContainInConsecutiveOrder(testData.Skip(deleted))
					.And.BeEquivalentTo(testData.Skip(deleted));
			}
		}

		[Fact]
		public void RemoveFromTheEndShouldRemoveTheCorrectItem()
		{
			// Prepare
			var testData = _testData.ToArray();

			// Act & Validate
			var list = new RecyclableArrayList<int>(testData);
			for (var deleted = 1; deleted <= testData.LongLength; deleted++)
			{
				// Act
				_ = list.Remove(testData[^deleted]).Should().BeTrue();

				// Validate
				_ = list.Should().HaveCount(testData.Length - deleted)
					.And.ContainInConsecutiveOrder(testData.Take(testData.Length - deleted))
					.And.BeEquivalentTo(testData.Take(testData.Length - deleted));
			}
		}

		[Fact]
		public void RemoveAtFromTheBeginningShouldSucceed()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableArrayList<int>(testData);

			// Act & Validate
			for (var deleted = 1; deleted <= testData.LongLength; deleted++)
			{
				// Act
				list.RemoveAt(0);

				// Validate
				_ = list.Should().HaveCount(testData.Length - deleted)
					.And.ContainInConsecutiveOrder(testData.Skip(deleted))
					.And.BeEquivalentTo(testData.Skip(deleted));
			}

			_ = list.Should().BeEmpty();
			_ = list.Capacity.Should().Be(20);
		}

		[Fact]
		public void RemoveAtFromTheEndShouldSucceed()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableArrayList<int>(testData);

			// Act & Validate
			for (var deleted = 1; deleted <= testData.LongLength; deleted++)
			{
				// Act
				list.RemoveAt(list.Count - 1);

				// Validate
				_ = list.Should().HaveCount(testData.Length - deleted)
					.And.ContainInConsecutiveOrder(testData.Take(testData.Length - deleted))
					.And.BeEquivalentTo(testData.Take(testData.Length - deleted));
			}

			_ = list.Should().BeEmpty();
			_ = list.Capacity.Should().Be(20);
		}

		[Fact]
		public void EnumerateShouldYieldAllItemsInCorrectOrder()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableArrayList<int>(testData);

			// Act
			var yieldedItems = new List<int>();
			foreach (var item in list)
			{
				yieldedItems.Add(item);
			}

			// Validate
			_ = yieldedItems.Should().HaveCount(testData.Length)
				.And.ContainInConsecutiveOrder(testData)
				.And.BeEquivalentTo(testData);
		}

		[Fact]
		public void CopyToShouldCopyAllItemsInTheCorrectOrder()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableArrayList<int>(testData);
			int[] yieldedItems = new int[testData.Length];

			// Act
			list.CopyTo(yieldedItems, 0);

			// Validate
			_ = yieldedItems.Should().HaveCount(testData.Length)
				.And.ContainInConsecutiveOrder(testData)
				.And.BeEquivalentTo(testData);
		}

		[Fact]
		public void IndexOfShouldFindTheIndexes()
		{
			// Prepare
			var testData = _testData.ToArray();
			using var list = new RecyclableArrayList<int>(testData);

			// Act & Validate
			foreach (var index in _testData.ToArray())
			{
				var actual = list.IndexOf(index);
				_ = actual.Should().Be(index - 1);
			}
		}

		[Fact]
		public void ContainsShouldNotFindNonExistingItems()
		{
			// Prepare
			var testData = _testData.ToArray();
			using var list = new RecyclableArrayList<int>(testData);
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
			var testData = _testData.ToArray();
			using var list = new RecyclableArrayList<int>(testData);

			// Validate
			_ = list.Contains(0).Should().BeFalse();
			_ = list.Contains(1).Should().BeTrue();
		}

	}
} 
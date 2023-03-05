﻿using FluentAssertions;
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
		public void IndexOfShouldFindTheIndexes()
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

		[Fact]
		public void AddRangeShouldAddItemsInCorrectOrderWhenSourceIsArray()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableList<int>();


			// Act
			list.AddRange(testData);

			// Validate
			_ = list.Capacity.Should().Be(114_688, "when capacity == 0, then we allocate as much memory as needed, only");
			_ = list.Should().HaveCount(testData.Length)
			.And.ContainInConsecutiveOrder(testData)
			.And.BeEquivalentTo(testData);
		}

		[Fact]
		public void AddRangeShouldAddItemsInCorrectOrderWhenSourceIsList()
		{
			// Prepare
			var testData = _testData.ToList();
			var list = new RecyclableList<int>();

			// Act
			list.AddRange(testData);

			// Validate
			_ = list.Capacity.Should().Be(114_688, "when capacity == 0, then we allocate as much memory as needed, only");
			_ = list.Should().HaveCount(testData.Count)
			.And.ContainInConsecutiveOrder(testData)
			.And.BeEquivalentTo(testData);
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
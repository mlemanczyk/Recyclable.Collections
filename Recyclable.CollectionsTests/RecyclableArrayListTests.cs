using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableArrayListTests
	{
		private readonly static IEnumerable<int> _testData = Enumerable.Range(1, 20);

		[Fact]
		public void AddCountShouldSucceed()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableArrayList<int>();

			// Act
			foreach (var item in testData)
			{
				list.Add(item);
				_ = list.Count.Should().Be(item);
			}

			// Validate
			_ = list.Capacity.Should().Be(32);
			_ = list.Should().HaveCount(testData.Length)
				.And.ContainInConsecutiveOrder(testData)
				.And.BeEquivalentTo(testData);
		}

		[Fact]
		public void AddRangeShouldAddItemsInCorrectOrderWhenSourceIsArray()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableArrayList<int>();

			// Act
			list.AddRange(testData);

			// Validate
			_ = list.Capacity.Should().Be(20, "when capacity == 0, then we allocate as much memory as needed, only");
			_ = list.Should().HaveCount(testData.Length)
				.And.ContainInConsecutiveOrder(testData)
				.And.BeEquivalentTo(testData);
		}

		[Fact]
		public void AddRangeShouldAddItemsInCorrectOrderWhenSourceIsList()
		{
			// Prepare
			var testData = _testData.ToList();
			var list = new RecyclableArrayList<int>();

			// Act
			list.AddRange(testData);

			// Validate
			_ = list.Capacity.Should().Be(20, "when capacity == 0, then we allocate as much memory as needed, only");
			_ = list.Should().HaveCount(testData.Count)
				.And.ContainInConsecutiveOrder(testData)
				.And.BeEquivalentTo(testData);
		}

		[Fact]
		public void ConstructorShouldAddItemsInCorrectOrderWhenSourceIsIEnumerable()
		{
			// Prepare
			var testData = _testData;

			// Act
			var list = new RecyclableArrayList<int>(testData);

			// Validate
			_ = list.Capacity.Should().Be(32, "when we succeed in taking count without enumerating, then we allocate as much memory as needed, only");
			_ = list.Should().HaveCount(testData.Count())
				.And.ContainInConsecutiveOrder(testData)
				.And.BeEquivalentTo(testData);
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
		public void ConstructorSourceShouldInitializeList()
		{
			// Prepare
			var testData = _testData.ToArray();

			// Act
			var list = new RecyclableArrayList<int>(testData);

			// Validate
			_ = list.Should().HaveCount(testData.Length)
				.And.ContainInConsecutiveOrder(testData)
				.And.BeEquivalentTo(testData);
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
		public void ClearShouldRemoveAllItems()
		{
			// Prepare
			var testData = _testData.ToArray();
			var list = new RecyclableArrayList<int>(testData);
			_ = list.Count.Should().NotBe(0);

			// Act		
			list.Clear();

			// Validate			
			_ = list.Count.Should().Be(0);
			_ = list.Should().HaveCount(0);

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
		public void ContainsShouldFindAllItems()
		{
			// Prepare
			var testData = _testData.ToArray();
			using var list = new RecyclableArrayList<int>(testData);
			_ = testData.Any().Should().BeTrue("we need items on the list that we can look for");

			// Act
			foreach (var item in testData)
			{
				// Validate
				_ = list.Contains(item).Should().BeTrue();
			}
		}

		[Fact]
		public void AddShouldAcceptDuplicates()
		{
			// Prepare
			int[] testNumbers = new[] { 1, 2, 2, 3 };

			// Act
			using var list = new RecyclableArrayList<int>();
			foreach (var index in testNumbers)
			{
				list.Add(index);
			}

			// Validate
			_ = list.Count.Should().Be(testNumbers.Length);
			_ = list.Should().BeEquivalentTo(testNumbers);
		}
	}
}
using FluentAssertions;
using Recyclable.Collections;
using Recyclable.Collections.Parallel;
using System.Numerics;

namespace Recyclable.CollectionsTests
{
	public class ItemRangesIteratorTests
	{
		[Theory]
		[MemberData(nameof(TestCases))]
		public void IterateShouldYieldAllItems(int startingBlockIndex, int blockSize, long itemsCount, int step, IEnumerable<(int BlockIndex, long ItemsCount)> expected)
		{
			// Prepare
			var mockList = new RecyclableList<long>();
			byte blockSizePow2BitShift = (byte)(31 - BitOperations.LeadingZeroCount((uint)blockSize));

			var synchronizationContext = new ParallelSynchronizationContext(1);
			var search = new SearchConfiguration<RecyclableList<long>, long>()
			{
				List = mockList,
				ItemToFind = 0,
				BlockSizeMinus1 = blockSize - 1,
				BlockSizePow2BitShift = blockSizePow2BitShift,
				ItemsCount = itemsCount,
				BlockSize = step,
			};

			var actualItemRanges = new List<(int BlockIndex, long ItemsCount)>();

			// Act
			ItemRangesIterator.Iterate(synchronizationContext, search, (context, search, itemRange) =>
			{
				actualItemRanges.Add((itemRange.BlockIndex, itemRange.ItemsToSearchCount));
				return true;
			});

			// Validate
			_ = actualItemRanges.Should().HaveCount(expected.Count()).And.ContainInConsecutiveOrder(expected);
		}

		public static IEnumerable<object[]> TestCases => new[]
		{
			new object[]
			{
				0, 128, 333, (long)(333 * 0.329), new(int BlockIndex, long ItemsCount)[]
				{
					(0, 109),
					(0, 109),
					(1, 109),
					(2, 6)
				}
			},

			new object[]
			{
				0, 2, 10, 2, new (int BlockIndex, long ItemsCount)[]
				{
					(0, 2),
					(1, 2),
					(2, 2),
					(3, 2),
					(4, 2),
				}
			},

			new object[]
			{
				0, 2, 10, 3, new (int BlockIndex, long ItemsCount)[]
				{
					(0, 3),
					(1, 3),
					(3, 3),
					(4, 1),
				}
			},

			new object[]
			{
				0, 1, 10, 3, new (int BlockIndex, long ItemsCount)[]
				{
					(0, 3),
					(3, 3),
					(6, 3),
					(9, 1),
				}
			},

			new object[]
			{
				0, 2, 10, 4, new (int BlockIndex, long ItemsCount)[]
				{
					(0, 4),
					(2, 4),
					(4, 2),
				}
			},

			new object[]
			{
				0, 1, 10, 2, new (int BlockIndex, long ItemsCount)[]
				{
					(0, 2),
					(2, 2),
					(4, 2),
					(6, 2),
					(8, 2),
				}
			},

			new object[]
			{
				0, 2, 10, 1, new (int BlockIndex, long ItemsCount)[]
				{
					(0, 1),
					(0, 1),
					(1, 1),
					(1, 1),
					(2, 1),
					(2, 1),
					(3, 1),
					(3, 1),
					(4, 1),
					(4, 1),
				}
			},

			new object[]
			{
				0, 1, 10, 1, new (int BlockIndex, long ItemsCount)[]
				{
					(0, 1),
					(1, 1),
					(2, 1),
					(3, 1),
					(4, 1),
					(5, 1),
					(6, 1),
					(7, 1),
					(8, 1),
					(9, 1),
				}
			},
		};
	}
}

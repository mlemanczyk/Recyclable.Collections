using FluentAssertions;
using Recyclable.Collections;
using System.Numerics;

namespace Recyclable.CollectionsTests
{
	public class ItemRangesIteratorTests
	{
		[Theory]
		[MemberData(nameof(TestCases))]
		public void IterateShouldYieldAllItems(int startingBlockIndex, int blockSize, long itemsCount, int step, IEnumerable<(int BlockIndex, int StartingItemIndex, long ItemsCount)> expected)
		{
			// Prepare
			byte blockSizePow2BitShift = (byte)(31 - BitOperations.LeadingZeroCount((uint)blockSize));
			var actual = new List<(int BlockIndex, int StartingItemIndex, long ItemsCount)>();

			// Act
			//ItemRangesIterator.Iterate(startingBlockIndex, blockSize, blockSizePow2BitShift, itemsCount, step, (searchInfo) =>
			//{
			//	actual.Add((searchInfo.BlockIndex, searchInfo.StartingItemIndex, searchInfo.ItemsToSearchCount));
			//	return true;
			//});

			// Validate
			//_ = actual.Should().HaveCount(expected.Count()).And.ContainInConsecutiveOrder(expected);
		}

		public static IEnumerable<object[]> TestCases => new[]
		{
			new object[]
			{
				0, 128, 333, (long)(333 * 0.329), new(int BlockIndex, int StartingItemIndex, long ItemsCount)[]
				{
					(0, 0, 109),
					(0, 109, 109),
					(1, 90, 109),
					(2, 71, 6)
				}
			},

			new object[]
			{
				0, 2, 10, 2, new (int BlockIndex, int StartingItemIndex, long ItemsCount)[]
				{
					(0, 0, 2),
					(1, 0, 2),
					(2, 0, 2),
					(3, 0, 2),
					(4, 0, 2),
				}
			},

			new object[]
			{
				0, 2, 10, 3, new (int BlockIndex, int StartingItemIndex, long ItemsCount)[]
				{
					(0, 0, 3),
					(1, 1, 3),
					(3, 0, 3),
					(4, 1, 1),
				}
			},

			new object[]
			{
				0, 1, 10, 3, new (int BlockIndex, int StartingItemIndex, long ItemsCount)[]
				{
					(0, 0, 3),
					(3, 0, 3),
					(6, 0, 3),
					(9, 0, 1),
				}
			},

			new object[]
			{
				0, 2, 10, 4, new (int BlockIndex, int StartingItemIndex, long ItemsCount)[]
				{
					(0, 0, 4),
					(2, 0, 4),
					(4, 0, 2),
				}
			},

			new object[]
			{
				0, 1, 10, 2, new (int BlockIndex, int StartingItemIndex, long ItemsCount)[]
				{
					(0, 0, 2),
					(2, 0, 2),
					(4, 0, 2),
					(6, 0, 2),
					(8, 0, 2),
				}
			},

			new object[]
			{
				0, 2, 10, 1, new (int BlockIndex, int StartingItemIndex, long ItemsCount)[]
				{
					(0, 0, 1),
					(0, 1, 1),
					(1, 0, 1),
					(1, 1, 1),
					(2, 0, 1),
					(2, 1, 1),
					(3, 0, 1),
					(3, 1, 1),
					(4, 0, 1),
					(4, 1, 1),
				}
			},

			new object[]
			{
				0, 1, 10, 1, new (int BlockIndex, int StartingItemIndex, long ItemsCount)[]
				{
					(0, 0, 1),
					(1, 0, 1),
					(2, 0, 1),
					(3, 0, 1),
					(4, 0, 1),
					(5, 0, 1),
					(6, 0, 1),
					(7, 0, 1),
					(8, 0, 1),
					(9, 0, 1),
				}
			},

		};
	}
}

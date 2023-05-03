using System.Runtime.CompilerServices;
using Recyclable.Collections.Parallel;

namespace Recyclable.Collections
{
    public static class ItemRangesIterator
	{
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.NoInlining)]
		public static void Iterate<TList, T>(in ParallelSynchronizationContext context, in SearchConfiguration<TList, T> search, Func<ParallelSynchronizationContext, SearchConfiguration<TList, T>, ItemRange, bool> action)
		{
			int itemIndex = 0;
			ItemRange itemRange;
			int blockIndex = search.StartingBlockIndex;
			long itemsCount = search.ItemsCount;
			while (itemsCount > search.Step)
			{
				itemRange = new(blockIndex, itemIndex, search.Step);
				if (!action(context, search, itemRange))
				{
					break;
				}

				itemsCount -= search.Step;
				//blockIndex = (int)Math.DivRem(itemIndex + step, blockSize, out itemIndex);
				blockIndex += (int)((itemIndex + search.Step) >> search.BlockSizePow2BitShift);
				itemIndex = (int)((itemIndex + search.Step) & search.BlockSizeMinus1);
				//itemIndex += step;
				//while (itemIndex >= blockSize)
				//{
				//	blockIndex++;
				//	itemIndex -= blockSize;
				//}
			}

			itemRange = new(blockIndex, itemIndex, itemsCount);
			_ = action(context, search, itemRange);
		}
	}
}

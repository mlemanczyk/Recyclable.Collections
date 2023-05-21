using System.Runtime.CompilerServices;
using Recyclable.Collections.Parallel;

namespace Recyclable.Collections
{
    public static class ItemRangesIterator
	{
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.NoInlining)]
		public static void Iterate<TList, T>(in ParallelSynchronizationContext context, in SearchConfiguration<TList, T> search, Func<ParallelSynchronizationContext, SearchConfiguration<TList, T>, ItemRange, bool> action)
		{
			ItemRange itemRange;
			int blockIndex = search.StartingBlockIndex;
			long itemsCount = search.ItemsCount;
			while (itemsCount > search.BlockSize)
			{
				itemRange = new(blockIndex, search.BlockSize);
				if (!action(context, search, itemRange))
				{
					break;
				}

				itemsCount -= search.BlockSize;
				blockIndex++;
			}

			itemRange = new(blockIndex, itemsCount);
			_ = action(context, search, itemRange);
		}
	}
}

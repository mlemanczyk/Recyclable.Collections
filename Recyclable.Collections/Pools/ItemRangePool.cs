using Microsoft.Extensions.ObjectPool;

namespace Recyclable.Collections.Pools
{
    public static class ItemRangePool
	{
		private static readonly SpinLock _updateLock = new(false);
		public static readonly ObjectPool<ItemRange> Shared = new DefaultObjectPool<ItemRange>(new DefaultPooledObjectPolicy<ItemRange>());

		public static ItemRange Create(int blockIndex, long itemsToSearchCount)
		{
			bool lockTaken = false;
			_updateLock.Enter(ref lockTaken);
			var itemRange = Shared.Get();
			if (lockTaken)
			{
				_updateLock.Exit(false);
			}

			itemRange._returned = false;
			itemRange.BlockIndex = blockIndex;
			itemRange.ItemsToSearchCount = itemsToSearchCount;
			return itemRange;
		}
	}
}

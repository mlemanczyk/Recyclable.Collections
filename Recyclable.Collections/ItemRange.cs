using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
	public sealed class ItemRange
	{
		public int BlockIndex;
		public int StartingItemIndex;
		public long ItemsToSearchCount;
		internal bool _returned;

		public ItemRange()
		{
		}

		public ItemRange(int blockIndex, int startingItemIndex, long itemsToSearchCount)
		{
			BlockIndex = blockIndex;
			ItemsToSearchCount = itemsToSearchCount;
			StartingItemIndex = startingItemIndex;
		}

		public void Dispose()
		{
			if (!_returned)
			{
				_returned = true;
				ItemRangePool.Shared.Return(this);
			}
		}
	}
}

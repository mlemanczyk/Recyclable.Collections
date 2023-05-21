using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void PooledList_AddRangeWhenSourceIsPooledList()
		{
			var data = TestObjectsAsPooledList;
			using var list = new PooledList<long>(capacity: TestObjectCount, ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableArrayList_AddRangeWhenSourceIsRecyclableArrayList()
		{
			var data = TestObjectsAsRecyclableArrayList;
			using var list = new RecyclableArrayList<long>();
			list.AddRange(data);
		}

		public void RecyclableLongList_AddRangeWhenSourceIsRecyclableArrayList()
		{
			var data = TestObjectsAsRecyclableArrayList;
			using var list = new RecyclableLongList<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}
	}
}

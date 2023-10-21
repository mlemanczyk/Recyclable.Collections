using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void PooledList_AddRangeWhenSourceIsPooledList()
		{
			var data = TestObjectsAsPooledList;
			using var list = new PooledList<int>(capacity: TestObjectCount, ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableList_AddRangeWhenSourceIsRecyclableList()
		{
			var data = TestObjectsAsRecyclableList;
			using var list = new RecyclableList<int>(TestObjectCount);
			list.AddRange(data);
		}

		public void RecyclableLongList_AddRangeWhenSourceIsRecyclableLongList()
		{
			var data = TestObjectsAsRecyclableList;
			using var list = new RecyclableLongList<int>(minBlockSize: BlockSize, initialCapacity: TestObjectCount);
			list.AddRange(data);
		}
	}
}

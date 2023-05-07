using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void List_AddRangeWhenSourceIsIList()
		{
			var data = (IList<long>)TestObjectsAsRecyclableList;
			var list = new List<long>();
			list.AddRange(data);
		}

		public void PooledList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<long>)TestObjectsAsPooledList;
			using var list = new PooledList<long>(capacity: TestObjectCount, ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableArrayList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<long>)TestObjectsAsRecyclableList;
			using var list = new RecyclableArrayList<long>();
			list.AddRange(data);
		}

		public void RecyclableList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<long>)TestObjectsAsRecyclableList;
			using var list = new RecyclableList<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}
	}
}

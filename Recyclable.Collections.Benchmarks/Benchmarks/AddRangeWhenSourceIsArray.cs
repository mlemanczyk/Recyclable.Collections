using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void List_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			var list = new List<long>();
			list.AddRange(data);
		}

		public void PooledList_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			using var list = new PooledList<long>(capacity: TestObjectCount, ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableArrayList_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			using var list = new RecyclableArrayList<long>();
			list.AddRange(data);
		}

		public void RecyclableList_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			using var list = new RecyclableList<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}
	}
}

using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void List_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			var list = new List<long>();
			list.AddRange(data);
		}

		public void PooledList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new PooledList<long>(capacity: TestObjectCount, ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableArrayList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new RecyclableArrayList<long>();
			list.AddRange(data);
		}

		public void RecyclableList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new RecyclableList<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			list.AddRange(data);
		}
	}
}

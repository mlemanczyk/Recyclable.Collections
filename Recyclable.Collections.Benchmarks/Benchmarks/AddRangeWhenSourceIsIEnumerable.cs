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
			using var list = new PooledList<long>(ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new RecyclableList<long>();
			list.AddRange(data);
		}

		public void RecyclableLongList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new RecyclableLongList<long>(minBlockSize: BlockSize);
			list.AddRange(data);
		}
	}
}

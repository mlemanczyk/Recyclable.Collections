using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			var list = new long[TestObjectCount];
			data.CopyTo(list, 0);
		}

		public void List_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			var list = new List<long>();
			list.AddRange(data);
		}

		public void PooledList_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			using var list = new PooledList<long>(ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableList_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			using var list = new RecyclableList<long>();
			list.AddRange(data);
		}

		public void RecyclableLongList_AddRangeWhenSourceIsList()
		{
			var data = TestObjectsAsList;
			using var list = new RecyclableLongList<long>(minBlockSize: BlockSize);
			list.AddRange(data);
		}
	}
}

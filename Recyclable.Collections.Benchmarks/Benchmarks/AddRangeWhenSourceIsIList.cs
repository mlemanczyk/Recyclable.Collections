using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_AddRangeWhenSourceIsIList()
		{
			var data = (IList<long>)TestObjectsAsRecyclableLongList;
			var list = new long[TestObjectCount];
			data.CopyTo(list, 0);
		}

		public void List_AddRangeWhenSourceIsIList()
		{
			var data = (IList<long>)TestObjectsAsRecyclableLongList;
			var list = new List<long>();
			list.AddRange(data);
		}

		public void PooledList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<long>)TestObjectsAsRecyclableLongList;
			using var list = new PooledList<long>(ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<long>)TestObjectsAsRecyclableLongList;
			using var list = new RecyclableList<long>();
			list.AddRange(data);
		}

		public void RecyclableLongList_AddRangeWhenSourceIsIList()
		{
			var data = (IList<long>)TestObjectsAsRecyclableLongList;
			using var list = new RecyclableLongList<long>(minBlockSize: BlockSize);
			list.AddRange(data);
		}
	}
}

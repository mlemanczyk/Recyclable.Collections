using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			var list = new int[TestObjectCount];
			data.CopyTo(list, 0);
		}

		public void List_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			var list = new List<int>();
			list.AddRange(data);
		}

		public void PooledList_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			using var list = new PooledList<int>(ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableList_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			using var list = new RecyclableList<int>();
			list.AddRange(data);
		}

		public void RecyclableLongList_AddRangeWhenSourceIsArray()
		{
			var data = TestObjects;
			using var list = new RecyclableLongList<int>(minBlockSize: BlockSize);
			list.AddRange(data);
		}
	}
}

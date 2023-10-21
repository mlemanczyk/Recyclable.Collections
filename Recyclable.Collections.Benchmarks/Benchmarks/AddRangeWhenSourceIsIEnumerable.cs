using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			var list = new int[TestObjectCount];
			int itemIndex = 0;

			foreach (var item in data)
			{
				list[itemIndex++] = item;
			}
		}

		public void List_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			var list = new List<int>();
			list.AddRange(data);
		}

		public void PooledList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new PooledList<int>(ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new RecyclableList<int>();
			list.AddRange(data);
		}

		public void RecyclableLongList_AddRangeWhenSourceIsIEnumerable()
		{
			var data = TestObjectsAsIEnumerable;
			using var list = new RecyclableLongList<int>(minBlockSize: BlockSize);
			list.AddRange(data);
		}
	}
}

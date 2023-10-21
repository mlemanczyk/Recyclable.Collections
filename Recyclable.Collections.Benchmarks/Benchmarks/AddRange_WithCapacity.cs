using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new int[dataCount];
			data.CopyTo(list, 0);
		}

		public void List_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new List<int>(dataCount);
			list.AddRange(data);
		}

		public void PooledList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new PooledList<int>(capacity: dataCount, ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableList<int>(dataCount);
			list.AddRange(data);
		}

		public void RecyclableLongList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableLongList<int>(minBlockSize: BlockSize, initialCapacity: dataCount);
			list.AddRange(data);
		}
	}
}

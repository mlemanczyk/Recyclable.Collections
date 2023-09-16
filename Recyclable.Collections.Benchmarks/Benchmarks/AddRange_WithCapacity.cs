using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new long[dataCount];
			data.CopyTo(list, 0);
		}

		public void List_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new List<long>(dataCount);
			list.AddRange(data);
		}

		public void PooledList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new PooledList<long>(capacity: dataCount, ClearMode.Auto);
			list.AddRange(data);
		}

		public void RecyclableList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableList<long>(dataCount);
			list.AddRange(data);
		}

		public void RecyclableLongList_AddRange_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableLongList<long>(minBlockSize: BlockSize, initialCapacity: dataCount);
			list.AddRange(data);
		}
	}
}

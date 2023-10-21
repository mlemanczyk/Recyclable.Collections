using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_Create_WithCapacity()
		{
			var data = new int[TestObjectCount];
			DoNothing(data);
		}

		public void List_Create_WithCapacity()
		{
			var list = new List<int>(TestObjectCount);
			DoNothing(list);
		}

		public void PooledList_Create_WithCapacity()
		{
			using var list = new PooledList<int>(capacity: TestObjectCount, ClearMode.Auto);
			DoNothing(list);
		}

		public void RecyclableList_Create_WithCapacity()
		{
			using var list = new RecyclableList<int>(TestObjectCount);
			DoNothing(list);
		}

		public void RecyclableLongList_Create_WithCapacity()
		{
			using var list = new RecyclableLongList<int>(minBlockSize: BlockSize, initialCapacity: TestObjectCount);
			DoNothing(list);
		}
	}
}

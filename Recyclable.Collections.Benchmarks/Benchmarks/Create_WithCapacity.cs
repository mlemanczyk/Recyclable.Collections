using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_Create_WithCapacity()
		{
			var data = new long[TestObjectCount];
			DoNothing(data);
		}

		public void List_Create_WithCapacity()
		{
			var list = new List<long>(TestObjectCount);
			DoNothing(list);
		}

		public void PooledList_Create_WithCapacity()
		{
			using var list = new PooledList<long>(capacity: TestObjectCount, ClearMode.Auto);
			DoNothing(list);
		}

		public void RecyclableList_Create_WithCapacity()
		{
			using var list = new RecyclableList<long>(TestObjectCount);
			DoNothing(list);
		}

		public void RecyclableLongList_Create_WithCapacity()
		{
			using var list = new RecyclableLongList<long>(minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			DoNothing(list);
		}
	}
}

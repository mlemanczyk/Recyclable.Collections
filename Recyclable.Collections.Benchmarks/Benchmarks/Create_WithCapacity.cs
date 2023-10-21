using Collections.Pooled;
using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_Create_WithCapacity()
		{
			var data = new int[TestObjectCount];
			DoNothing.With(data);
		}

		public void List_Create_WithCapacity()
		{
			var list = new List<int>(TestObjectCount);
			DoNothing.With(list);
		}

		public void PooledList_Create_WithCapacity()
		{
			using var list = new PooledList<int>(capacity: TestObjectCount, ClearMode.Auto);
			DoNothing.With(list);
		}

		public void RecyclableList_Create_WithCapacity()
		{
			using var list = new RecyclableList<int>(TestObjectCount);
			DoNothing.With(list);
		}

		public void RecyclableLongList_Create_WithCapacity()
		{
			using var list = new RecyclableLongList<int>(minBlockSize: BlockSize, initialCapacity: TestObjectCount);
			DoNothing.With(list);
		}
	}
}

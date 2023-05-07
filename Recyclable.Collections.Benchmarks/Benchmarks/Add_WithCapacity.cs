using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void List_Add_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new List<long>(dataCount);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		public void PooledList_Add_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new PooledList<long>(capacity: dataCount, ClearMode.Auto);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		public void RecyclableArrayList_Add_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableArrayList<long>(dataCount);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		public void RecyclableList_Add_WithCapacity()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableList<long>(minBlockSize: BlockSize, expectedItemsCount: dataCount);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}
	}
}

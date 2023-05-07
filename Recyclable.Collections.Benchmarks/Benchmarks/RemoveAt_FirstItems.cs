using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void List_RemoveAt_FirstItems()
		{
			var data = TestObjects;
			var list = new List<long>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				list.RemoveAt(i);
			}
		}

		public void PooledList_RemoveAt_FirstItems()
		{
			var data = TestObjects;
			using var list = new PooledList<long>(data, ClearMode.Auto);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				list.RemoveAt(i);
			}
		}

		public void RecyclableArrayList_RemoveAt_FirstItems()
		{
			var data = TestObjects;
			using var list = new RecyclableArrayList<long>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				list.RemoveAt(i);
			}
		}

		public void RecyclableList_RemoveAt_FirstItems()
		{
			var data = TestObjects;
			using var list = new RecyclableList<long>(data, minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				list.RemoveAt(i);
			}
		}
	}
}

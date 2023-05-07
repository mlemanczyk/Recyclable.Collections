using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void List_Remove_LastItems()
		{
			var data = TestObjects;
			var list = new List<long>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		public void PooledList_Remove_LastItems()
		{
			var data = TestObjects;
			using var list = new PooledList<long>(data, ClearMode.Auto);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		public void RecyclableArrayList_Remove_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableArrayList<long>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		public void RecyclableList_Remove_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableList<long>(data, minBlockSize: BlockSize, expectedItemsCount: TestObjectCount);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}
	}
}

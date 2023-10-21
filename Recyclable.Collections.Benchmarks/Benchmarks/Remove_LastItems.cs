using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void List_Remove_LastItems()
		{
			var data = TestObjects;
			var list = new List<int>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		public void PooledList_Remove_LastItems()
		{
			var data = TestObjects;
			using var list = new PooledList<int>(data, ClearMode.Auto);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		public void RecyclableList_Remove_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableList<int>(data);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}

		public void RecyclableLongList_Remove_LastItems()
		{
			var data = TestObjects;
			using var list = new RecyclableLongList<int>(data, minBlockSize: BlockSize, initialCapacity:base.TestObjectCount);
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = data.Length - 1; i > data.Length - dataCount - 1; i--)
			{
				DoNothing(list.Remove(data[i]));
			}
		}
	}
}

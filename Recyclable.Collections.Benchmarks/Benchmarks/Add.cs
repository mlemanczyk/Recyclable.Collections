using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void List_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new List<long>();
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		public void PooledList_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new PooledList<long>(ClearMode.Auto);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		public void RecyclableArrayList_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableArrayList<long>();
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		public void RecyclableLongList_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableLongList<long>();
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}
	}
}

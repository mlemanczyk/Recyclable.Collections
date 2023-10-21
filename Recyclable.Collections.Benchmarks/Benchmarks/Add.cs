using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new int[dataCount];
			for (var i = 0; i < dataCount; i++)
			{
				list[i] = data[i];
			}
		}

		public void List_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			var list = new List<int>();
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		public void PooledList_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new PooledList<int>(ClearMode.Auto);
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		public void RecyclableList_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableList<int>();
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}

		public void RecyclableLongList_Add()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			using var list = new RecyclableLongList<int>();
			for (var i = 0; i < dataCount; i++)
			{
				list.Add(data[i]);
			}
		}
	}
}

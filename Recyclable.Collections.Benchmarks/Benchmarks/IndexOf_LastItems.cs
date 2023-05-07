namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(Array.IndexOf(list, data[^(i + 1)]));
			}
		}

		public void List_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void PooledList_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void RecyclableArrayList_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableArrayList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void RecyclableList_IndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void RecyclableList_LongIndexOf_LastItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount / 10 > 0 ? TestObjectCount / 10 : TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.LongIndexOf(data[^(i + 1)]));
			}
		}
	}
}

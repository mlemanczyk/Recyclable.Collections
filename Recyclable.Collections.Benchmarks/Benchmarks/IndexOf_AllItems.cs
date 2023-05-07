namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_IndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(Array.IndexOf(list, data[i]));
			}
		}

		public void List_IndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
			}
		}

		public void PooledList_IndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
			}
		}

		public void RecyclableArrayList_IndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableArrayList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
			}
		}

		public void RecyclableList_IndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
			}
		}

		public void RecyclableList_LongIndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.LongIndexOf(data[i]));
			}
		}
	}
}

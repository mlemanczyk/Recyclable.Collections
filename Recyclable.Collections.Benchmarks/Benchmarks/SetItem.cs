namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_SetItem()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			long index = 0;
			for (var i = 0; i < dataCount; i++)
			{
				data[index] = TestObjects[index++];
			}
		}

		public void List_SetItem()
		{
			var data = TestObjectsAsList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		public void PooledList_SetItem()
		{
			var data = TestObjectsAsPooledList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		public void RecyclableList_SetItem()
		{
			var data = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}

		public void RecyclableLongList_SetItem()
		{
			var data = TestObjectsAsRecyclableLongList;
			var dataCount = TestObjectCount;
			for (var i = 0; i < dataCount; i++)
			{
				data[i] = TestObjects[i];
			}
		}
	}
}

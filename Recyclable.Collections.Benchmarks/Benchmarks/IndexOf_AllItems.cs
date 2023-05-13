namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		private const long MaxItemsToSearchFor = RecyclableDefaults.MinItemsCountForParallelization * 3;

		public void Array_IndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 1; i <= dataCount; i++)
			{
				DoNothing(Array.IndexOf(list, data[^i]));
			}
		}

		public void List_IndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 1; i <= dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^i]));
			}
		}

		public void PooledList_IndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 1; i <= dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^i]));
			}
		}

		public void RecyclableArrayList_IndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableArrayList;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 1; i <= dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^i]));
			}
		}

		public void RecyclableList_IndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 1; i <= dataCount; i++)
			{
				DoNothing(list.IndexOf(data[^i]));
			}
		}

		public void RecyclableList_LongIndexOf_AllItems()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 1; i <= dataCount; i++)
			{
				DoNothing(list.LongIndexOf(data[^i]));
			}
		}
	}
}

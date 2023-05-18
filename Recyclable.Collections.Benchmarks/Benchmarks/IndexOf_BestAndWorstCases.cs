namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		private const long MaxItemsToSearchFor = 100_000;// RecyclableDefaults.MinItemsCountForParallelization * 3;

		public void Array_IndexOf_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(Array.IndexOf(list, data[i]));
				DoNothing(Array.IndexOf(list, data[^(i + 1)]));
			}
		}

		public void List_IndexOf_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void PooledList_IndexOf_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void RecyclableArrayList_IndexOf_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableArrayList;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void RecyclableList_IndexOf_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
				DoNothing(list.IndexOf(data[^(i + 1)]));
			}
		}

		public void RecyclableList_LongIndexOf_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = Math.Min(TestObjectCount, MaxItemsToSearchFor);
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.IndexOf(data[i]));
				DoNothing(list.LongIndexOf(data[^(i + 1)]));
			}
		}
	}
}

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_Find_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(Array.Find(list, (x) => x == data[i]));
				DoNothing(Array.Find(list, (x) => x == data[^(i + 1)]));
			}
		}

		public void List_Find_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Find((x) => x == data[i]));
				DoNothing(list.Find((x) => x == data[^(i + 1)]));
			}
		}

		public void RecyclableList_Find_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.Find((x) => x == data[i]));
				DoNothing(list.Find((x) => x == data[^(i + 1)]));
			}
		}

		// public void RecyclableLongList_Find_BestAndWorstCases()
		// {
		// 	var data = TestObjects;
		// 	var list = TestObjectsAsRecyclableLongList;
		// 	var dataCount = TestObjectCountForSlowMethods;
		// 	for (var i = 0; i < dataCount; i++)
		// 	{
		// 		DoNothing(list.Find((x) => x == data[i]));
		// 		DoNothing(list.Find((x) => x == data[^(i + 1)]));
		// 	}
		// }
	}
}

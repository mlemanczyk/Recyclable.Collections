namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_FindLast_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(Array.FindLast(list, (x) => x == data[i]));
				DoNothing(Array.FindLast(list, (x) => x == data[^(i + 1)]));
			}
		}

		public void List_FindLast_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.FindLast((x) => x == data[i]));
				DoNothing(list.FindLast((x) => x == data[^(i + 1)]));
			}
		}

		public void RecyclableList_FindLast_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.FindLast((x) => x == data[i]));
				DoNothing(list.FindLast((x) => x == data[^(i + 1)]));
			}
		}

		// public void RecyclableLongList_FindLast_BestAndWorstCases()
		// {
		// 	var data = TestObjects;
		// 	var list = TestObjectsAsRecyclableLongList;
		// 	var dataCount = TestObjectCountForSlowMethods;
		// 	for (var i = 0; i < dataCount; i++)
		// 	{
		// 		DoNothing(list.FindLast((x) => x == data[i]));
		// 		DoNothing(list.FindLast((x) => x == data[^(i + 1)]));
		// 	}
		// }
	}
}

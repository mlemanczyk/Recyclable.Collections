namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_FindAll_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(Array.FindAll(list, (item) => item == data[i]));
				DoNothing(Array.FindAll(list, (item) => item == data[^(i + 1)]));
			}
		}

		public void List_FindAll_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.FindAll((item) => item == data[i]));
				DoNothing(list.FindAll((item) => item == data[^(i + 1)]));
			}
		}

		public void PooledList_FindAll_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				var temp = list.FindAll((item) => item == data[i]);
				DoNothing(temp);
				temp.Dispose();

				temp = list.FindAll((item) => item == data[^(i + 1)]);
				DoNothing(temp);
				temp.Dispose();
			}
		}

		public void RecyclableList_FindAll_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				var temp = list.FindAll((item) => item == data[i]);
				DoNothing(temp);
				temp.Dispose();

				temp = list.FindAll((item) => item == data[^(i + 1)]);
				DoNothing(temp);
				temp.Dispose();
			}
		}

		// public void RecyclableLongList_FindAll_BestAndWorstCases()
		// {
		// 	var data = TestObjects;
		// 	var list = TestObjectsAsRecyclableLongList;
		// 	var dataCount = TestObjectCountForSlowMethods;
		// 	for (var i = 0; i < dataCount; i++)
		// 	{
		// 		var temp = list.FindAll((item) => item == data[i]);
		// 		DoNothing(temp);
		// 		temp.Dispose();

		// 		temp = list.FindAll((item) => item == data[^(i + 1)]);
		// 		DoNothing(temp);
		// 		temp.Dispose();
		// 	}
		// }
	}
}

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_FindLastIndex_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = TestObjectCountForSlowMethods;
			// Console.WriteLine($"{nameof(TestObjectCount)}={TestObjectCount}");
			// Console.WriteLine($"{nameof(TestObjectCountForSlowMethods)}={TestObjectCountForSlowMethods}");
			for (var i = 0; i < dataCount; i++)
			{
				// Console.WriteLine($"data.Length={data.Length}");
				// Console.WriteLine($"{nameof(TestObjectCount)}={TestObjectCount}");
				// Console.WriteLine($"{nameof(TestObjectCountForSlowMethods)}={TestObjectCountForSlowMethods}");
				DoNothing(Array.FindLastIndex(list, 0, (x) => x == data[i]));
				DoNothing(Array.FindLastIndex(list, 0, (x) => x == data[^(i + 1)]));
			}
		}

		public void List_FindLastIndex_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = TestObjectCountForSlowMethods;
			// Console.WriteLine($"{nameof(TestObjectCount)}={TestObjectCount}");
			// Console.WriteLine($"{nameof(TestObjectCountForSlowMethods)}={TestObjectCountForSlowMethods}");
			for (var i = 0; i < dataCount; i++)
			{
				// Console.WriteLine($"data.Length={data.Length}");
				// Console.WriteLine($"{nameof(TestObjectCount)}={TestObjectCount}");
				// Console.WriteLine($"{nameof(TestObjectCountForSlowMethods)}={TestObjectCountForSlowMethods}");
				DoNothing(list.FindLastIndex(0, (x) => x == data[i]));
				DoNothing(list.FindLastIndex(0, (x) => x == data[^(i + 1)]));
			}
		}

		public void RecyclableList_FindLastIndex_BestAndWorstCases()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCountForSlowMethods;
			for (var i = 0; i < dataCount; i++)
			{
				DoNothing(list.FindLastIndex(0, (x) => x == data[i]));
				DoNothing(list.FindLastIndex(0, (x) => x == data[^(i + 1)]));
			}
		}

		// public void RecyclableLongList_FindLastIndex_BestAndWorstCases()
		// {
		// 	var data = TestObjects;
		// 	var list = TestObjectsAsRecyclableLongList;
		// 	var dataCount = TestObjectCountForSlowMethods;
		// 	for (var i = 0; i < dataCount; i++)
		// 	{
		// 		DoNothing(list.FindLastIndex(0, (x) => x == data[i]));
		// 		DoNothing(list.FindLastIndex(0, (x) => x == data[^(i + 1)]));
		// 	}
		// }
	}
}

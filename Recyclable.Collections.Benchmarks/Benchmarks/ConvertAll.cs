namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_ConvertAll()
		{
			var data = TestObjects;
			var list = TestObjects;
			var dataCount = TestObjectCountForSlowMethods;
			DoNothing(Array.ConvertAll(list, static (item) => (long) item));
		}

		public void List_ConvertAll()
		{
			var data = TestObjects;
			var list = TestObjectsAsList;
			var dataCount = TestObjectCountForSlowMethods;
			DoNothing(list.ConvertAll(static (item) => (long)item));
		}

		public void PooledList_ConvertAll()
		{
			var data = TestObjects;
			var list = TestObjectsAsPooledList;
			var dataCount = TestObjectCountForSlowMethods;
			using var converted = list.ConvertAll(static (item) => (long)item);
			DoNothing(converted);
		}

		public void RecyclableList_ConvertAll()
		{
			var data = TestObjects;
			var list = TestObjectsAsRecyclableList;
			var dataCount = TestObjectCountForSlowMethods;
			using RecyclableList<long> converted = list.ConvertAll(static (item) => (long)item);
			DoNothing(converted);
		}

		// public void RecyclableLongList_ConvertAll()
		// {
		// 	var data = TestObjects;
		// 	var list = TestObjectsAsRecyclableLongList;
		// 	var dataCount = TestObjectCountForSlowMethods;
		// 	using RecyclableList<long> converted = list.ConvertAll(static (item) => (long)item);
		// 	DoNothing(converted);
		// }
	}
}

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_Count()
		{
			var data = TestObjects;
			DoNothing(data.Length);
		}

		public void List_Count()
		{
			var data = TestObjectsAsList;
			DoNothing(data.Count);
		}

		public void PooledList_Count()
		{
			var data = TestObjectsAsPooledList;
			DoNothing(data.Count);
		}

		public void RecyclableArrayList_Count()
		{
			var data = TestObjectsAsRecyclableArrayList;
			DoNothing(data.Count);
		}

		public void RecyclableList_Count()
		{
			var data = TestObjectsAsRecyclableList;
			DoNothing(data.Count);
		}
	}
}

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_LongCount()
		{
			var data = TestObjects;
			DoNothing(data.LongLength);
		}

		public void RecyclableList_LongCount()
		{
			var data = TestObjectsAsRecyclableList;
			DoNothing(data.LongCount);
		}
	}
}

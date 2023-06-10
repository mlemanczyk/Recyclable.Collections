namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_LongCount()
		{
			var data = TestObjects;
			DoNothing(data.LongLength);
		}

		public void RecyclableLongList_LongCount()
		{
			var data = TestObjectsAsRecyclableLongList;
			DoNothing(data.LongCount);
		}
	}
}

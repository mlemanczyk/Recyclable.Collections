using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		public void Array_LongCount()
		{
			var data = TestObjects;
			DoNothing.With(data.LongLength);
		}

		public void RecyclableLongList_LongCount()
		{
			var data = TestObjectsAsRecyclableLongList;
			DoNothing.With(data.LongCount);
		}
	}
}

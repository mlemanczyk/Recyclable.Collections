using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void List_Create()
		{
			var list = new List<object>();
			DoNothing(list);
		}

		//[Benchmark(Baseline = true)]
		public void PooledList_Create()
		{
			using var list = new PooledList<object>(ClearMode.Always);
			DoNothing(list);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_Create()
		{
			using var list = new RecyclableArrayList<object>();
			DoNothing(list);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_Create()
		{
			using var list = new RecyclableList<object>();
			DoNothing(list);
		}
	}
}

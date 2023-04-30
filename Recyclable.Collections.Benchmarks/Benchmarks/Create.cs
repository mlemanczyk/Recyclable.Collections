using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		//[Benchmark(Baseline = false)]
		public void List_Create()
		{
			var list = new List<long>();
			DoNothing(list);
		}

		//[Benchmark(Baseline = true)]
		public void PooledList_Create()
		{
			using var list = new PooledList<long>(ClearMode.Auto);
			DoNothing(list);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableArrayList_Create()
		{
			using var list = new RecyclableArrayList<long>();
			DoNothing(list);
		}

		//[Benchmark(Baseline = false)]
		public void RecyclableList_Create()
		{
			using var list = new RecyclableList<long>();
			DoNothing(list);
		}
	}
}

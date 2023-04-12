namespace Recyclable.Collections.Benchmarks.POC
{
	public partial class RecyclableListPocBenchmarks : PocBenchmarkBase
	{
		public static void Run()
		{
			var benchmark = new RecyclableListPocBenchmarks();
			benchmark.RecyclableList_EnsureCapacityV1_ByPowOf2();
			benchmark.RecyclableList_EnsureCapacityV2_ByPowOf2();
			benchmark.RecyclableList_EnsureCapacity_ByPowOf2();
			benchmark.RecyclableList_EnsureCapacityV1_ByBlockSize();
			benchmark.RecyclableList_EnsureCapacityV2_ByBlockSize();
			benchmark.RecyclableList_EnsureCapacity_ByBlockSize();
		}
	}
}

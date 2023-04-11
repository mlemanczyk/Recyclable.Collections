namespace Recyclable.Collections.Benchmarks.POC
{
	public partial class RecyclableListPocBenchmarks : PocBenchmarkBase
	{
		public static void Run()
		{
			var benchmark = new RecyclableListPocBenchmarks();
			benchmark.RecyclableList_EnsureCapacity_Old_ByPowOf2();
			benchmark.RecyclableList_EnsureCapacity_ByPowOf2();
			benchmark.RecyclableList_EnsureCapacity_Old_ByBlockSize();
			benchmark.RecyclableList_EnsureCapacity_ByBlockSize();
		}
	}
}

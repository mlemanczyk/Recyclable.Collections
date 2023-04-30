using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks.POC
{
	public class SpanVsArrayBenchmarks : BenchmarkBase
	{
		//[Benchmark]
		public void ArrayGetWithVar()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			for (long i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		//[Benchmark(Baseline = true)]
		public void ArrayGetWithField()
		{
			var dataCount = TestObjectCount;
			for (long i = 0; i < dataCount; i++)
			{
				DoNothing(_testObjects![i]);
			}
		}

		//[Benchmark]
		public void SpanGet()
		{
			var dataCount = TestObjectCount;
			Span<long> data = new(_testObjects, 0, dataCount);
			for (int i = 0; i < dataCount; i++)
			{
				DoNothing(data[i]);
			}
		}

		[Benchmark]
		public void ArraySetWithVar()
		{
			var data = TestObjects;
			var dataCount = TestObjectCount;
			for (long i = 0; i < dataCount; i++)
			{
				data[i] = i;
			}
		}

		[Benchmark(Baseline = true)]
		public void ArraySetWithField()
		{
			var dataCount = TestObjectCount;
			for (long i = 0; i < dataCount; i++)
			{
				_testObjects![i] = i;
			}
		}

		[Benchmark]
		public void SpanSet()
		{
			var dataCount = TestObjectCount;
			Span<long> data = new(_testObjects, 0, dataCount);
			for (int i = 0; i < dataCount; i++)
			{
				data[i] = i;
			}
		}
	}
}

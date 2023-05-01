using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections.Benchmarks.POC
{
	[MemoryDiagnoser]
	public class BoolOrComparePocBenchmarks
	{
		public static void Run()
		{
			var benchmark = new BoolOrComparePocBenchmarks();
			benchmark.Bool();
			benchmark.Compare();
		}

		private static void DoNothing<T>(T result, [CallerMemberName] string? callerName = null)
		{
			//Console.WriteLine($"{callerName}: result = {result}");
		}

		private readonly bool _testBool = true;
		private readonly int _blockSize = 8;

		[Benchmark(Baseline = false)]
		public void Bool()
		{
			if (_testBool)
			{
				DoNothing(_testBool);
			}
			else
			{
				DoNothing(_testBool);
			}
		}

		[Benchmark(Baseline = false)]
		public void Compare()
		{
			if (_blockSize >= RecyclableDefaults.MinPooledArrayLength)
			{
				DoNothing(_testBool);
			}
			else
			{
				DoNothing(_testBool);
			}
		}
	}
}
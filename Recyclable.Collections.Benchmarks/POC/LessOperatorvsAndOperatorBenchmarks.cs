using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections.Benchmarks.POC
{
	public class LessOperatorVsAndOperatorBenchmarks
	{
		private const int bitShift = 1 << 31;
		private static Random _rnd = new(1);
		private static int num = -Math.Abs(_rnd.Next());

		public void Setup()
		{
			num = -1;
			Console.WriteLine($"*** We're comparing if {num} is < 0 ***");
		}

		public void Cleanup() { }

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void DoNothing<T>(T item)
		{ }


		[Benchmark(Baseline = true)]
		public void CompareWithLess()
		{
			DoNothing(num < 0);
		}

		[Benchmark]
		public void CompareWithAnd()
		{
			DoNothing((num & bitShift) != 0);
		}

		[Benchmark]
		public void CompareWithEqual()
		{
			DoNothing(num == -1);
		}

		[Benchmark]
		public void CompareToZero()
		{
			DoNothing(num == 0);
		}
	}
}

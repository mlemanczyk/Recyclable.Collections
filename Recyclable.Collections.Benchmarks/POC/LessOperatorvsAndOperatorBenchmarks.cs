// Ignore Spelling: Poc

using BenchmarkDotNet.Attributes;
using Collections.Benchmarks.Core;

#pragma warning disable CA1822

namespace Recyclable.Collections.Benchmarks.POC
{
	public class LessOperatorVsAndOperatorBenchmarks
	{
		private const int BitShift = 1 << 31;
		private static readonly Random _rnd = new(1);
		private static int _num = -Math.Abs(_rnd.Next());

		public void Setup()
		{
			_num = -1;
			Console.WriteLine($"*** We're comparing if {_num} is < 0 ***");
		}

		public void Cleanup() { }

		[Benchmark(Baseline = true)]
		public void CompareWithLess()
		{
			DoNothing.With(_num < 0);
		}

		[Benchmark]
		public void CompareWithAnd()
		{
			DoNothing.With((_num & BitShift) != 0);
		}

		[Benchmark]
		public void CompareWithEqual()
		{
			DoNothing.With(_num == -1);
		}

		[Benchmark]
		public void CompareToZero()
		{
			DoNothing.With(_num == 0);
		}
	}
}

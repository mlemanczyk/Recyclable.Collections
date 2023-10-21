// Ignore Spelling: Poc

using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

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

#pragma warning disable RCS1163, IDE0060
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void DoNothing<T>(T item)
		{
		}

#pragma warning restore RCS1163

		[Benchmark(Baseline = true)]
		public void CompareWithLess()
		{
			DoNothing(_num < 0);
		}

		[Benchmark]
		public void CompareWithAnd()
		{
			DoNothing((_num & BitShift) != 0);
		}

		[Benchmark]
		public void CompareWithEqual()
		{
			DoNothing(_num == -1);
		}

		[Benchmark]
		public void CompareToZero()
		{
			DoNothing(_num == 0);
		}
	}
}

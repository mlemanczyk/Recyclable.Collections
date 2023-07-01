using BenchmarkDotNet.Attributes;
using System.Runtime.CompilerServices;

#pragma warning disable CA1822

namespace Recyclable.Collections.Benchmarks.POC
{
	public class LessOperatorVsAndOperatorBenchmarks
	{
		private const int bitShift = 1 << 31;
		private static readonly Random _rnd = new(1);
		private static int num = -Math.Abs(_rnd.Next());

		public void Setup()
		{
			num = -1;
			Console.WriteLine($"*** We're comparing if {num} is < 0 ***");
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

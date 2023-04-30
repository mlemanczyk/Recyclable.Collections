using BenchmarkDotNet.Attributes;
using System.Buffers;

namespace Recyclable.Collections.Benchmarks.POC
{
	public class ArraySizeLimitPocBenchmarks : PocBenchmarkBase
	{
		public static void Run()
		{
			var benchmark = new ArraySizeLimitPocBenchmarks();
			benchmark.FindArraySizeLimit();
		}

		private const int _maxArraySize = 2_147_483_605;

		[Benchmark(OperationsPerInvoke = 1)]
		public void Max_Allowed_Size_2_147_483_605()
		{
			var arr = ArrayPool<long>.Shared.Rent(_maxArraySize);
		}

		[Benchmark(OperationsPerInvoke = 1)]
		public void FindArraySizeLimit()
		{
			var pool = ArrayPool<long>.Shared;
			var bufferSize = int.MaxValue / 2;
			var oldValue = 0;
			while (true)
			{
				try
				{
					Console.Write($"Allocating {bufferSize}...");
					var arr = pool.Rent(bufferSize);
					Console.WriteLine("success");
					pool.Return(arr);
					arr = null;

					var nextValue = bufferSize - oldValue;
					oldValue = bufferSize;
					bufferSize = checked(bufferSize + nextValue);

					GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
				}
				catch (Exception)
				{
					Console.WriteLine($"error");
					var nextValue = (bufferSize - oldValue) >> 1;
					if (nextValue == 0)
					{
						break;
					}

					bufferSize = oldValue + nextValue;
				}
			}

			Console.WriteLine("All done");
			throw new Exception($"Everything was successful. You can create {oldValue} byte big arrays. Exception is logged to prevent more benchmarks.");
		}
	}
}

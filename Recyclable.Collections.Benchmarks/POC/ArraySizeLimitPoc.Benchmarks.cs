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

		[Benchmark]
		public void FindArraySizeLimit()
		{
			var pool = ArrayPool<object>.Shared;
			var bufferSize = int.MaxValue;
			var oldValue = 0;
			while (true)
			{
				try
				{
					Console.WriteLine($"Allocating {bufferSize}");
					var arr = pool.Rent((int)bufferSize);
					var nextValue = (oldValue - bufferSize) >> 1;
					if (nextValue == 0)
					{
						break;
					}

					oldValue = bufferSize;
					bufferSize += nextValue;

					pool.Return(arr);
					GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
				}
				catch (Exception)
				{
					Console.WriteLine($"Error during array creation when {nameof(bufferSize)} = {bufferSize}");
					var nextValue = (oldValue - bufferSize) >> 1;
					if (nextValue == 0)
					{
						break;
					}

					oldValue = bufferSize;
					bufferSize -= nextValue;
				}
			}

			Console.WriteLine("All done");
		}
	}
}

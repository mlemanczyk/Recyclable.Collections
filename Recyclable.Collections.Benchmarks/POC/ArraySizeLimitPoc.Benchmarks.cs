using BenchmarkDotNet.Attributes;
using System.Buffers;

namespace Recyclable.Collections.Benchmarks.POC
{
	public enum ArraySizeLimitPocBenchmarkType
	{
		ArraySizeLimit
	}

	public class ArraySizeLimitPocBenchmarks : PocBenchmarkBase<ArraySizeLimitPocBenchmarkType>
	{
		public static void Run()
		{
			var benchmark = new ArraySizeLimitPocBenchmarks();
			benchmark.Setup();
			FindArraySizeLimit();
			benchmark.Cleanup();
		}

		[Params(2_147_483_605)]
		public override int TestObjectCount { get => base.TestObjectCount; set => base.TestObjectCount = value; }

		[Benchmark(OperationsPerInvoke = 1)]
		public void Max_Allowed_Size_2_147_483_605()
		{
			long[] arr = ArrayPool<long>.Shared.Rent(TestObjectCount);
			DoNothing(arr);
		}

		[Benchmark(OperationsPerInvoke = 1)]
		public static void FindArraySizeLimit()
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
					Console.WriteLine("error");
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

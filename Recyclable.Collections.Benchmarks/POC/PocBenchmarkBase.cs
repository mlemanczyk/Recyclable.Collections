using BenchmarkDotNet.Attributes;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections.Benchmarks.POC
{
	public abstract class PocBenchmarkBase<TBenchmarkType>
	{
		public virtual TBenchmarkType? DataType { get; set; }
		public virtual TBenchmarkType? BaseDataType { get; set; }

		private Action? _testMethod;
		protected Action TestMethod
		{
			get => _testMethod ?? throw new OperationCanceledException("Skip - No actual defined");
			set => _testMethod = value;
		}

		private Action? _baselineMethod;
		protected Action BaselineMethod
		{
			get => _baselineMethod ?? throw new OperationCanceledException("Skip - No baseline defined");
			set => _baselineMethod = value;
		}

		[Benchmark(Baseline = true)]
		public void Baseline() => BaselineMethod.Invoke();

		[Benchmark]
		public void Actual() => TestMethod.Invoke();

		// 0 2 4 8 16 32 64 128 256 512 1024 2048 4096 8192 16384 32768 65536 131072 262144 524_288
		// 1048576 2097152 4194304 8388608 16777216 33554432 67108864 134217728 268435456 536870912
		// 1073741824 2147483648

		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 1_000_000)]
		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 1_000_000)]
		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 100_000_000)]
		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 850_000 - 1, 1_000_000, RecyclableDefaults.MaxPooledBlockSize)]
		// [Params(1_000_000, 10_000_000, 100_000_000, 1_000_000_000)]
		// [Params(192, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144)]
		// [Params(524_288, 786_432, 1_048_576)]
		// [Params(262_144, 393_216, 524_288)]
		//[Params(850_000)]
		// [Params(RecyclableDefaults.MaxPooledBlockSize)]
		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576, 10_000_000, 33_554_432, 50_000_000, 100_000_000)]
		[Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072)]
		//[Params(RecyclableDefaults.MinItemsCountForParallelization, 1_048_576, 2_097_152, 4_194_304, 8_388_608, 16_777_216, 33_554_432, RecyclableDefaults.MaxPooledBlockSize)]
		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128)]
		// [Params(RecyclableDefaults.MinItemsCountForParallelization * 3)]
		// [Params(1_048_576)]
		public virtual int TestObjectCount { get; set; } = 1; //33_554_432; //524_288; //131_072;
		public virtual int TestObjectCountForSlowMethods => (TestObjectCount / 2) + 1;// Math.Min(TestObjectCount, 131_072);

		//[Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200)]
		//[Params(10, 20, 50, 100)]
		//[Params(1, 10)]
		public int Divider = 10;
		//public int BlockSize = 1_048_576;//int.MaxValue - 1;
		public int BlockSize => TestObjectCount switch
		{
			0 => 1,
			> 0 and <= 10_240 => TestObjectCount,
			_ => TestObjectCount / Divider > 0 ? TestObjectCount / Divider : TestObjectCount
		};

		public byte BlockSizePow2BitShift => checked((byte)(31 - BitOperations.LeadingZeroCount((uint)BlockSize)));

		[MethodImpl(MethodImplOptions.NoInlining)]
		protected static void DoNothing<T>(in T item)
		{
			_ = item;
		}

		protected static Exception CreateUnknownBenchmarkTypeException<T>(T benchmarkType)
		{
			return new InvalidOperationException($"******* UNKNOWN BENCHMARK TYPE {{{benchmarkType}}} *******");
		}

		protected static Exception CreateMethodNotFoundException(in string methodName, in string? className)
		{
			return new MethodAccessException($"******* METHOD {{{methodName}}} NOT FOUND IN CLASS {{{className}}} *******");
		}

		protected virtual void PrepareData<T>(T benchmarkType) { }
		protected virtual Action? GetTestMethod(TBenchmarkType? benchmarkType) => null;

		[GlobalCleanup]
		public virtual void Cleanup()
		{
			_baselineMethod = null;
			_testMethod = null;
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
		}

		[GlobalSetup]
		public virtual void Setup()
		{
			Console.WriteLine($"******* SETTING UP TEST CASE FOR BENCHMARK {{{DataType}}} *******");

			if (!EqualityComparer<TBenchmarkType>.Default.Equals(BaseDataType, default))
			{
				Console.WriteLine("******* SETTING UP BASELINE DATA *******");
				PrepareData(BaseDataType);
				_baselineMethod = GetTestMethod(BaseDataType);
			}

			//~ We don't want to prepare data if baseline data is the same is benchmark data.
			//~ One would override another - waste of time & resources.
			if (!EqualityComparer<TBenchmarkType>.Default.Equals(BaseDataType, DataType))
			{
				Console.WriteLine("******* SETTING UP TEST CASE DATA *******");
				//~ If BaseDataType == null, then we'll come here only if DataType != null.
				PrepareData(DataType!);
				_testMethod = GetTestMethod(DataType);
			}

			Console.WriteLine("******* DATA PREPARED *******");
		}
	}
}

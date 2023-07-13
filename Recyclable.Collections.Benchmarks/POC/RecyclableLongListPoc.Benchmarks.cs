using System.Reflection;
using BenchmarkDotNet.Attributes;
using Collections.Pooled;
using Recyclable.Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
    public enum RecyclableLongListPocBenchmarkType
	{
		EnsureCapacity_ByPowOf2,
		EnsureCapacityV1_ByPowOf2,
		EnsureCapacityV3_ByPowOf2,
	}

	[MemoryDiagnoser]
	public partial class RecyclableLongListPocBenchmarks : PocBenchmarkBase<RecyclableLongListPocBenchmarkType>
	{
		[Params(RecyclableLongListPocBenchmarkType.EnsureCapacity_ByPowOf2)]
		public override RecyclableLongListPocBenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

		// [Params(BenchmarkType.Array, BenchmarkType.List, BenchmarkType.RecyclableList, BenchmarkType.RecyclableLongList)]
		[Params(RecyclableLongListPocBenchmarkType.EnsureCapacityV1_ByPowOf2)]
		public override RecyclableLongListPocBenchmarkType BaseDataType { get => base.BaseDataType; set => base.BaseDataType = value; }

        private long[]? _testArray;
		public long[] TestObjects => _testArray ?? throw new NullReferenceException("Something is wrong and test objects are null");

		public RecyclableLongList<long> TestObjectsAsRecyclableLongList => _testRecyclableLongList ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableLongList<long>)} is null");
		private RecyclableLongList<long>? _testRecyclableLongList;
		private PooledList<long>? _testPooledList;
		internal PooledList<long> TestPooledList => _testPooledList ?? throw new NullReferenceException($"Something is wrong and {nameof(PooledList<long>)} is null");

		private static readonly MethodInfo _ensureCapacityNewFunc;
		private static readonly MethodInfo _ensureCapacityV1Func;
		private static readonly MethodInfo _ensureCapacityV3Func;

		static RecyclableLongListPocBenchmarks()
		{
			_ensureCapacityNewFunc = typeof(RecyclableLongList<long>).GetMethod("EnsureCapacity", BindingFlags.Static | BindingFlags.NonPublic)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableLongList<long>)}");

			_ensureCapacityV1Func = typeof(RecyclableLongListV1<long>).GetMethod("EnsureCapacity", BindingFlags.Instance | BindingFlags.NonPublic)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableLongListV1<long>)}");

			_ensureCapacityV3Func = typeof(RecyclableLongListV3<long>).GetMethod("EnsureCapacity", BindingFlags.Static | BindingFlags.NonPublic)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableLongListV3<long>)}");
		}

		public static void Run()
		{
			var benchmark = new RecyclableLongListPocBenchmarks();
			benchmark.Setup();
			// benchmark.RecyclableLongList_EnsureCapacity_ByPowOf2();
			// benchmark.RecyclableLongList_EnsureCapacityV1_ByPowOf2();
			// benchmark.RecyclableLongList_EnsureCapacityV2_ByPowOf2();
			// benchmark.RecyclableLongList_EnsureCapacityV3_ByPowOf2();
			// benchmark.RecyclableLongList_EnsureCapacity_ByBlockSize();
			// benchmark.RecyclableLongList_EnsureCapacityV1_ByBlockSize();
			// benchmark.RecyclableLongList_EnsureCapacityV2_ByBlockSize();
			// benchmark.RecyclableLongList_EnsureCapacityV3_ByBlockSize();
			benchmark.Cleanup();
		}

		public override void Setup()
		{
			Console.WriteLine("******* SETTING UP EXPECTED RESULTS DATA *******");
			_testArray = DataGenerator.EnumerateTestObjects(TestObjectCount);

			base.Setup();
        }

		public override void Cleanup()
		{
			_testArray = null;
			_testRecyclableLongList?.Dispose();
			_testRecyclableLongList = null;
			_testPooledList?.Dispose();
			_testPooledList = null;

			base.Cleanup();
		}

        protected override Action? GetTestMethod(RecyclableLongListPocBenchmarkType benchmarkType) => benchmarkType switch
            {
                RecyclableLongListPocBenchmarkType.EnsureCapacity_ByPowOf2 => () => RecyclableLongList_EnsureCapacity_ByPowOf2(),
                RecyclableLongListPocBenchmarkType.EnsureCapacityV1_ByPowOf2 => () => RecyclableLongList_EnsureCapacityV1_ByPowOf2(),
                RecyclableLongListPocBenchmarkType.EnsureCapacityV3_ByPowOf2 => () => RecyclableLongList_EnsureCapacityV3_ByPowOf2(),
                _ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
            };

        protected override void PrepareData<T>(T benchmarkType)
        {
			base.PrepareData(benchmarkType);
        }
    }
}

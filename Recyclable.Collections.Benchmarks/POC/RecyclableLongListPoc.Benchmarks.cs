using System.Reflection;
using BenchmarkDotNet.Attributes;
using Collections.Pooled;
using Recyclable.Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
    public enum RecyclableLongListPocBenchmarkType
	{
		IndexOfV1,
		IndexOfV2,
		IndexOfV3,
		IndexOfV4,
		IndexOf_PooledList,
		IndexOf,
		EnsureCapacity_ByPowOf2,
		EnsureCapacityV1_ByPowOf2,
		EnsureCapacityV2_ByPowOf2,
		EnsureCapacityV3_ByPowOf2,
	}

	[MemoryDiagnoser]
	public partial class RecyclableLongListPocBenchmarks : PocBenchmarkBase<RecyclableLongListPocBenchmarkType>
	{
		[Params(RecyclableLongListPocBenchmarkType.IndexOf)]
		public override RecyclableLongListPocBenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

		// [Params(BenchmarkType.Array, BenchmarkType.List, BenchmarkType.RecyclableList, BenchmarkType.RecyclableLongList)]
		[Params(RecyclableLongListPocBenchmarkType.IndexOf_PooledList)]
		public override RecyclableLongListPocBenchmarkType BaseDataType { get => base.BaseDataType; set => base.BaseDataType = value; }

        private long[]? _testArray;
		public long[] TestObjects => _testArray ?? throw new NullReferenceException("Something is wrong and test objects are null");

		public RecyclableLongList<long> TestObjectsAsRecyclableLongList => _testRecyclableLongList ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableLongList<long>)} is null");
		private RecyclableLongListIndexOfV1<long>? _testRecyclableLongListIndexOfV1;
		internal RecyclableLongListIndexOfV1<long> TestObjectsAsRecyclableLongListIndexOfV1 => _testRecyclableLongListIndexOfV1 ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableLongListIndexOfV1<long>)} is null");
		private RecyclableLongListIndexOfV2<long>? _testRecyclableLongListIndexOfV2;
		private RecyclableLongList<long>? _testRecyclableLongList;
		internal RecyclableLongListIndexOfV2<long> TestObjectsAsRecyclableLongListIndexOfV2 => _testRecyclableLongListIndexOfV2 ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableLongListIndexOfV2<long>)} is null");
		private RecyclableLongListIndexOfV3<long>? _testRecyclableLongListIndexOfV3;
		internal RecyclableLongListIndexOfV3<long> TestObjectsAsRecyclableLongListIndexOfV3 => _testRecyclableLongListIndexOfV3 ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableLongListIndexOfV3<long>)} is null");
		private RecyclableLongListIndexOfV4<long>? _testRecyclableLongListIndexOfV4;
		internal RecyclableLongListIndexOfV4<long> TestObjectsAsRecyclableLongListIndexOfV4 => _testRecyclableLongListIndexOfV4 ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableLongListIndexOfV4<long>)} is null");
		private PooledList<long>? _testPooledList;
		internal PooledList<long> TestPooledList => _testPooledList ?? throw new NullReferenceException($"Something is wrong and {nameof(PooledList<long>)} is null");

		private static readonly MethodInfo _ensureCapacityNewFunc;
		private static readonly MethodInfo _ensureCapacityV1Func;
		private static readonly MethodInfo _ensureCapacityV2Func;
		private static readonly MethodInfo _ensureCapacityV3Func;

		static RecyclableLongListPocBenchmarks()
		{
			_ensureCapacityNewFunc = typeof(RecyclableLongList<long>).GetMethod("EnsureCapacity", BindingFlags.Static | BindingFlags.NonPublic)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableLongList<long>)}");

			_ensureCapacityV1Func = typeof(RecyclableLongListV1<long>).GetMethod("EnsureCapacity", BindingFlags.Instance | BindingFlags.NonPublic)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableLongListV1<long>)}");

			_ensureCapacityV2Func = typeof(RecyclableLongListV2<long>).GetMethod("EnsureCapacity", BindingFlags.Static | BindingFlags.Public)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableLongListV2<long>)}");

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
			benchmark.RecyclableLongList_IndexOf();
			benchmark.RecyclableLongList_IndexOfV1();
			benchmark.RecyclableLongList_IndexOfV2();
			benchmark.RecyclableLongList_IndexOfV3();
			benchmark.RecyclableLongList_IndexOfV4();
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
			_testRecyclableLongListIndexOfV1?.Dispose();
			_testRecyclableLongListIndexOfV1 = null;
			_testRecyclableLongListIndexOfV2?.Dispose();
			_testRecyclableLongListIndexOfV2 = null;
			_testRecyclableLongListIndexOfV3?.Dispose();
			_testRecyclableLongListIndexOfV3 = null;
			_testRecyclableLongListIndexOfV4?.Dispose();
			_testRecyclableLongListIndexOfV4 = null;
			_testPooledList?.Dispose();
			_testPooledList = null;

			base.Cleanup();
		}

        protected override Action? GetTestMethod(RecyclableLongListPocBenchmarkType benchmarkType) => benchmarkType switch
            {
                RecyclableLongListPocBenchmarkType.IndexOfV1 => () => RecyclableLongList_IndexOfV1(),
                RecyclableLongListPocBenchmarkType.IndexOfV2 => () => RecyclableLongList_IndexOfV2(),
                RecyclableLongListPocBenchmarkType.IndexOfV3 => () => RecyclableLongList_IndexOfV3(),
                RecyclableLongListPocBenchmarkType.IndexOfV4 => () => RecyclableLongList_IndexOfV4(),
                RecyclableLongListPocBenchmarkType.IndexOf_PooledList => () => PooledList_IndexOf(),
                RecyclableLongListPocBenchmarkType.IndexOf => () => RecyclableLongList_IndexOf(),
                RecyclableLongListPocBenchmarkType.EnsureCapacity_ByPowOf2 => () => RecyclableLongList_EnsureCapacity_ByPowOf2(),
                RecyclableLongListPocBenchmarkType.EnsureCapacityV1_ByPowOf2 => () => RecyclableLongList_EnsureCapacityV1_ByPowOf2(),
                RecyclableLongListPocBenchmarkType.EnsureCapacityV2_ByPowOf2 => () => RecyclableLongList_EnsureCapacityV2_ByPowOf2(),
                RecyclableLongListPocBenchmarkType.EnsureCapacityV3_ByPowOf2 => () => RecyclableLongList_EnsureCapacityV3_ByPowOf2(),
                _ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
            };

        protected override void PrepareData<T>(T benchmarkType)
        {
			base.PrepareData(benchmarkType);

            switch (benchmarkType)
            {
                case RecyclableLongListPocBenchmarkType.IndexOfV1:
					_testRecyclableLongListIndexOfV1 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
                    break;
                case RecyclableLongListPocBenchmarkType.IndexOfV2:
					_testRecyclableLongListIndexOfV2 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
                    break;
                case RecyclableLongListPocBenchmarkType.IndexOfV3:
					_testRecyclableLongListIndexOfV3 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
                    break;
                case RecyclableLongListPocBenchmarkType.IndexOfV4:
					_testRecyclableLongListIndexOfV4 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
                    break;
                case RecyclableLongListPocBenchmarkType.IndexOf_PooledList:
					_testPooledList = new PooledList<long>(TestObjects, ClearMode.Auto);
                    break;
                case RecyclableLongListPocBenchmarkType.IndexOf:
                    _testRecyclableLongList = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
                    break;
            }
        }
    }
}

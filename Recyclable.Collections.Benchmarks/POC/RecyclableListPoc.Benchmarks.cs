using System.Reflection;
using BenchmarkDotNet.Attributes;
using Collections.Pooled;
using Recyclable.Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
	public enum RecyclableListPocBenchmarkType
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

	public partial class RecyclableListPocBenchmarks : PocBenchmarkBase
	{
		[Params(BenchmarkType.Array, BenchmarkType.List, BenchmarkType.PooledList, BenchmarkType.RecyclableArrayList, BenchmarkType.RecyclableList)]
		// [Params(RecyclableListPocBenchmarkType.IndexOf)]
		public RecyclableListPocBenchmarkType _benchmarkType;
		private Action? _testMethod;

        [Benchmark]
        public void RunWhen() => _testMethod!.Invoke();

        private long[]? _testArray;
		public long[] TestObjects => _testArray ?? throw new NullReferenceException("Something is wrong and test objects are null");
		private RecyclableList<long>? _testRecyclableList;
		public RecyclableList<long> TestObjectsAsRecyclableList => _testRecyclableList ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableList<long>)} is null");
		private RecyclableListIndexOfV1<long>? _testRecyclableListIndexOfV1;
		internal RecyclableListIndexOfV1<long> TestObjectsAsRecyclableListIndexOfV1 => _testRecyclableListIndexOfV1 ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableListIndexOfV1<long>)} is null");
		private RecyclableListIndexOfV2<long>? _testRecyclableListIndexOfV2;
		internal RecyclableListIndexOfV2<long> TestObjectsAsRecyclableListIndexOfV2 => _testRecyclableListIndexOfV2 ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableListIndexOfV2<long>)} is null");
		private RecyclableListIndexOfV3<long>? _testRecyclableListIndexOfV3;
		internal RecyclableListIndexOfV3<long> TestObjectsAsRecyclableListIndexOfV3 => _testRecyclableListIndexOfV3 ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableListIndexOfV3<long>)} is null");
		private RecyclableListIndexOfV4<long>? _testRecyclableListIndexOfV4;
		internal RecyclableListIndexOfV4<long> TestObjectsAsRecyclableListIndexOfV4 => _testRecyclableListIndexOfV4 ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableListIndexOfV4<long>)} is null");
		private PooledList<long>? _testPooledList;
		internal PooledList<long> TestPooledList => _testPooledList ?? throw new NullReferenceException($"Something is wrong and {nameof(PooledList<long>)} is null");

		private static readonly MethodInfo _ensureCapacityNewFunc;
		private static readonly MethodInfo _ensureCapacityV1Func;
		private static readonly MethodInfo _ensureCapacityV2Func;
		private static readonly MethodInfo _ensureCapacityV3Func;

		static RecyclableListPocBenchmarks()
		{
			_ensureCapacityNewFunc = typeof(RecyclableList<long>).GetMethod("EnsureCapacity", BindingFlags.Static | BindingFlags.NonPublic)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableList<long>)}");

			_ensureCapacityV1Func = typeof(RecyclableListV1<long>).GetMethod("EnsureCapacity", BindingFlags.Instance | BindingFlags.NonPublic)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableListV1<long>)}");

			_ensureCapacityV2Func = typeof(RecyclableListV2<long>).GetMethod("EnsureCapacity", BindingFlags.Static | BindingFlags.Public)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableListV2<long>)}");

			_ensureCapacityV3Func = typeof(RecyclableListV3<long>).GetMethod("EnsureCapacity", BindingFlags.Static | BindingFlags.NonPublic)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableListV3<long>)}");
		}

		public static void Run()
		{
			var benchmark = new RecyclableListPocBenchmarks();
			benchmark.RecyclableList_EnsureCapacity_ByPowOf2();
			benchmark.RecyclableList_EnsureCapacityV1_ByPowOf2();
			benchmark.RecyclableList_EnsureCapacityV2_ByPowOf2();
			benchmark.RecyclableList_EnsureCapacityV3_ByPowOf2();
			benchmark.RecyclableList_EnsureCapacity_ByBlockSize();
			benchmark.RecyclableList_EnsureCapacityV1_ByBlockSize();
			benchmark.RecyclableList_EnsureCapacityV2_ByBlockSize();
			benchmark.RecyclableList_EnsureCapacityV3_ByBlockSize();
			benchmark.RecyclableList_IndexOf();
			benchmark.RecyclableList_IndexOfV1();
			benchmark.RecyclableList_IndexOfV2();
			benchmark.RecyclableList_IndexOfV3();
			benchmark.RecyclableList_IndexOfV4();
		}

		[GlobalSetup]
		public void Setup()
		{
			Console.WriteLine($"Setting up exptected results data");
			_testArray = DataGenerator.EnumerateTestObjects(TestObjectCount);
			Console.WriteLine($"Setting up test case for benchmark {{{_benchmarkType}}}");
            switch (_benchmarkType)
            {
                case RecyclableListPocBenchmarkType.IndexOfV1:
					_testRecyclableListIndexOfV1 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
					_testMethod = () => RecyclableList_IndexOfV1();
                    break;
                case RecyclableListPocBenchmarkType.IndexOfV2:
					_testRecyclableListIndexOfV2 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
					_testMethod = () => RecyclableList_IndexOfV2();
                    break;
                case RecyclableListPocBenchmarkType.IndexOfV3:
					_testRecyclableListIndexOfV3 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
					_testMethod = () => RecyclableList_IndexOfV3();
                    break;
                case RecyclableListPocBenchmarkType.IndexOfV4:
					_testRecyclableListIndexOfV4 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
					_testMethod = () => RecyclableList_IndexOfV4();
                    break;
                case RecyclableListPocBenchmarkType.IndexOf_PooledList:
					_testPooledList = new PooledList<long>(TestObjects, ClearMode.Auto);
					_testMethod = () => PooledList_IndexOf();
                    break;
                case RecyclableListPocBenchmarkType.IndexOf:
                    _testRecyclableList = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
					_testMethod = () => RecyclableList_IndexOf();
                    break;
				case RecyclableListPocBenchmarkType.EnsureCapacity_ByPowOf2:
					_testMethod = () => RecyclableList_EnsureCapacity_ByPowOf2();
					break;
				case RecyclableListPocBenchmarkType.EnsureCapacityV1_ByPowOf2:
					_testMethod = () => RecyclableList_EnsureCapacityV1_ByPowOf2();
					break;
				case RecyclableListPocBenchmarkType.EnsureCapacityV2_ByPowOf2:
					_testMethod = () => RecyclableList_EnsureCapacityV2_ByPowOf2();
					break;
				case RecyclableListPocBenchmarkType.EnsureCapacityV3_ByPowOf2:
					_testMethod = () => RecyclableList_EnsureCapacityV3_ByPowOf2();
					break;
                default:
                    throw new InvalidOperationException($"Unknown benchmark type {{{_benchmarkType}}}");
            }

			Console.WriteLine("Data prepared");
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_testArray = null;
			_testRecyclableList?.Dispose();
			_testRecyclableList = null;
			_testRecyclableListIndexOfV1?.Dispose();
			_testRecyclableListIndexOfV1 = null;
			_testRecyclableListIndexOfV2?.Dispose();
			_testRecyclableListIndexOfV2 = null;
			_testRecyclableListIndexOfV3?.Dispose();
			_testRecyclableListIndexOfV3 = null;
			_testRecyclableListIndexOfV4?.Dispose();
			_testRecyclableListIndexOfV4 = null;
			_testPooledList?.Dispose();
			_testPooledList = null;
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
		}
	}
}

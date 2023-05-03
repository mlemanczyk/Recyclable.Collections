using System.Reflection;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Collections.Pooled;
using Recyclable.Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
	public partial class RecyclableListPocBenchmarks : PocBenchmarkBase
	{
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
			_testArray = DataGenerator.EnumerateTestObjects(TestObjectCount);
			_testRecyclableList = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
			_testRecyclableListIndexOfV1 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
			_testRecyclableListIndexOfV2 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
			_testRecyclableListIndexOfV3 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
			_testRecyclableListIndexOfV4 = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
			_testPooledList = new PooledList<long>(TestObjects, ClearMode.Auto);

			// Warm up memory pools for fair comparison with 1st run
			RecyclableList_EnsureCapacity_ByPowOf2();
			RecyclableList_EnsureCapacityV1_ByPowOf2();
			RecyclableList_EnsureCapacityV2_ByPowOf2();
			RecyclableList_EnsureCapacityV3_ByPowOf2();
			RecyclableList_IndexOf();
			RecyclableList_IndexOfV1();
			RecyclableList_IndexOfV2();
			RecyclableList_IndexOfV3();
			RecyclableList_IndexOfV4();
			PooledList_IndexOf();
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

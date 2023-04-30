using System.Reflection;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using Recyclable.Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
	public partial class RecyclableListPocBenchmarks : PocBenchmarkBase
	{
		private long[]? _testArray;
		public long[] TestObjects => _testArray ?? throw new NullReferenceException("Something is wrong and test objects are null");
		private RecyclableList<long>? _testRecyclableList;
		public RecyclableList<long> TestObjectsAsRecyclableList => _testRecyclableList ?? throw new NullReferenceException("Something is wrong and RecyclableList is null");

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

			_indexOfFunc = typeof(RecyclableList<long>).GetMethod("IndexOf", BindingFlags.Public | BindingFlags.Instance)
				?? throw new NullReferenceException($"Method IndexOf not found in class {nameof(RecyclableList<long>)}");

			_indexOfV4Func = typeof(RecyclableListIndexOfV1<long>).GetMethod("IndexOf", BindingFlags.Public | BindingFlags.Instance)
				?? throw new NullReferenceException($"Method IndexOf not found in class {nameof(RecyclableListIndexOfV1<long>)}");
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
		}

		[GlobalSetup]
		public void Setup()
		{
			_testArray = DataGenerator.EnumerateTestObjects(TestObjectCount);
			_testRecyclableList = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);

			// Warm up memory pools for fair comparison with 1st run
			RecyclableList_EnsureCapacity_ByPowOf2();
			RecyclableList_EnsureCapacityV1_ByPowOf2();
			RecyclableList_EnsureCapacityV2_ByPowOf2();
			RecyclableList_EnsureCapacityV3_ByPowOf2();
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			_testArray = null;
			_testRecyclableList?.Dispose();
			_testRecyclableList = null;
		}
	}
}

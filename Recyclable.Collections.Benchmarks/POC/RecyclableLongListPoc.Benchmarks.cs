using System.Reflection;
using BenchmarkDotNet.Attributes;
using Collections.Pooled;
using Recyclable.Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
	[MemoryDiagnoser]
	public partial class RecyclableLongListPocBenchmarks<T> : PocBenchmarkBase<T>
	{
		private long[]? _testArray;
		public long[] TestObjects => _testArray ?? throw new NullReferenceException("Something is wrong and test objects are null");

		public RecyclableLongList<long> TestObjectsAsRecyclableLongList => _testRecyclableLongList ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableLongList<long>)} is null");
		private RecyclableLongList<long>? _testRecyclableLongList;
		private PooledList<long>? _testPooledList;
		internal PooledList<long> TestPooledList => _testPooledList ?? throw new NullReferenceException($"Something is wrong and {nameof(PooledList<long>)} is null");

		private static readonly MethodInfo _ensureCapacityNewFunc;

		static RecyclableLongListPocBenchmarks()
		{
			_ensureCapacityNewFunc = typeof(RecyclableLongList<long>).GetMethod("EnsureCapacity", BindingFlags.Static | BindingFlags.NonPublic)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableLongList<long>)}");

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
	}
}

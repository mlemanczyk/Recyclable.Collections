using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	[MemoryDiagnoser]
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		private long[] _testArray;
		private RecyclableArrayList<long> _testRecyclableArrayList;
		private RecyclableList<long> _testRecyclableList;
		private PooledList<long> _testPooledList;

		public PooledList<long> TestObjectsAsPooledList => _testPooledList ?? throw new NullReferenceException("Something is wrong and the field is not initialized");

		[GlobalSetup]
		public void Setup()
		{
			Console.WriteLine("******* GLOBAL SETUP RAISED *******");
			_testObjects = EnumerateTestObjects().ToArray();
			_testRecyclableArrayList = new(TestObjects, initialCapacity: TestObjectCount);
			_testRecyclableList = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
			_testArray = TestObjects.ToArray();
			_testObjectsAsList = TestObjects.ToList();
			_testObjectsAsRecyclableArrayList = TestObjects.ToRecyclableArrayList();
			_testObjectsAsRecyclableList = TestObjects.ToRecyclableList();
			_testPooledList = new PooledList<long>(TestObjects, ClearMode.Always);
		}

		private IEnumerable<long> EnumerateTestObjects()
		{
			for (long i = 0; i < TestObjectCount; i++)
			{
				yield return i;
			}
		}

		[GlobalCleanup]
		public void Cleanup()
		{
			Console.WriteLine("******* GLOBAL CLEANUP *******");
			_testRecyclableArrayList?.Dispose();
			_testRecyclableList?.Dispose();
			_testPooledList?.Dispose();
			TestObjectsAsRecyclableArrayList?.Dispose();
			_testObjectsAsRecyclableArrayList = default;
			_testObjectsAsRecyclableList = default;
			_testObjectsAsList = default;
			_testObjects = default;
		}
	}
}

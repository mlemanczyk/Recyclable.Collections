using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks
	{
		private object[] _testArray;
		private RecyclableArrayList<object> _testRecyclableArrayList;
		private RecyclableList<object> _testRecyclableList;
		private PooledList<object> _testPooledList;

		public PooledList<object> TestObjectsAsPooledList => _testPooledList ?? throw new NullReferenceException("Something is wrong and the field is not initialized");

		[GlobalSetup]
		public void Setup()
		{
			Console.WriteLine("******* GLOBAL SETUP RAISED *******");
			_testObjects = EnumerateTestObjects().ToArray();
			_testRecyclableArrayList = new(TestObjects, initialCapacity: checked((int)TestObjectCount));
			_testRecyclableList = new(TestObjects, BlockSize, expectedItemsCount: checked((int)TestObjectCount));
			_testArray = TestObjects.ToArray();
			_testObjectsAsList = TestObjects.ToList();
			_testObjectsAsRecyclableArrayList = TestObjects.ToRecyclableArrayList();
			_testObjectsAsRecyclableList = TestObjects.ToRecyclableList();
			_testPooledList = new PooledList<object>(TestObjects);
		}

		private IEnumerable<object> EnumerateTestObjects()
		{
			for (long i = 0; i < TestObjectCount; i++)
			{
				yield return new object();
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

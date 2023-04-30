using BenchmarkDotNet.Attributes;
using Collections.Pooled;

namespace Recyclable.Collections.Benchmarks
{
	[MemoryDiagnoser]
	public partial class RecyclableCollectionsBenchmarks : BenchmarkBase
	{
		private long[]? _testArray;
		private RecyclableArrayList<long>? _testRecyclableArrayList;
		private RecyclableList<long>? _testRecyclableList;
		private PooledList<long>? _testPooledList;

		public PooledList<long> TestObjectsAsPooledList => _testPooledList ?? throw new NullReferenceException("Something is wrong and the field is not initialized");

		public override void Setup()
		{
			base.Setup();
			_testRecyclableArrayList = new(TestObjects, initialCapacity: TestObjectCount);
			_testRecyclableList = new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
			_testArray = TestObjects.ToArray();
			_testObjectsAsList = TestObjects.ToList();
			_testObjectsAsRecyclableArrayList = TestObjects.ToRecyclableArrayList();
			_testObjectsAsRecyclableList = TestObjects.ToRecyclableList();
			_testPooledList = new PooledList<long>(TestObjects, ClearMode.Always);
		}

		public override void Cleanup()
		{
			_testRecyclableArrayList?.Dispose();
			_testRecyclableList?.Dispose();
			_testPooledList?.Dispose();
			TestObjectsAsRecyclableArrayList?.Dispose();
			_testObjectsAsRecyclableArrayList = default;
			_testObjectsAsRecyclableList = default;
			_testObjectsAsList = default;
			base.Cleanup();
		}
	}
}

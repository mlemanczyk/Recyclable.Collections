using BenchmarkDotNet.Attributes;
using Collections.Pooled;
using Recyclable.Collections.Benchmarks.Core;
using Recyclable.Collections.Benchmarks.POC;

namespace Recyclable.Collections.Benchmarks
{
	public enum RecyclableCollectionsBenchmarkSource
	{
		Unknown,
		Array,
		List,
		PooledList,
		RecyclableList,
		RecyclableLongList,
	}

	[MemoryDiagnoser]
	public class RecyclableBenchmarkBase<TBenchmarkType> : PocBenchmarkBase<TBenchmarkType>
	{
		protected long[]? _testObjects;
		protected long[] TestObjects => _testObjects ?? throw new NullReferenceException("Something is wrong and the field is not initialized");

		protected List<long>? _testObjectsAsList;
		protected List<long> TestObjectsAsList => _testObjectsAsList ?? throw new NullReferenceException("Something went wront and the field is not initialized");

		protected PooledList<long>? _testObjectsAsPooledList;
		public PooledList<long> TestObjectsAsPooledList => _testObjectsAsPooledList ?? throw new NullReferenceException("Something is wrong and the field is not initialized");
		protected RecyclableList<long>? _testObjectsAsRecyclableList;
		protected RecyclableList<long> TestObjectsAsRecyclableList => _testObjectsAsRecyclableList ?? throw new NullReferenceException("Something is wrong and the field is not initialized");
		protected RecyclableLongList<long>? _testObjectsAsRecyclableLongList;
		protected RecyclableLongList<long> TestObjectsAsRecyclableLongList => _testObjectsAsRecyclableLongList ?? throw new NullReferenceException("Something is wrong and the field is not initialized");
		protected IEnumerable<long> TestObjectsAsIEnumerable
		{
			get
			{
				for (long i = 0; i < TestObjectCount; i++)
				{
					yield return i;
				}
			}
		}

		public override void Setup()
		{
			PrepareData(RecyclableCollectionsBenchmarkSource.Array);
			base.Setup();
		}

		protected override void PrepareData<T>(T benchmarkType)
		{
			Console.WriteLine($"******* PREPARING DATA FOR {benchmarkType} *******");
			switch (benchmarkType)
			{
				case RecyclableCollectionsBenchmarkSource.Array:
					_testObjects ??= DataGenerator.EnumerateTestObjects(TestObjectCount);
					break;

				case RecyclableCollectionsBenchmarkSource.List:
					_testObjectsAsList ??= TestObjects.ToList();
					break;

				case RecyclableCollectionsBenchmarkSource.PooledList:
					_testObjectsAsPooledList ??= new PooledList<long>(TestObjects, ClearMode.Auto);
					break;

				case RecyclableCollectionsBenchmarkSource.RecyclableList:
					_testObjectsAsRecyclableList ??= new(TestObjects, initialCapacity: TestObjectCount);
					break;

				case RecyclableCollectionsBenchmarkSource.RecyclableLongList:
					_testObjectsAsRecyclableLongList ??= new(TestObjects, BlockSize, expectedItemsCount: TestObjectCount);
					break;

				default:
					throw CreateUnknownBenchmarkTypeException(benchmarkType);
			}

			base.PrepareData(benchmarkType);
		}

		public override void Cleanup()
		{
			Console.WriteLine("******* GLOBAL CLEANUP *******");
			// Enable this if item type is changed to reference type
			//if (_testObjects != null)
			//{
			//	Array.Fill(_testObjects, 0);
			//}

			_testObjects = default;
			_testObjectsAsList = default;
			_testObjectsAsPooledList?.Dispose();
			_testObjectsAsPooledList = default;
			_testObjectsAsRecyclableList?.Dispose();
			_testObjectsAsRecyclableList = default;
			_testObjectsAsRecyclableLongList?.Dispose();
			_testObjectsAsRecyclableLongList = default;
			base.Cleanup();
		}
	}

	//[MemoryDiagnoser]
	//public class ListVsLinkedListBenchmarks
	//{
	//	[Benchmark]
	//	public void RecyclableList()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		using var list = new RecyclableList<object>(initialCapacity: dataCount);
	//		for (long i = 0; i < dataCount; i++)
	//		{
	//			list.Add(data[i]);
	//		}

	//		for (long i = 0; i < list.LongCount; i++)
	//		{
	//			DoNothing(list[i]);
	//		}

	//		list.Clear();
	//	}

	//	[Benchmark(Baseline = true)]
	//	public void RecyclableLongList()
	//	{
	//		var data = TestObjects;
	//		long dataCount = TestObjectCount;
	//		using var list = new RecyclableLongList<object>(BlockSize, totalItemsCount: dataCount);
	//		for (long i = 0; i < dataCount; i++)
	//		{
	//			list.Add(data[i]);
	//		}

	//		for (long i = 0; i < list.LongCount; i++)
	//		{
	//			DoNothing(list[i]);
	//		}

	//		list.Clear();
	//	}

	//	//[Benchmark]
	//	public void List()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		var list = new List<object>(capacity: dataCount);
	//		for (var i = 0; i < dataCount; i++)
	//		{
	//			list.Add(data[i]);
	//		}

	//		for (var i = 0; i < list.Count; i++)
	//		{
	//			DoNothing(list[i]);
	//		}

	//		list.Clear();
	//	}

	//	//[Benchmark]
	//	public void LinkedList()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		var list = new LinkedList<object>();
	//		for (var i = 0; i < dataCount; i++)
	//		{
	//			_ = list.AddLast(data[i]);
	//		}

	//		foreach (var node in list)
	//		{
	//			DoNothing(node);
	//		}

	//		list.Clear();
	//	}

	//	//[Benchmark]
	//	public void RecyclableQueue()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		var list = new RecyclableQueue<object>(BlockSize);
	//		for (var i = 0; i < dataCount; i++)
	//		{
	//			list.Enqueue(data[i]);
	//		}

	//		while (list.LongCount > 0)
	//		{
	//			DoNothing(list.Dequeue());
	//		}

	//		list.Clear();
	//	}

	//	//[Benchmark]
	//	public void Queue()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		var list = new Queue<object>(capacity: dataCount);
	//		for (var i = 0; i < dataCount; i++)
	//		{
	//			list.Enqueue(data[i]);
	//		}

	//		while (list.Count > 0)
	//		{
	//			DoNothing(list.Dequeue());
	//		}

	//		list.Clear();
	//	}

	//	//[Benchmark]
	//	public void PriorityQueue()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		var list = new PriorityQueue<object, long>(initialCapacity: dataCount);
	//		for (var i = 0; i < dataCount; i++)
	//		{
	//			list.Enqueue(data[i], i);
	//		}

	//		while (list.Count > 0)
	//		{
	//			DoNothing(list.Dequeue());
	//		}

	//		list.Clear();
	//	}

	//	//[Benchmark]
	//	public void RecyclableSortedList_InUpdateMode()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		var list = new RecyclableSortedList<long, object>(BlockSize);
	//		list.BeginUpdate();
	//		for (var i = 0; i < dataCount; i++)
	//		{
	//			list.Add(i, data[i]);
	//		}

	//		list.EndUpdate();

	//		for (var i = 0; i < list.LongCount; i++)
	//		{
	//			DoNothing(list[i]);
	//		}

	//		list.Clear();
	//	}

	//	//[Benchmark]
	//	public void RecyclableSortedList()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		var list = new RecyclableSortedList<long, object>(BlockSize);
	//		for (var i = 0; i < dataCount; i++)
	//		{
	//			list.Add(i, data[i]);
	//		}

	//		for (var i = 0; i < list.LongCount; i++)
	//		{
	//			DoNothing(list[i]);
	//		}

	//		list.Clear();
	//	}

	//	//[Benchmark]
	//	public void SortedList()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		var list = new SortedList<long, object>();
	//		for (var i = 0; i < dataCount; i++)
	//		{
	//			list.Add(i, data[i]);
	//		}

	//		for (var i = 0; i < list.Count; i++)
	//		{
	//			DoNothing(list[i]);
	//		}

	//		list.Clear();
	//	}

	//	//[Benchmark]
	//	public void RecyclableStack()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		var list = new RecyclableStack<object>(BlockSize);
	//		for (var i = 0; i < dataCount; i++)
	//		{
	//			list.Push(data[i]);
	//		}

	//		while (list.LongCount > 0)
	//		{
	//			DoNothing(list.Pop());
	//		}

	//		list.Clear();
	//	}

	//	//[Benchmark]
	//	public void Stack()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		var list = new Stack<object>();
	//		for (var i = 0; i < dataCount; i++)
	//		{
	//			list.Push(data[i]);
	//		}

	//		while (list.Count > 0)
	//		{
	//			DoNothing(list.Pop());
	//		}

	//		list.Clear();
	//	}
	//}
}

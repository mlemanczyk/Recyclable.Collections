namespace Recyclable.Collections.Benchmarks
{
	public class BenchmarkBase
	{
		//[Params(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 128, 256, 512, 1024, 2048, 4096, 8192, 1_000_000)]
		public const int TestObjectCount = 1_000_000;
		public static int BlockSize = TestObjectCount switch
		{
			0 => 1,
			> 0 and <= 10 => TestObjectCount,
			_ => TestObjectCount / 10
		};

		protected object[]? _testObjects;
		protected object[] TestObjects => _testObjects ??= Enumerable.Range(1, TestObjectCount).Select(x => new object()).ToArray();

		private List<object>? _testObjectsAsList;
		protected List<object> TestObjectsAsList => _testObjectsAsList ??= TestObjects.ToList();

		private RecyclableArrayList<object>? _testObjectsAsRecyclableArrayList;
		protected RecyclableArrayList<object> TestObjectsAsRecyclableArrayList => _testObjectsAsRecyclableArrayList ??= TestObjects.ToRecyclableArrayList();

		protected static void DoNothing<T>(T obj)
		{
			_ = obj;
		}

		public BenchmarkBase()
		{
			_ = TestObjects;
		}
	}

	//[MemoryDiagnoser]
	//public class ListVsLinkedListBenchmarks
	//{
	//	[Benchmark]
	//	public void RecyclableArrayList()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		using var list = new RecyclableArrayList<object>(initialCapacity: dataCount);
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
	//	public void RecyclableList()
	//	{
	//		var data = TestObjects;
	//		long dataCount = TestObjectCount;
	//		using var list = new RecyclableList<object>(BlockSize, totalItemsCount: dataCount);
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

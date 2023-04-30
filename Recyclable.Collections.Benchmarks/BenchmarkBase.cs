using Recyclable.Collections.Benchmarks.POC;

namespace Recyclable.Collections.Benchmarks
{
	public class BenchmarkBase : PocBenchmarkBase
	{
		protected long[]? _testObjects;
		protected long[] TestObjects => _testObjects ?? throw new NullReferenceException("Something is wrong and the field is not initialized");

		protected List<long>? _testObjectsAsList;
		protected List<long> TestObjectsAsList => _testObjectsAsList ?? throw new NullReferenceException("Something went wront and the field is not initialized");

		protected RecyclableArrayList<long>? _testObjectsAsRecyclableArrayList;
		protected RecyclableArrayList<long> TestObjectsAsRecyclableArrayList => _testObjectsAsRecyclableArrayList ?? throw new NullReferenceException("Something is wrong and the field is not initialized");
		protected RecyclableList<long>? _testObjectsAsRecyclableList;
		protected RecyclableList<long> TestObjectsAsRecyclableList => _testObjectsAsRecyclableList ?? throw new NullReferenceException("Something is wrong and the field is not initialized");
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

		protected static void DoNothing<T>(T obj)
		{
			_ = obj;
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

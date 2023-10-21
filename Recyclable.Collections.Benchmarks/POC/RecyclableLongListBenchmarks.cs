// Ignore Spelling: POC

using System.Reflection;
using BenchmarkDotNet.Attributes;
using Collections.Pooled;
using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks.POC
{
	[MemoryDiagnoser]
	public partial class RecyclableLongListBenchmarks<T> : BaselineVsActualBenchmarkBase<T>
	{
		private int[]? _testArray;
		public int[] TestObjects => _testArray ?? throw new NullReferenceException("Something is wrong and test objects are null");

		public RecyclableLongList<int> TestObjectsAsRecyclableLongList => _testRecyclableLongList ?? throw new NullReferenceException($"Something is wrong and {nameof(RecyclableLongList<int>)} is null");
		private RecyclableLongList<int>? _testRecyclableLongList;
		private PooledList<int>? _testPooledList;
		internal PooledList<int> TestPooledList => _testPooledList ?? throw new NullReferenceException($"Something is wrong and {nameof(PooledList<int>)} is null");

		private static readonly MethodInfo _ensureCapacityNewFunc;

		static RecyclableLongListBenchmarks()
		{
			_ensureCapacityNewFunc = typeof(RecyclableLongList<int>).GetMethod("EnsureCapacity", BindingFlags.Static | BindingFlags.NonPublic)
				?? throw new NullReferenceException($"Method EnsureCapacity not found in class {nameof(RecyclableLongList<int>)}");

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

	//[MemoryDiagnoser]
	//public class ListVsLinkedListBenchmarks
	//{
	//	[Benchmark]
	//	public void RecyclableList()
	//	{
	//		var data = TestObjects;
	//		var dataCount = TestObjectCount;
	//		using var list = new RecyclableList<object>(initialCapacity: dataCount);
	//		for (int i = 0; i < dataCount; i++)
	//		{
	//			list.Add(data[i]);
	//		}

	//		for (int i = 0; i < list.LongCount; i++)
	//		{
	//			DoNothing(list[i]);
	//		}

	//		list.Clear();
	//	}

	//	[Benchmark(Baseline = true)]
	//	public void RecyclableLongList()
	//	{
	//		var data = TestObjects;
	//		int dataCount = TestObjectCount;
	//		using var list = new RecyclableLongList<object>(BlockSize, totalItemsCount: dataCount);
	//		for (int i = 0; i < dataCount; i++)
	//		{
	//			list.Add(data[i]);
	//		}

	//		for (int i = 0; i < list.LongCount; i++)
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
	//		var list = new PriorityQueue<object, int>(initialCapacity: dataCount);
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
	//		var list = new RecyclableSortedList<int, object>(BlockSize);
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
	//		var list = new RecyclableSortedList<int, object>(BlockSize);
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
	//		var list = new SortedList<int, object>();
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

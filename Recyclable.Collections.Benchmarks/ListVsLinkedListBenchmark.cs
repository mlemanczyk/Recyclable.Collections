using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	[MemoryDiagnoser]
	public class ListVsLinkedListBenchmark
	{
		private const int _objectCount = 1_000_000;
		private const int _blockSize = 100_000;
		private static readonly object[] _testObjects = Enumerable.Range(1, _objectCount).Select(x => new object()).ToArray();

		private static void DoNothing<T>(T obj)
		{
			_ = obj;
		}

		[Benchmark]
		public void Array()
		{
			var list = new object[_testObjects.LongLength];
			for (var i = 0; i < _testObjects.Length; i++)
			{
				list[i] = _testObjects[i];
			}

			for (var i = 0; i < list.LongLength; i++)
			{
				DoNothing(list[i]);
			}
		}

		[Benchmark(Baseline = true)]
		public void RecyclableList()
		{
			using var list = new RecyclableList<object>(_blockSize);
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				list.Add(_testObjects[i]);
			}

			for (var i = 0; i < list.Count; i++)
			{
				DoNothing(_testObjects[i]);
			}

			list.Clear();
		}

		[Benchmark]
		public void List()
		{
			var list = new List<object>();
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				list.Add(_testObjects[i]);
			}

			for (var i = 0; i < list.Count; i++)
			{
				DoNothing(list[i]);
			}

			list.Clear();
		}

		//[Benchmark]
		public void LinkedList()
		{
			var list = new LinkedList<object>();
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				_ = list.AddLast(_testObjects[i]);
			}

			foreach (var node in list)
			{
				DoNothing(node);
			}

			list.Clear();
		}

		[Benchmark]
		public void RecyclableQueue()
		{
			var list = new RecyclableQueue<object>(_blockSize);
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				list.Enqueue(_testObjects[i]);
			}

			foreach (var node in list)
			{
				DoNothing(node);
			}

			list.Clear();
		}

		//[Benchmark]
		public void Queue()
		{
			var list = new Queue<object>();
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				list.Enqueue(_testObjects[i]);
			}

			foreach (var node in list)
			{
				DoNothing(node);
			}

			list.Clear();
		}

		//[Benchmark]
		public void PriorityQueue()
		{
			var list = new PriorityQueue<object, long>();
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				list.Enqueue(_testObjects[i], i);
			}

			while (list.Count > 0)
			{
				var node = list.Dequeue();
				DoNothing(node);
			}

			list.Clear();
		}

		//[Benchmark]
		public void RecyclableSortedList_InUpdateMode()
		{
			var list = new RecyclableSortedList<long, object>(_blockSize);
			list.BeginUpdate();
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				list.Add(i, _testObjects[i]);
			}

			list.EndUpdate();

			for (var i = 0; i < list.LongCount; i++)
			{
				DoNothing(_testObjects[i]);
			}

			list.Clear();
		}

		//[Benchmark]
		public void RecyclableSortedList()
		{
			var list = new RecyclableSortedList<long, object>(_blockSize);
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				list.Add(i, _testObjects[i]);
			}

			for (var i = 0; i < list.LongCount; i++)
			{
				DoNothing(_testObjects[i]);
			}

			list.Clear();
		}

		//[Benchmark]
		public void SortedList()
		{
			var list = new SortedList<long, object>();
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				list.Add(i, _testObjects[i]);
			}

			for (var i = 0; i < list.Count; i++)
			{
				DoNothing(_testObjects[i]);
			}

			list.Clear();
		}

		//[Benchmark]
		public void RecyclableStack()
		{
			var list = new RecyclableStack<object>(_blockSize);
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				list.Push(_testObjects[i]);
			}

			foreach (var node in list)
			{
				DoNothing(node);
			}

			list.Clear();
		}

		//[Benchmark]
		public void Stack()
		{
			var list = new Stack<object>();
			for (var i = 0; i < _testObjects.LongLength; i++)
			{
				list.Push(_testObjects[i]);
			}

			foreach (var node in list)
			{
				DoNothing(node);
			}

			list.Clear();
		}
	}
}

using System.Collections;

namespace Recyclable.Collections
{
	public class RecyclableStack<T> : IList<T>, IDisposable
	{
		private bool _disposedValue;
		private readonly int _blockSize;

		protected RecyclableList<T> List { get; }
		public int Count => List.Count;
		public long LongCount
		{
			get => List.LongCount;
			set => List.LongCount = value;
		}

		public bool IsReadOnly => List.IsReadOnly;

		public T this[int index] { get => this[(long)index]; set => this[(long)index] = value; }
		public T this[long index] { get => List[index]; set => List[index] = value; }

		public RecyclableStack(int blockSize = RecyclableDefaults.BlockSize)
		{
			_blockSize = blockSize;
			List = new(blockSize);
		}

		public RecyclableStack(IEnumerable<T> list, int blockSize = RecyclableDefaults.BlockSize)
		{
			_blockSize = blockSize;
			List = new(list, blockSize);
		}

		public RecyclableStack(RecyclableList<T> list, int blockSize = RecyclableDefaults.BlockSize)
		{
			_blockSize = blockSize;
			List = new(list, blockSize);
		}

		public void Push(T item)
		{
			List.Add(item);
		}

		public T Pop()
		{
			var toRemove = List[LongCount - 1];
			LongCount--;
			if ((List.Capacity * _blockSize) - LongCount == _blockSize)
			{
				List.RemoveBlock(List.BlockCount - 1);
			}

			return toRemove;
		}

		public int IndexOf(T item) => List.IndexOf(item);
		public long LongIndexOf(T item) => List.LongIndexOf(item);
		public void Insert(int index, T item) => List.Insert(index, item);
		public void RemoveAt(int index) => List.RemoveAt(index);
		public void Add(T item) => List.Add(item);
		public void Clear() => List.Clear();
		public bool Contains(T item) => List.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => List.CopyTo(array, arrayIndex);
		public bool Remove(T item) => List.Remove(item);
		public IEnumerator<T> GetEnumerator() => List.Reverse().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				Clear();
				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}

using System.Collections;

namespace Recyclable.Collections
{
	internal class RecyclableStack<T> : IList<T>, IDisposable
	{
		private static readonly bool _needsClearing = !typeof(T).IsValueType;
		private bool _disposedValue;

		protected RecyclableLongList<T> List { get; }
		public int Count => checked((int)List._longCount);
		public long LongCount
		{
			get => List._longCount;
			set => List._longCount = value;
		}
		public bool IsReadOnly => false;

		public T this[int index] { get => this[(long)index]; set => this[(long)index] = value; }
		public T this[long index] { get => List[index]; set => List[index] = value; }

		public RecyclableStack(int blockSize = RecyclableDefaults.BlockSize)
		{
			List = new(blockSize);
		}

		public RecyclableStack(IEnumerable<T> list, int blockSize = RecyclableDefaults.BlockSize)
		{
			List = new(list, blockSize);
		}

		public RecyclableStack(RecyclableLongList<T> list, int blockSize = RecyclableDefaults.BlockSize)
		{
			List = new(list, blockSize);
		}

		public void Push(T item)
		{
			List.Add(item);
		}

		public T Pop()
		{
			RecyclableLongList<T> list = List;
			var itemIndex = (int)(list.LongCount & list._blockSizeMinus1) - 1;
			var blockIndex = (int)(LongCount >> list._blockSizePow2BitShift);
			var toRemove = list._memoryBlocks[blockIndex][itemIndex];
			
			LongCount--;

			if (_needsClearing)
			{
#nullable disable
				new Span<T>(list._memoryBlocks[blockIndex])[itemIndex] = default;
#nullable restore
			}

			if (itemIndex == 0)
			{
				list.RemoveBlock(blockIndex);
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

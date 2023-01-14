using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableQueue<T> : IRecyclableOwner<T>, IList<T>, IDisposable
	{
		private static readonly ArrayPool<T> _arrayPool = ArrayPool<T>.Create();
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;

		private bool _disposedValue;
		private RecyclableArrayList<T[]> Memory { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static T Dequeue(RecyclableQueue<T> queue)
		{
			if (queue.LongCount <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(queue), "There are no items in the queue");
			}

			var toRemove = queue[0];
			queue.RemovedCount++;
			queue.LongCount--;
			if (queue.Capacity - queue.LongCount == queue.BlockSize)
			{
				queue.RemoveBlock(0);
				queue.RemovedCount -= queue.BlockSize;
			}

			return toRemove;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool TryDequeue(RecyclableQueue<T> queue, out T? item)
		{
			if (queue.LongCount > 0)
			{
				item = queue.Dequeue();
				return true;
			}

			item = default;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected static long GetAbsoluteIndex(long index, long removedCount) => index + removedCount;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected long GetRelativeIndex(long index) => index - RemovedCount;

		protected long RemovedCount { get; set; }

		public RecyclableQueue(IEnumerable<T> source, int blockSize = RecyclableDefaults.BlockSize)
		{
			BlockSize = blockSize;
			Memory = new(1);
			foreach (var item in source)
			{
				Enqueue(item);
			}
		}

		public RecyclableQueue(int blockSize = RecyclableDefaults.BlockSize)
		{
			BlockSize = blockSize;
			Memory = new(1);
		}

		public int BlockSize { get; protected set; }
		public long Capacity { get; set; }

		public int Count
		{
			get => (int)LongCount;
			set => LongCount = value;
		}

		public long LongCount { get; set; }
		public bool IsReadOnly { get; } = false;

		public T this[int index]
		{
			get => GetItem(index);
			set => SetItem(index, value);
		}

		public T this[long index]
		{
			get => GetItem(index);
			set => SetItem(index, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToArrayIndex(long index, int blockSize) => (int)(index / blockSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ToItemIndex(long index, int blockSize) => index % blockSize;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetItem(long index, T value)
		{
			long absoluteIndex = GetAbsoluteIndex(index, RemovedCount);
			int arrayIndex = ToArrayIndex(absoluteIndex, BlockSize);
			long itemIndex = ToItemIndex(absoluteIndex, BlockSize);
			Memory[arrayIndex][itemIndex] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T GetItem(long index)
		{
			long absoluteIndex = GetAbsoluteIndex(index, RemovedCount);
			int arrayIndex = ToArrayIndex(absoluteIndex, BlockSize);
			long itemIndex = ToItemIndex(absoluteIndex, BlockSize);
			return Memory[arrayIndex][itemIndex];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue(T item)
		{
			if (LongCount == Capacity)
			{
				AddBlock();
			}

			SetItem(LongCount, item);
			LongCount++;
		}

		public void AddBlock()
		{
			T[] newArray = BlockSize.RentArrayFromPool<T>(_arrayPool);
			Memory.Add(newArray);
			Capacity += BlockSize;
		}

		public T Dequeue() => Dequeue(this);

		public void Add(T item) => Enqueue(item);

		public void Clear()
		{
			try
			{
				// Remove in reversed order for performance savings
				while (Memory.Count > 0)
				{
					try
					{
						RemoveBlock(Memory.Count - 1);
					}
					catch (Exception)
					{
						// We want to try returning as many arrays, as possible, before
						// the list is cleared.
					}
				}
			}
			finally
			{
				LongCount = 0;
				RemovedCount = 0;
				Memory.Clear();
			}
		}

		public bool Contains(T item) => Memory.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => Memory.CopyTo(RemovedCount, BlockSize, (int)(LongCount % BlockSize), array, arrayIndex);
		public IEnumerable<T> Enumerate() => Memory.Enumerate(BlockSize, LongCount);
		public IEnumerator<T> GetEnumerator() => Memory.Enumerate(BlockSize, LongCount).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Memory.Enumerate(BlockSize, LongCount).GetEnumerator();
		public int IndexOf(T item) => (int)LongIndexOf(item);
		public long LongIndexOf(T item) => GetRelativeIndex(Memory.LongIndexOf(BlockSize, item, _equalityComparer));
		public void Insert(int index, T item) => throw new NotSupportedException();
		public void RemoveAt(int index) => throw new NotSupportedException();
		public bool TryDequeue(out T? item) => TryDequeue(this, out item);
		public bool Remove(T item) => TryDequeue(out var _);
		public void RemoveBlock(int index)
		{
			try
			{
				Memory[index].ReturnToPool(_arrayPool);
				Memory.RemoveAt(index);
			}
			finally
			{
				Capacity -= BlockSize;
				//LongCount -= BlockSize;
			}
		}

		protected void Dispose(bool disposing)
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

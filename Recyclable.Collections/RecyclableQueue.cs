using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	internal class RecyclableQueue<T> : IRecyclableOwner<T>, IList<T>, IDisposable
	{
		private static readonly ArrayPool<T> _arrayPool = ArrayPool<T>.Create();
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;

		private bool _disposedValue;
		private RecyclableList<T[]> Memory { get; }

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

		protected static readonly bool NeedsClearing = !typeof(T).IsValueType;

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
		public bool IsReadOnly { get; }

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
		private void SetItem(long index, T value)
		{
			long absoluteIndex = GetAbsoluteIndex(index, RemovedCount);
			int arrayIndex = RecyclableQueueHelpers.ToArrayIndex(absoluteIndex, BlockSize);
			long itemIndex = RecyclableQueueHelpers.ToItemIndex(absoluteIndex, BlockSize);
			Memory[arrayIndex][itemIndex] = value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T GetItem(long index)
		{
			long absoluteIndex = GetAbsoluteIndex(index, RemovedCount);
			int arrayIndex = RecyclableQueueHelpers.ToArrayIndex(absoluteIndex, BlockSize);
			long itemIndex = RecyclableQueueHelpers.ToItemIndex(absoluteIndex, BlockSize);
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
			T[] newArray = (BlockSize >= RecyclableDefaults.MinPooledArrayLength)
				? _arrayPool.Rent(BlockSize)
				: new T[BlockSize];

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
				while (Memory._count > 0)
				{
					try
					{
						RemoveBlock(Memory._count - 1);
					}
#pragma warning disable RCS1075 // We want to try returning as many arrays, as possible, before the list is cleared.
					catch (Exception)
#pragma warning restore RCS1075
					{
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

#pragma	warning disable CS0618
		public bool Contains(T item) => RecyclableListHelpers<T>.Contains(Memory, item);
		public void CopyTo(T[] array, int arrayIndex) => RecyclableListHelpers<T>.CopyTo(Memory, RemovedCount, BlockSize, (int)(LongCount % BlockSize), array, arrayIndex);
		public IEnumerable<T> Enumerate() => RecyclableListHelpers<T>.Enumerate(Memory, BlockSize, LongCount);
		public IEnumerator<T> GetEnumerator() => RecyclableListHelpers<T>.Enumerate(Memory, BlockSize, LongCount).GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => RecyclableListHelpers<T>.Enumerate(Memory, BlockSize, LongCount).GetEnumerator();
		public int IndexOf(T item) => (int)GetRelativeIndex(RecyclableListHelpers<T>.LongIndexOf(Memory, BlockSize, item, _equalityComparer));
		public long LongIndexOf(T item) => GetRelativeIndex(RecyclableListHelpers<T>.LongIndexOf(Memory, BlockSize, item, _equalityComparer));
#pragma warning restore CS0618

		public void Insert(int index, T item) => throw new NotSupportedException();
		public void RemoveAt(int index) => throw new NotSupportedException();
		public bool TryDequeue(out T? item) => TryDequeue(this, out item);
		public bool Remove(T item) => TryDequeue(out var _);
		public void RemoveBlock(int index)
		{
			try
			{
				if (Memory[index].LongLength is > RecyclableDefaults.MinPooledArrayLength and <= int.MaxValue)
				{
					_arrayPool.Return(Memory[index], NeedsClearing);
				}

				Memory.RemoveAt(index);
			}
			finally
			{
				Capacity -= BlockSize;
				//LongCount -= BlockSize;
			}
		}

		public void Dispose()
		{
			if (!_disposedValue)
			{
				Clear();
				_disposedValue = true;
			}

			GC.SuppressFinalize(this);
		}
	}
}

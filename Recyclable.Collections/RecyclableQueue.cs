using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableQueue<T> : IRecyclableOwner<T>, IList<T>, IDisposable
	{
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;
		private static readonly IComparer<long> _comparer = Comparer<long>.Default;

		private bool _disposedValue;

		protected List<T[]> Arrays { get; }
		protected long GetAbsoluteIndex(long index) => index + RemovedCount;
		protected long GetRelativeIndex(long index) => index - RemovedCount;
		protected long RemovedCount { get; set; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static T[] RentArrayFromPool(int minSize) => ArrayPool<T>.Shared.Rent(minSize);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void ReturnToPool(T[] array) => ArrayPool<T>.Shared.Return(array);

		public RecyclableQueue(IEnumerable<T> source, int blockSize = RecyclableDefaults.BlockSize)
		{
			BlockSize = blockSize;
			Arrays = new();
			foreach (var item in source)
			{
				Enqueue(item);
			}
		}

		public RecyclableQueue(int blockSize = RecyclableDefaults.BlockSize)
		{
			BlockSize = blockSize;
			Arrays = new();
		}

		public int BlockSize { get; protected set; }
		public long Capacity { get; set; }

		public int Count
		{
			get => (int)LongCount.LimitTo(int.MaxValue, _comparer);
			set => LongCount = value;
		}

		public long LongCount { get; set; }
		public bool IsReadOnly { get; } = false;

		public T this[int index] { get => this[(long)index]; set => this[(long)index] = value; }
		public T this[long index]
		{
			get
			{
				long absoluteIndex = GetAbsoluteIndex(index);
				int arrayIndex = absoluteIndex.ToArrayIndex(BlockSize);
				long itemIndex = absoluteIndex.ToItemIndex(BlockSize);
				return Arrays[arrayIndex][itemIndex];
			}

			set
			{
				long absoluteIndex = GetAbsoluteIndex(index);
				int arrayIndex = absoluteIndex.ToArrayIndex(BlockSize);
				long itemIndex = absoluteIndex.ToItemIndex(BlockSize);
				Arrays[arrayIndex][itemIndex] = value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue(T item)
		{
			if (LongCount == Capacity)
			{
				T[] newArray = BlockSize.RentArrayFromPool<T>();
				Arrays.Add(newArray);
				Capacity += BlockSize;
			}

			var newIndex = LongCount;
			// We don't want this[newIndex] set to raise the update event, because
			// the count would be wrong. We need to increase it, first.
			this[newIndex] = item;
			LongCount++;
		}


		public T Dequeue()
		{
			var toRemove = this[0];
			RemovedCount++;
			LongCount--;
			if (Capacity - LongCount == BlockSize)
			{
				RemoveBlock(0);
				RemovedCount -= BlockSize;
			}

			return toRemove;
		}

		public void Add(T item) => Enqueue(item);

		public void Clear()
		{
			try
			{
				// Remove in reversed order for performance savings
				while (Arrays.Count > 0)
				{
					try
					{
						RemoveBlock(Arrays.Count - 1);
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
				Arrays.Clear();
			}
		}

		public bool Contains(T item) => Arrays.Contains(item);
		public void CopyTo(T[] array, int arrayIndex) => Arrays.CopyTo(RemovedCount, BlockSize, (int)(LongCount % BlockSize), array, arrayIndex);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerable<T> Enumerate() => Arrays.Enumerate(BlockSize, LongCount);
		public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public int IndexOf(T item) => (int)LongIndexOf(item).LimitTo(int.MaxValue, _comparer);
		public long LongIndexOf(T item) => GetRelativeIndex(Arrays.LongIndexOf(BlockSize, item, _equalityComparer));
		public void Insert(int index, T item) => throw new NotSupportedException();
		public void RemoveAt(int index) => throw new NotSupportedException();

		public bool TryDequeue(out T? item)
		{
			if (LongCount > 0)
			{
				item = Dequeue();
				return true;
			}

			item = default;
			return false;
		}

		public bool Remove(T item) => throw new NotSupportedException();
		public void RemoveBlock(int index)
		{
			try
			{
				ReturnToPool(Arrays[index]);
				Arrays.RemoveAt(index);
			}
			finally
			{
				Capacity -= BlockSize;
				LongCount -= BlockSize;
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

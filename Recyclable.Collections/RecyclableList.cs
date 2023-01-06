using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableList<T> : IDisposable, IList<T>
	{
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;

		private readonly List<T[]> _arrays = new();
		private bool _disposedValue;

		public RecyclableList(int blockSize = RecyclableDefaults.BlockSize)
		{
			BlockSize = blockSize;
		}

		public RecyclableList(IEnumerable<T> source, int blockSize = RecyclableDefaults.BlockSize)
		{
			BlockSize = blockSize;
			foreach (var item in source)
			{
				Add(item);
			}
		}

		public int BlockSize { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SetItem(List<T[]> arrays, in int blockSize, in long index, in T value)
			=> arrays[(int)(index / blockSize)][index % blockSize] = value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T GetItem(List<T[]> arrays, in int blockSize, in long index)
			=> arrays[(int)(index / blockSize)][index % blockSize];

		public T this[long index]
		{
			get => GetItem(_arrays, BlockSize, index);
			set => SetItem(_arrays, BlockSize, index, value);
		}

		public T this[int index]
		{
			get => GetItem(_arrays, BlockSize, index);
			set => SetItem(_arrays, BlockSize, index, value);
		}

		public long Capacity { get; set; }
		public int Count => (int)LongCount;
		public long LongCount { get; set; }
		public bool IsReadOnly { get; } = false;
		public int BlockCount => _arrays.Count;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(T item)
		{
			if (LongCount == Capacity)
			{
				T[] newArray = BlockSize.RentArrayFromPool<T>();
				_arrays.Add(newArray);
				Capacity += BlockSize;
			}

			this[LongCount] = item;
			LongCount++;
		}

		public void Clear()
		{
			try
			{
				// Remove in reversed order for performance savings
				while (_arrays.Count > 0)
				{
					try
					{
						RemoveBlock(_arrays.Count - 1);
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
				_arrays.Clear();
			}
		}

		public bool Contains(T item) => _arrays.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => _arrays.CopyTo(0, BlockSize, (int)LongCount % BlockSize, array, arrayIndex);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerator<T> GetEnumerator() => _arrays.Enumerate(BlockSize, LongCount).GetEnumerator();

		public int IndexOf(T item) => (int)_arrays.LongIndexOf(BlockSize, item, _equalityComparer);

		public void Insert(int index, T item) => throw new NotSupportedException();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public long LongIndexOf(T item) => _arrays.LongIndexOf(BlockSize, item, _equalityComparer);

		public bool Remove(T item) => throw new NotSupportedException();

		public void RemoveAt(int index) => throw new NotSupportedException();

		IEnumerator IEnumerable.GetEnumerator() => _arrays.Enumerate(BlockSize, LongCount).GetEnumerator();

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					Clear();
				}

				_disposedValue = true;
			}
		}

		// override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
		// ~RecyclableList()
		// {
		//     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		//     Dispose(disposing: false);
		// }

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public void RemoveBlock(int index)
		{
			try
			{
				_arrays[index].ReturnToPool();
				_arrays.RemoveAt(index);
			}
			finally
			{
				Capacity -= BlockSize;
				LongCount -= BlockSize;
			}
		}
	}
}

using System.Buffers;
using System.Collections;

namespace Recyclable.Collections
{
	public class RecyclableList<T> : IDisposable, IList<T>
	{
		private static readonly IEqualityComparer<T> _comparer = EqualityComparer<T>.Default;

		private readonly int _blockSize;
		private readonly List<T[]> _arrays = new();
		private bool _disposedValue;
		private long _capacity;
		protected uint _isUpdating;

		private static IEnumerable<T> EnumerateElements(List<T[]> arrays, int chunkSize, long totalCount)
		{
			long currentCount = 0;
			for (var arrayIdx = 0; arrayIdx < arrays.Count; arrayIdx++)
			{
				var array = arrays[arrayIdx];
				currentCount += chunkSize;
				switch (currentCount < totalCount)
				{
					case true:
						for (var valueIdx = 0; valueIdx < chunkSize; valueIdx++)
						{
							yield return array[valueIdx];
						}

						break;

					case false:
						var partialCount = (int)(totalCount % chunkSize);
						int maxCount = partialCount > 0 ? partialCount : chunkSize;
						for (var valueIdx = 0; valueIdx < maxCount; valueIdx++)
						{
							yield return array[valueIdx];
						}

						break;
				}
			}
		}

		private static T[] RentArray(int minSize) => ArrayPool<T>.Shared.Rent(minSize);
		private static void ReturnArray(in T[] array) => ArrayPool<T>.Shared.Return(array);

		public RecyclableList(int blockSize = RecyclableDefaults.BlockSize)
		{
			_blockSize = blockSize;
		}

		public RecyclableList(IEnumerable<T> source, int blockSize = RecyclableDefaults.BlockSize)
		{
			_blockSize = blockSize;
			BeginUpdate();
			try
			{
				foreach (var item in source)
				{
					Add(item);
				}
			}
			finally
			{
				EndUpdate();
			}
		}

		public T this[long index]
		{
			get => _arrays[(int)(index / _blockSize)][index % _blockSize];

			set
			{
				_arrays[(int)(index / _blockSize)][index % _blockSize] = value;
				if (!IsUpdating)
				{
					ListUpdated();
				}
			}
		}

		public T this[int index]
		{
			get => this[(long)index];
			set => this[(long)index] = value;
		}

		public int Count => LongCount <= int.MaxValue ? (int)LongCount : int.MaxValue;
		public long LongCount { get; protected set; }
		public bool IsReadOnly { get; } = false;
		public bool IsUpdating => _isUpdating > 0;

		public void Add(T item)
		{
			if (LongCount == _capacity)
			{
				var newArray = RentArray(_blockSize);
				_arrays.Add(newArray);
				_capacity += _blockSize;
			}

			var newIndex = LongCount;
			// We don't want this[newIndex] set to raise the update event, because
			// the count would be wrong. We need to increase it, first.
			BeginUpdate();
			try
			{
				this[newIndex] = item;
				LongCount++;
			}
			finally
			{
				EndUpdate();
			}
		}

		public void Clear()
		{
			BeginUpdate();
			try
			{
				for (var arrayIdx = 0; arrayIdx < _arrays.Count; arrayIdx++)
				{
					try
					{
						ReturnArray(_arrays[arrayIdx]);
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
				try
				{
					// Make sure the list is cleared so that we no longer use any of the arrays
					LongCount = 0;
					_arrays.Clear();
				}
				finally
				{
					EndUpdate();
				}
			}
		}

		public bool Contains(T item) => _arrays.Any(x => x.Contains(item));

		public void CopyTo(T[] array, int arrayIndex)
		{
			Span<T> arrayMemory = array.AsSpan()[arrayIndex..];
			for (var arrayIdx = 0; arrayIdx < _arrays.Count - 1; arrayIdx++)
			{
				T[] partialArray = _arrays[arrayIdx];
				ReadOnlySpan<T> sourceMemory = partialArray.AsSpan();
				sourceMemory = sourceMemory[.._blockSize];
				sourceMemory.CopyTo(arrayMemory);
				arrayMemory = arrayMemory[_blockSize..];
			}

			if (_arrays.Count > 0)
			{
				ReadOnlySpan<T> sourceMemory = _arrays[^1].AsSpan();
				var maxCount = Count % _blockSize;
				sourceMemory = maxCount > 0 
					? sourceMemory[..maxCount]
					: sourceMemory[.._blockSize];

				sourceMemory.CopyTo(arrayMemory);
			}
		}

		public IEnumerator<T> GetEnumerator() => EnumerateElements(_arrays, _blockSize, LongCount).GetEnumerator();

		public int IndexOf(T item)
		{
			for (var arrayIdx = 0; arrayIdx < _arrays.Count; arrayIdx++)
			{
				var array = _arrays[arrayIdx];
				Span<T> arrayMemory = ((T[]?)array).AsSpan();
				for (var memoryIdx = 0; memoryIdx < arrayMemory.Length; memoryIdx++)
				{
					if (_comparer.Equals(arrayMemory[memoryIdx], item))
					{
						return (arrayIdx * _blockSize) + memoryIdx;
					}
				}
			}

			return -1;
		}

		public void Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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

		protected virtual void ListUpdated()
		{
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

		public void BeginUpdate() => _isUpdating++;

		public void EndUpdate()
		{
			if (_isUpdating > 0)
			{
				_isUpdating--;
				if (_isUpdating == 0)
				{
					ListUpdated();
				}
			}
		}
	}
}

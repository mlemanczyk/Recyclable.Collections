using System.Collections;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public class RecyclableBlockList<T> : IEnumerable<T>, IDisposable
	{
		private static readonly IEqualityComparer<T> _equalityComparer = EqualityComparer<T>.Default;
		private bool _disposedValue;


		private void CopyItems(long startingIndex, long count, ref T[] destArray, long offset = 1)
		{
			for (long destItemIndex = startingIndex, sourceItemIndex = offset; destItemIndex < startingIndex + count; destItemIndex++, sourceItemIndex++)
			{
				destArray[destItemIndex] = _memory[sourceItemIndex];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static T GetItem(in T[] arrays, in long index) => arrays[index];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void InternalRemoveAt(long index)
		{
			long oldCount = LongCount;
			long oldCountMinus1 = oldCount - 1;
			if (index == oldCountMinus1)
			{
				LongCount--;
				return;
			}

			if (index >= oldCount)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			CopyItems(index, oldCountMinus1, ref _memory);

			LongCount--;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static long LongIndexOf(in T[] memory, in long itemCount, T? itemToFind, IEqualityComparer<T> equalityComparer)
		{
			for (var itemIdx = 0; itemIdx < itemCount; itemIdx++)
			{
				var item = memory[itemIdx];
				if (equalityComparer.Equals(item, itemToFind))
				{
					return itemIdx;
				}
			}

			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void SetItem(in T[] arrays, in long index, in T value) => arrays[index] = value;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T[] SetNewLength(in long newSize)
		{
			if (newSize == _capacity)
			{
				return _memory;
			}

			var source = _memory;
			var sourceLength = source.LongLength;
			var is32bit = newSize <= int.MaxValue;
			T[] newArray;
			if (is32bit)
			{
				var newSize32bit = (int)newSize;
				newArray = newSize32bit.RentArrayFromPool<T>();
				if (newSize32bit >= sourceLength)
				{
					if (sourceLength > 0)
					{
						Memory<T> sourceMemory = new(source);
						Memory<T> newArrayMemory = new(newArray);
						sourceMemory.CopyTo(newArrayMemory);
						source.ReturnToPool();
					}

					return newArray;
				}
				else
				{
					Span<T> sourceSpan = source.AsSpan();
					Span<T> newArraySpan = newArray.AsSpan();
					sourceSpan = sourceSpan[..newSize32bit];
					sourceSpan.CopyTo(newArraySpan);
				}
			}
			else
			{
				newArray = newSize.RentArrayFromPool<T>();
				if (newSize >= sourceLength)
				{
					source.CopyTo(newArray, 0);
				}
				else
				{
					CopyItems(0, newArray.LongLength, ref newArray, 0);
				}
			}

			source.ReturnToPool();
			return newArray;
		}

		protected T[] _memory = Array.Empty<T>();

		public RecyclableBlockList(long initialCapacity = RecyclableDefaults.Capacity)
		{
			_memory = SetNewLength(initialCapacity);
			_capacity = initialCapacity;
		}

		public RecyclableBlockList(IEnumerable<T> source, int initialCapacity = RecyclableDefaults.Capacity)
		{
			_memory = SetNewLength(initialCapacity);
			_capacity = initialCapacity;

			foreach (var item in source)
			{
				Add(item);
			}
		}

		public T this[long index]
		{
			get => _memory[index];
			set => _memory[index] = value;
		}

		public T this[int index]
		{
			get => _memory[index];
			set => _memory[index] = value;
		}

		private long _capacity;
		public long Capacity 
		{
			get => _capacity;
			set
			{
				if (_capacity != value)
				{
					_memory = SetNewLength(value);
					_capacity = value;
				}
			}
		}
		public int Count => (int)LongCount;
		public long LongCount { get; set; }
		public bool IsReadOnly { get; } = false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(in T item)
		{
			//EnsureCapacity(LongCount + 1);
			long requestedCapacity = LongCount + 1;
			if (_capacity < requestedCapacity)
			{
				long newCapacity;
				switch (_capacity > 0)
				{
					case true:
						newCapacity = _capacity * 2;
						while (newCapacity < requestedCapacity)
						{
							newCapacity *= 2;
						}

						break;

					case false:
						newCapacity = requestedCapacity;
						break;
				}

				_memory = SetNewLength(newCapacity);
				_capacity = newCapacity;
			}

			_memory[LongCount] = item;
			LongCount++;
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public void EnsureCapacity(long requestedCapacity)
		//{
		//	if (Capacity < requestedCapacity)
		//	{
		//		long newCapacity;
		//		switch (Capacity > 0)
		//		{
		//			case true:
		//				newCapacity = Capacity * 2;
		//				while (newCapacity < requestedCapacity)
		//				{
		//					newCapacity *= 2;
		//				}

		//				break;

		//			case false:
		//				newCapacity = requestedCapacity;
		//				break;
		//		}

		//		_memory = SetNewLength(newCapacity);
		//		_capacity = newCapacity;
		//	}
		//}

		public void Clear()
		{
			LongCount = 0;
		}

		public bool Contains(T item) => _memory.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => _memory.CopyTo(array, arrayIndex);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IEnumerable<T> Enumerate()
		{
			for (var i = 0; i < LongCount; i++)
			{
				yield return _memory[i];
			}
		}

		public int IndexOf(T item) => (int)LongIndexOf(_memory, LongCount, item, _equalityComparer);

		public void Insert(int index, T item)
		{
			long requestedCapacity = LongCount + 1;
			if (Capacity < requestedCapacity)
			{
				long newCapacity;
				switch (Capacity > 0)
				{
					case true:
						newCapacity = Capacity * 2;
						while (newCapacity < requestedCapacity)
						{
							newCapacity *= 2;
						}

						break;

					case false:
						newCapacity = requestedCapacity;
						break;
				}

				_memory = SetNewLength(newCapacity);
				_capacity = newCapacity;
			}

			for (var toMoveIdx = LongCount - 1; toMoveIdx >= index; toMoveIdx--)
			{
				_memory[toMoveIdx + 1] = _memory[toMoveIdx];
			}

			_memory[index] = item;
			LongCount++;
		}

		public long LongIndexOf(T item) => LongIndexOf(_memory, LongCount, item, _equalityComparer);

		public bool Remove(T item)
		{
			var itemIdx = LongIndexOf(item);
			if (itemIdx >= 0)
			{
				RemoveAt(itemIdx);
				return true;
			}

			return false;
		}

		public void RemoveAt(int index) => InternalRemoveAt(index);
		public void RemoveAt(long index) => InternalRemoveAt(index);

		public IEnumerator<T> GetEnumerator() => Enumerate().GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => Enumerate().GetEnumerator();

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					Clear();
					_memory.ReturnToPool();
					Capacity = 0;
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
	}
}

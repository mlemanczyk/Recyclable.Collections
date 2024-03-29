﻿using System.Collections;

namespace Recyclable.Collections
{
	public partial class RecyclableLongList<T> : IList
	{
		public bool IsFixedSize => false;
		public bool IsSynchronized => false;

		private object? _syncRoot;
		public object SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					_ = Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				}

				return _syncRoot;
			}
		}

		object? IList.this[int index]
		{
			get => _memoryBlocks[index >> _blockSizePow2BitShift][index & _blockSizeMinus1];

			set
			{
				if (value is T typedValue)
				{
					new Span<T>(_memoryBlocks[index >> _blockSizePow2BitShift])[index & _blockSizeMinus1] = typedValue;
				}
				else if (value == null && _defaultIsNull)
				{
#nullable disable
					new Span<T>(_memoryBlocks[index >> _blockSizePow2BitShift])[index & _blockSizeMinus1] = default;
#nullable restore
				}
				else
				{
					Helpers.ThrowArgumentOutOfRangeException(nameof(value), $"Argument is invalid. {(value != null ? "List element type is incompatible with the given value" : "Element is null and the list doesn't allow null values.")}");
				}
			}
		}

		public int Add(object? value)
		{
			if (value is T typeValue)
			{
				Add(typeValue);
				return checked((int)(_longCount - 1));
			}
			else if (value == null && _defaultIsNull)
			{
				Add(null);
				return checked((int)(_longCount - 1));
			}
			else
			{
				Helpers.ThrowArgumentOutOfRangeException(nameof(value), $"Argument is invalid. {(value != null ? "List element type is incompatible with the given value" : "Element is null and the list doesn't allow null values.")}");
				return RecyclableDefaults.ItemNotFoundIndex;
			}
		}

		public bool Contains(object? value) => value is T typeValue
			? Contains(typeValue)
#nullable disable
			: value == null && _defaultIsNull && Contains(default);
#nullable restore

		public int IndexOf(object? value) => value is T typeValue
			? IndexOf(typeValue)
			: value == null && _defaultIsNull
#nullable disable
			? IndexOf(default)
#nullable restore
			: RecyclableDefaults.ItemNotFoundIndex;

		public void Insert(int index, object? value)
		{
			if (value is T typeValue)
			{
				Insert(index, typeValue);
			}
			else if (value == null && _defaultIsNull)
			{
#nullable disable
				Insert(index, default);
#nullable restore
			}
			else
			{
				Helpers.ThrowArgumentOutOfRangeException(nameof(value), $"Argument is invalid. {(value != null ? "List element type is incompatible with the given value" : "Element is null and the list doesn't allow null values.")}");
			}
		}

		public void Remove(object? value)
		{
			if (value is T typeValue)
			{
				_ = Remove(typeValue);
			}
			else if (value == null && _defaultIsNull)
			{
#nullable disable
				_ = Remove(default);
#nullable restore
			}
			else
			{
				Helpers.ThrowArgumentOutOfRangeException(nameof(value), $"Argument is invalid. {(value != null ? "List element type is incompatible with the given value" : "Element is null and the list doesn't allow null values.")}");
			}
		}

		public void CopyTo(Array array, int index) => RecyclableLongList<T>.Helpers.CopyTo(_memoryBlocks, 0, _blockSize, _longCount, array, index);
	}
}

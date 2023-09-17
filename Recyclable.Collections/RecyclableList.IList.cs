using System.Collections;

namespace Recyclable.Collections
{
	public partial class RecyclableList<T> : IList
	{
		private static readonly bool _defaultIsNull = default(T) == null;

		public bool IsFixedSize => false;
		public bool IsSynchronized => false;

		[NonSerialized]
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
			get => _memoryBlock[index];

			set
			{
				if (value is T typedValue)
				{
					new Span<T>(_memoryBlock)[index] = typedValue;
				}
				else if (value == null && _defaultIsNull)
				{
#nullable disable
					new Span<T>(_memoryBlock)[index] = default;
#nullable restore
				}
				else
				{
					ThrowHelper.ThrowArgumentOutOfRangeException(nameof(value), $"Argument is invalid. {(value != null ? "List element type is incompatible with the given value" : "Element is null and the list doesn't allow null values.")}");
				}
			}
		}

		public int Add(object? value)
		{
			if (value is T typeValue)
			{
				Add(typeValue);
				return _count - 1;
			}
			else if (value == null && _defaultIsNull)
			{
				Add(null);
				return _count - 1;
			}
			else
			{
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(value), $"Argument is invalid. {(value != null ? "List element type is incompatible with the given value" : "Element is null and the list doesn't allow null values.")}");
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
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(value), $"Argument is invalid. {(value != null ? "List element type is incompatible with the given value" : "Element is null and the list doesn't allow null values.")}");
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
				ThrowHelper.ThrowArgumentOutOfRangeException(nameof(value), $"Argument is invalid. {(value != null ? "List element type is incompatible with the given value" : "Element is null and the list doesn't allow null values.")}");
			}
		}

		public void CopyTo(Array array, int index) => Array.Copy(_memoryBlock, 0, array, index, _count);
	}
}

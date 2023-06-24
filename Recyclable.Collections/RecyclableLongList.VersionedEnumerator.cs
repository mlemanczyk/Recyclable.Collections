using System.Collections;
using System.Diagnostics.Contracts;

namespace Recyclable.Collections
{

public partial class RecyclableLongList<T> : IVersionedLongList<T>
	{
		public VersionedEnumerator GetVersionedEnumerator() => new(this);
		VersionedEnumerator IVersionedLongList<T>.GetEnumerator() => new(this);

		public struct VersionedEnumerator : IEnumerator<T>
		{
#nullable disable
			private static readonly T _default = default;
#nullable restore

			private static readonly bool _isReferenceType = !typeof(T).IsValueType;

			private int _currentBlockIndex;
			private int _currentItemIndex;
			private readonly RecyclableLongList<T> _list;

#nullable disable
			private T _current;
#nullable restore
			public readonly T Current => _enumeratorVersion == _list._version ? _current : throw new InvalidOperationException();

			private readonly ulong _enumeratorVersion;

			public bool MoveNext()
			{
				var list = _list;
				if (_enumeratorVersion != list._version)
				{
					throw new InvalidOperationException();
				}

				Contract.EndContractBlock();

				if (_currentBlockIndex < list._lastBlockWithData)
				{
					_current = list._memoryBlocks[_currentBlockIndex][_currentItemIndex];
					if (_currentItemIndex + 1 < list._blockSize)
					{
						_currentItemIndex++;
					}
					else
					{
						_currentBlockIndex++;
						_currentItemIndex = 0;
					}

					return true;
				}
				// Move to the next item if we're within block boundaries, otherwise report end of collection.
				else if (list._longCount > 0 && _currentItemIndex < (list._nextItemIndex > 0 ? list._nextItemIndex : list._blockSize))
				{
					_current = list._memoryBlocks[_currentBlockIndex][_currentItemIndex];
					_currentItemIndex++;
					return true;
				}

				return false;
			}

#nullable disable
			readonly object IEnumerator.Current => _enumeratorVersion == _list._version ? _current : throw new InvalidOperationException();
#nullable restore

			public VersionedEnumerator(RecyclableLongList<T> list)
			{
				_current = _default;
				_currentBlockIndex = 0;
				_currentItemIndex = 0;
				_enumeratorVersion = list._version;
				_list = list;
			}

			public void Reset()
			{
				if (_enumeratorVersion != _list._version)
				{
					throw new InvalidOperationException();
				}

				_current = _default;
				_currentBlockIndex = 0;
				_currentItemIndex = 0;
			}

			public void Dispose()
			{
				if (_isReferenceType)
				{
					_current = _default;
				}
			}
		}
	}
}

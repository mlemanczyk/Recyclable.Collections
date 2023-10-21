using System.Collections;
using System.Diagnostics.Contracts;

namespace Recyclable.Collections
{
	public partial class RecyclableLongList<T>
	{
		public struct Enumerator : IEnumerator<T>
		{
#nullable disable
			private static readonly T _default = default;
#nullable restore

			private int _currentBlockIndex;
			private int _currentItemIndex;
			private readonly RecyclableLongList<T> _list;

#nullable disable
#pragma warning disable IDE0032 // This is intentional due to better performance
			private T _current;
#pragma warning restore IDE0032
#nullable restore

			[Pure]
			public readonly T Current => _current;

			public bool MoveNext()
			{
				var list = _list;

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
				if (list._longCount > 0 && _currentItemIndex < (list._nextItemIndex > 0 ? list._nextItemIndex : list._blockSize))
				{
					_current = list._memoryBlocks[_currentBlockIndex][_currentItemIndex];
					_currentItemIndex++;
					return true;
				}

				return false;
			}

#nullable disable
			[Pure]
			readonly object IEnumerator.Current => _current;
#nullable restore

			public Enumerator(RecyclableLongList<T> list)
			{
				_current = _default;
				_currentBlockIndex = 0;
				_currentItemIndex = 0;
				_list = list;
			}

			public void Reset()
			{
				if (_needsClearing)
				{
					_current = _default;
				}

				_currentBlockIndex = 0;
				_currentItemIndex = 0;
			}

			public void Dispose()
			{
				if (_needsClearing)
				{
					_current = _default;
				}
			}
		}
	}
}

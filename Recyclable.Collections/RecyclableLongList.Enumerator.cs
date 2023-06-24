using System.Collections;

namespace Recyclable.Collections
{
	public partial class RecyclableLongList<T>
	{
		public struct Enumerator : IEnumerator<T>
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
			public readonly T Current => _current;
			// & WAS SLOWER
			// {
			// get
			// {
			// if (_enumeratorVersion != _list._version)
			// {
			// 	throw new InvalidOperationException();
			// }

			// if (_list._longCount == 0 || _currentItemIndex == RecyclableDefaults.BlockIndexLimit)
			// {
			// 	throw new IndexOutOfRangeException();
			// }

			// Contract.EndContractBlock();

			// return _currentItemIndex > 0
			// 	? _list._memoryBlocks[_currentBlockIndex][_currentItemIndex - 1]
			// 	: _list._memoryBlocks[_currentBlockIndex - 1][_list._blockSize - 1];
			// }
			// }
			public bool MoveNext()
			{
				// & WAS SLOWER
				// if (_enumeratorVersion != _list._version)
				// {
				// 	throw new InvalidOperationException();
				// }
				//
				// Contract.EndContractBlock();
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
				else if (list._longCount > 0 && _currentItemIndex < (list._nextItemIndex > 0 ? list._nextItemIndex : list._blockSize))
				{
					_current = list._memoryBlocks[_currentBlockIndex][_currentItemIndex];
					_currentItemIndex++;
					return true;
				}

				return false;
			}

#nullable disable
			readonly object IEnumerator.Current => _current;
#nullable restore
			// & WAS SLOWER
			// readonly object? IEnumerator.Current => _enumeratorVersion != _list._version ? throw new InvalidOperationException() : _current;
			// {
			// 	get
			// 	{
			// 		// if (_enumeratorVersion != _list._version)
			// 		// {
			// 		// 	throw new InvalidOperationException();
			// 		// }

			// 		// Contract.EndContractBlock();

			// 		return _currentItemIndex > 0
			// 			? _list._memoryBlocks[_currentBlockIndex][_currentItemIndex - 1]
			// 			: _list._memoryBlocks[_currentBlockIndex - 1][_list._blockSize - 1];
			// 	}
			// }
			public Enumerator(RecyclableLongList<T> list)
			{
				_current = _default;
				_currentBlockIndex = 0;
				_currentItemIndex = 0;
				_list = list;
			}

			public void Reset()
			{
				// if (_enumeratorVersion != _list._version)
			 	// {
			 	// 	throw new InvalidOperationException();
			 	// }
				
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

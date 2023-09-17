using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Recyclable.Collections
{
	public partial class RecyclableList<T> : IRecyclableVersionedList<T>
	{
		VersionedEnumerator IRecyclableVersionedList<T>.GetEnumerator() => new(this);

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct VersionedEnumerator : IEnumerator<T>
		{
			private int _currentItemIndex;
			private readonly ulong _enumeratorVersion;
			private readonly RecyclableList<T> _list;

#nullable disable
			[Pure]
			public readonly T Current => _enumeratorVersion == _list._version ? (T)_list._memoryBlock.GetValue(_currentItemIndex - 1) : throw new InvalidOperationException();

			[Pure]
			readonly object IEnumerator.Current => _enumeratorVersion == _list._version ? _list._memoryBlock.GetValue(_currentItemIndex - 1) : throw new InvalidOperationException();
#nullable restore

			public bool MoveNext()
			{
				if (_enumeratorVersion != _list._version)
				{
					throw new InvalidOperationException();
				}

				Contract.EndContractBlock();

				if (_currentItemIndex < _list._count)
				{
					_currentItemIndex++;
					return true;
				}

				return false;
			}

			public VersionedEnumerator(RecyclableList<T> list)
			{
				_currentItemIndex = 0;
				_list = list;
				_enumeratorVersion = list._version;
			}

			[Pure]
			public readonly void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				if (_enumeratorVersion != _list._version)
				{
					throw new InvalidOperationException();
				}

				_currentItemIndex = 0;
			}
		}
	}
}

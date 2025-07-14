using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace Recyclable.Collections
{
	public partial class RecyclableList<T>
	{
                [StructLayout(LayoutKind.Sequential, Pack = 1)]
                [VersionedCollectionsGenerator.CloneForVersioned]
                public struct Enumerator : IEnumerator<T>
                {
			private static readonly bool _isReferenceType = !typeof(T).IsValueType;

			private int _currentItemIndex;
			private readonly RecyclableList<T> _list;

#nullable disable
			[Pure]
			public readonly T Current => _list._memoryBlock[_currentItemIndex - 1];
			[Pure]
			readonly object IEnumerator.Current => _list._memoryBlock[_currentItemIndex - 1];
#nullable restore

			public bool MoveNext()
			{
				if (_currentItemIndex < _list._count)
				{
					_currentItemIndex++;
					return true;
				}

				return false;
			}

			public Enumerator(RecyclableList<T> list)
			{
				_currentItemIndex = 0;
				_list = list;
			}

			[Pure]
			public readonly void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				_currentItemIndex = 0;
			}
		}
	}
}

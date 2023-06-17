using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public sealed class RecyclableCollectionVersion
	{
		private ulong _enumeratorCount;
		public ulong Version;
		public bool IsVersioned;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Inc() => _ = unchecked(Version++);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddEnumerator()
		{
			if (unchecked(_enumeratorCount++) == 0)
			{
				IsVersioned = true;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveEnumerator()
		{
			if (unchecked(--_enumeratorCount) == 0)
			{
				IsVersioned = false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RecyclableCollectionVersion Reset()
		{
			IsVersioned = false;
			Version = 0;
			_enumeratorCount = 0;
			return this;
		}
	}
}

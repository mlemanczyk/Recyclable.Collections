using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public delegate void VersionChangedEventHandler();

	public sealed class RecyclableCollectionVersion
	{
		//private ulong _enumeratorCount;

		//private ulong _version;
		//public ulong Version
		//{
		//	get => _version;
			
		//	set
		//	{
		//		_version = value;
		//		_versionChanged?.Invoke(this, EventArgs.Empty);
		//	}
		//}

		//public uint Version { get; set; }

		public bool IsVersioned;

		private VersionChangedEventHandler? _versionChanged;
		public event VersionChangedEventHandler VersionChanged
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			add 
			{ 
				_versionChanged += value;
				if (!IsVersioned)
				{
					IsVersioned = true;
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			remove
			{
				_versionChanged -= value;
				if (_versionChanged == null)
				{
					IsVersioned = false;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Inc()
		{
			if (IsVersioned)
			{
				_versionChanged!();
			}
			//_ = unchecked(Version++);
		}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public RecyclableCollectionVersion AddEnumerator()
		//{
		//	if (unchecked(_enumeratorCount++) == 0)
		//	{
		//		IsVersioned = true;
		//	}

		//	return this;
		//}

		//[MethodImpl(MethodImplOptions.AggressiveInlining)]
		//public void RemoveEnumerator()
		//{
		//	if (unchecked(--_enumeratorCount) == 0)
		//	{
		//		IsVersioned = false;
		//	}
		//}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RecyclableCollectionVersion Reset()
		{
			IsVersioned = false;
			//Version = 0;
			//_enumeratorCount = 0;
			return this;
		}
	}
}

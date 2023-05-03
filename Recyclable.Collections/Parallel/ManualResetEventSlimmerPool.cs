using Microsoft.Extensions.ObjectPool;

namespace Recyclable.Collections.Parallel
{
	public static class ManualResetEventSlimmerPool
	{
		public static readonly SpinLock _updateLock  = new(false);
		public static readonly ObjectPool<ManualResetEventSlimmer> Shared = new DefaultObjectPool<ManualResetEventSlimmer>(new DefaultPooledObjectPolicy<ManualResetEventSlimmer>());

		public static ManualResetEventSlimmer Create(bool initialState)
		{
			bool lockTaken = false;
			_updateLock.Enter(ref lockTaken);
			ManualResetEventSlimmer result = Shared.Get();
			if (lockTaken)
			{
				_updateLock.Exit(false);
			}

			result._returned = false;
			if (result._isSet != initialState)
			{
				result._isSet = initialState;
			}

			return result;
		}
	}
}

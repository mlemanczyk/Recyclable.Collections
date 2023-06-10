namespace Recyclable.Collections.Parallel
{
	public struct SpinLockSlimmer
	{
		private volatile int _locked;

		public void Enter()
		{
			if (Interlocked.CompareExchange(ref _locked, 1, 0) == 0)
			{
				return;
			}

			SpinWait waiter = new();
			while (Interlocked.CompareExchange(ref _locked, 1, 0) == 1)
			{
				waiter.SpinOnce();
			}
		}

		public void Exit()
		{
			_locked = 0;
		}
	}
}

namespace Recyclable.Collections.Parallel
{
	public sealed class ParallelSynchronizationContext : IDisposable
	{
		internal bool _isItemFound;

		public readonly Barrier AllDoneSignal;
		public long FoundItemIndex;
		public Exception? Exception;
		internal bool _returned;

		public ParallelSynchronizationContext()
		{
			AllDoneSignal = new(0);
		}

		public ParallelSynchronizationContext(int participantCount)
		{
			AllDoneSignal = new(participantCount);
		}

		public void SetItemFound()
		{
			_isItemFound = true;
		}

		public bool IsItemFound => _isItemFound;

		public void Dispose()
		{
			if (!_returned)
			{
				_returned = true;
				ParallelSynchronizationContextPool.Shared.Return(this);
			}
			else
			{
				AllDoneSignal.Dispose();
				GC.SuppressFinalize(this);
			}
		}
	}
}
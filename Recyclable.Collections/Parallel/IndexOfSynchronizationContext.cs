namespace Recyclable.Collections.Parallel
{
	public sealed class IndexOfSynchronizationContext : IDisposable
	{
		internal bool _isItemFound;

		public readonly Barrier AllDoneSignal;
		public long FoundItemIndex;
		public Exception? Exception;
		internal bool _returned;

		public IndexOfSynchronizationContext()
		{
			AllDoneSignal = new(0);
		}

		public IndexOfSynchronizationContext(int participantCount)
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
				IndexOfSynchronizationContextPool.Shared.Return(this);
			}
			else
			{
				AllDoneSignal.Dispose();
				GC.SuppressFinalize(this);
			}
		}
	}
}
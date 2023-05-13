using Microsoft.Extensions.ObjectPool;

namespace Recyclable.Collections.Parallel
{
	public static class ParallelSynchronizationContextPool
	{
		private static readonly SpinLock _updateLock = new(false);
		public static readonly ObjectPool<ParallelSynchronizationContext> Shared = new DefaultObjectPool<ParallelSynchronizationContext>(new DefaultPooledObjectPolicy<ParallelSynchronizationContext>());

		public static ParallelSynchronizationContext GetWithOneParticipant()
		{
			bool lockTaken = false;
			_updateLock.Enter(ref lockTaken);
			var context = Shared.Get();
			if (lockTaken)
			{
				_updateLock.Exit(false);
			}

			if (context.ItemFoundSignal._isSet)
			{
				context.ItemFoundSignal.Reset();
			}

			switch (context.AllDoneSignal.ParticipantsRemaining)
			{
				case > 1:
					context.AllDoneSignal.RemoveParticipants(context.AllDoneSignal.ParticipantsRemaining - 1);
					break;

				case 0:
					_ = context.AllDoneSignal.AddParticipant();
					break;
			}

			if (context.Exception != null)
			{
				context.Exception = null;
			}

			if (context.FoundItemIndex >= 0)
			{
				context.FoundItemIndex = -1L;
			}

			context._returned = false;
			return context;
		}
	}
}
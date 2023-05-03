using Microsoft.Extensions.ObjectPool;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections.Parallel
{
    public sealed class BarrierSlimmer : IDisposable
	{
		private static readonly ObjectPool<BarrierSlimmer> _defaultPool = new DefaultObjectPool<BarrierSlimmer>(new DefaultPooledObjectPolicy<BarrierSlimmer>());
		private ObjectPool<BarrierSlimmer>? _assignedPool;
		private readonly SpinLock _lock = new(false);

		public bool HasParticipants;
		public int Participants;

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void AddParticipant()
		{
			bool isLocked = false;
			_lock.Enter(ref isLocked);

			Participants++;
			if (Participants == 1)
			{
				HasParticipants = true;
			}

			if (isLocked)
			{
				_lock.Exit(false);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public void RemoveParticipant()
		{
			if (!HasParticipants)
			{
				ThrowNoMoreParticipants();
			}

			bool isLocked = false;
			_lock.Enter(ref isLocked);

			Participants--;
			if (Participants == 0)
			{
				HasParticipants = false;
			}

			if (isLocked)
			{
				_lock.Exit(false);
			}
		}

		private static void ThrowNoMoreParticipants()
		{
			throw new InvalidOperationException("There are no more participants. The operation is not allowed");
		}

		public BarrierSlimmer()
		{
			_lock = new(false);
		}

		public BarrierSlimmer(int participants)
		{
			_lock = new(false);
			Participants = participants;
			HasParticipants = participants > 0;
		}

		public static BarrierSlimmer Create(int participants)
		{
			var result = _defaultPool.Get();
			result.Participants = participants;
			result.HasParticipants = participants > 0;
			return result;
		}

		public void Dispose()
		{
			var assignedPool = _assignedPool;
			_assignedPool = null;
			assignedPool?.Return(this);

			GC.SuppressFinalize(this);
		}

		public void SignalAndWait()
		{
			RemoveParticipant();

			//if (!HasParticipants)
			//{
			//	return;
			//}

			//if (!HasParticipants)
			//{
			//	return;
			//}

			//if (!HasParticipants)
			//{
			//	return;
			//}

			// SpinWait.SpinUntil(() => HasParticipants);
			var waitSpinner = new SpinWait();
			while (HasParticipants)
			{
				waitSpinner.SpinOnce(0);
			}
		}
	}
}

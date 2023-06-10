namespace Recyclable.Collections.Parallel
{
	public sealed class IndexOfSynchronizationContext
	{
		private SpinWait _participantsSpinWait = new();
		private SpinLockSlimmer _participantsSpinLock = new();
		private SpinLockSlimmer _indexOfSpinLock = new();

		internal bool _isItemFound;
		internal int _participants;
		internal bool _returned;

		public void AddParticipant()
		{
			_participantsSpinLock.Enter();
			_participants++;
			_participantsSpinLock.Exit();
		}

		public void AddParticipants(int newParticipants)
		{
			_participantsSpinLock.Enter();
			_participants += newParticipants;
			_participantsSpinLock.Exit();
		}

		public void Lock()
		{
			_indexOfSpinLock.Enter();
		}

		public void RemoveParticipants(int participantsToRemove)
		{
			if (_participants < participantsToRemove)
			{
				ThrowNotEnoughParticipants();
			}

			_participantsSpinLock.Enter();
			_participants -= participantsToRemove;
			_participantsSpinLock.Exit();
		}

		public void RemoveParticipant()
		{
			if (_participants == 0)
			{
				ThrowNoMoreParticipants();
			}

			_participantsSpinLock.Enter();
			_participants--;
			_participantsSpinLock.Exit();
		}

		public void SignalAndWait()
		{
			if (_participants == 0)
			{
				ThrowNoMoreParticipants();
			}

			_participantsSpinLock.Enter();
			_participants--;
			_participantsSpinLock.Exit();

			while (_participants > 0)
			{
				_participantsSpinWait.SpinOnce();
			}
		}

		public void Unlock()
		{
			_indexOfSpinLock.Exit();
		}

		private static void ThrowNotEnoughParticipants() => throw new InvalidOperationException("Participant cannot be removed, because there are not enough participants.");
		private static void ThrowNoMoreParticipants() => throw new InvalidOperationException("Participant cannot be removed, because there are no more participants.");

		public long FoundBlockIndex;
		public long FoundItemIndex;
		public Exception? Exception;
		public int Participants => _participants;

		public IndexOfSynchronizationContext()
		{
		}

		public IndexOfSynchronizationContext(int participantCount)
		{
			_participants = participantCount;
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
		}
	}
}
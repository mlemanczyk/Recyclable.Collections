namespace Recyclable.Collections.Parallel
{
	public sealed class IndexOfSynchronizationContext
	{
		private readonly SpinWait _participantsSpinWait = new();
		private readonly SpinLock _participantsSpinLock = new(false);
		private readonly SpinLock _indexOfSpinLock = new(false);
		private bool _indexOfLockTaken;


		internal bool _isItemFound;
		internal int _participants;
		internal bool _returned;

		public void AddParticipant()
		{
			bool lockTaken = false;
			_participantsSpinLock.Enter(ref lockTaken);
			_participants++;
			if (lockTaken)
			{
				_participantsSpinLock.Exit(false);
			}
		}

		public void AddParticipants(int newParticipants)
		{
			bool lockTaken = false;
			_participantsSpinLock.Enter(ref lockTaken);
			_participants += newParticipants;
			if (lockTaken)
			{
				_participantsSpinLock.Exit(false);
			}
		}

		public void Lock()
		{
			_indexOfSpinLock.Enter(ref _indexOfLockTaken);
		}

		public void RemoveParticipants(int participantsToRemove)
		{
			if (_participants < participantsToRemove)
			{
				ThrowNotEnoughParticipants();
			}

			bool lockTaken = false;
			_participantsSpinLock.Enter(ref lockTaken);
			_participants -= participantsToRemove;
			if (lockTaken)
			{
				_participantsSpinLock.Exit(false);
			}
		}

		public void RemoveParticipant()
		{
			if (_participants == 0)
			{
				ThrowNoMoreParticipants();
			}

			bool lockTaken = false;
			_participantsSpinLock.Enter(ref lockTaken);
			_participants--;
			if (lockTaken)
			{
				_participantsSpinLock.Exit(false);
			}
		}

		public void SignalAndWait()
		{
			if (_participants == 0)
			{
				ThrowNoMoreParticipants();
			}

			bool lockTaken = false;
			_participantsSpinLock.Enter(ref lockTaken);
			_participants--;
			if (lockTaken)
			{
				_participantsSpinLock.Exit(false);
			}

			while (_participants > 0)
			{
				_participantsSpinWait.SpinOnce();
			}
		}

		public void Unlock()
		{
			if (_indexOfLockTaken)
			{
				_indexOfSpinLock.Exit(false);
			}
		}

		private static void ThrowNotEnoughParticipants() => throw new InvalidOperationException("Participant cannot be removed, because there are not enough participants.");
		private static void ThrowNoMoreParticipants() => throw new InvalidOperationException("Participant cannot be removed, because there are no more participants.");

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
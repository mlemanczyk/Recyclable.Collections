namespace Recyclable.Collections.Parallel
{
	public sealed class IndexOfSynchronizationContext
	{
		private readonly SpinWait _participantsSpinWait = new();

		internal bool _isItemFound;
		internal int _participants;
		internal bool _returned;

		public void AddParticipant()
		{
			_ = Interlocked.Increment(ref _participants);
		}

		public void AddParticipants(int newParticipants)
		{
			_ = Interlocked.Add(ref _participants, newParticipants);
		}

		public void RemoveParticipants(int participantsToRemove)
		{
			if (_participants < participantsToRemove)
			{
				ThrowNotEnoughParticipants();
			}

			_ = Interlocked.Add(ref _participants, -participantsToRemove);
		}

		public void RemoveParticipant()
		{
			if (_participants == 0)
			{
				ThrowNoMoreParticipants();
			}

			_ = Interlocked.Decrement(ref _participants);
		}

		public void SignalAndWait()
		{
			if (_participants == 0)
			{
				ThrowNoMoreParticipants();
			}

			_ = Interlocked.Decrement(ref _participants);
			while (_participants > 0)
			{
				_participantsSpinWait.SpinOnce();
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
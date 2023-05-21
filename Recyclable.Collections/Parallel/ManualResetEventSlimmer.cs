namespace Recyclable.Collections.Parallel
{
	public sealed class ManualResetEventSlimmer
	{
		internal bool _isSet;
		public bool IsSet => _isSet;
		internal bool _returned;

		public ManualResetEventSlimmer()
		{
		}

		public ManualResetEventSlimmer(bool initialState)
		{
			_isSet = initialState;
		}

		public void Set()
		{
			_isSet = true;
		}

		public void Reset()
		{
			_isSet = false;
		}

		public void Dispose()
		{
			if (!_returned)
			{
				_returned = true;
				ManualResetEventSlimmerPool.Shared.Return(this);
			}
		}
	}
}

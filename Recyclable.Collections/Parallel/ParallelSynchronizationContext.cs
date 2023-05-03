namespace Recyclable.Collections.Parallel
{
    public sealed class ParallelSynchronizationContext : IDisposable
    {
        public readonly ManualResetEventSlimmer ItemFoundSignal;
        public readonly Barrier AllDoneSignal;
        public long FoundItemIndex;
        public Exception? Exception;
        internal bool _returned;

        public ParallelSynchronizationContext()
        {
            ItemFoundSignal = ManualResetEventSlimmerPool.Create(false);
            AllDoneSignal = new(0);
        }

        public ParallelSynchronizationContext(int participantCount)
        {
            ItemFoundSignal = ManualResetEventSlimmerPool.Create(false);
            AllDoneSignal = new(participantCount);
        }

        public void Dispose()
        {
            if (!_returned)
            {
                _returned = true;
                ParallelSynchronizationContextPool.Shared.Return(this);
            }
            else
            {
                ItemFoundSignal.Dispose();
                AllDoneSignal.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
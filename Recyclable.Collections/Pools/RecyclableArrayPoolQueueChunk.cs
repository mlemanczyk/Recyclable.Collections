namespace Recyclable.Collections.Pools
{
    public sealed class RecyclableArrayPoolQueueChunk<T>
    {
        private static readonly T[] _empty = Array.Empty<T>();

        internal static T[] Empty => _empty;

        internal T[] Buffer;
        internal int Top;
        internal int Bottom;
        internal RecyclableArrayPoolQueueChunk<T>? Previous;
        internal RecyclableArrayPoolQueueChunk<T>? Next;

        public RecyclableArrayPoolQueueChunk()
        {
            Buffer = _empty;
        }
    }
}

namespace Recyclable.Collections.Pools
{
    public sealed class RecyclableArrayPoolChunk<T>
    {
        private static readonly T[] _empty = Array.Empty<T>();

        internal static T[] Empty => _empty;

        internal T[] Value;
        internal int Index;
        internal RecyclableArrayPoolChunk<T>? Previous;
        internal RecyclableArrayPoolChunk<T>? Next;

        public RecyclableArrayPoolChunk()
        {
            Value = _empty;
        }
    }
}

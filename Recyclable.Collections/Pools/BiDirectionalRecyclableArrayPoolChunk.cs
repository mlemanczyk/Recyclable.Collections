namespace Recyclable.Collections.Pools
{
    public sealed class BiDirectionalRecyclableArrayPoolChunk<T>
    {
        private static readonly T[] _empty = Array.Empty<T>();

        internal static T[] Empty => _empty;

        internal T[] Buffer;
        internal int Top;
        internal int Bottom;
        internal BiDirectionalRecyclableArrayPoolChunk<T>? Previous;
        internal BiDirectionalRecyclableArrayPoolChunk<T>? Next;

        public BiDirectionalRecyclableArrayPoolChunk()
        {
            Buffer = _empty;
        }
    }
}

using Microsoft.Extensions.ObjectPool;

namespace Recyclable.Collections.Pools
{
    public static class RecyclableArrayPoolChunkPool<T>
    {
        private static readonly ObjectPool<RecyclableArrayPoolChunk<T>> _pool =
            new DefaultObjectPool<RecyclableArrayPoolChunk<T>>(new DefaultPooledObjectPolicy<RecyclableArrayPoolChunk<T>>());

        public static RecyclableArrayPoolChunk<T> Rent() => _pool.Get();

        public static void Return(RecyclableArrayPoolChunk<T> chunk)
        {
            chunk.Index = 0;
            chunk.Previous = null;
            chunk.Next = null;
            _pool.Return(chunk);
        }

        public static void Dispose(RecyclableArrayPoolChunk<T> chunk, bool needsClearing)
        {
            var buffer = chunk.Value;
            if (buffer.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, needsClearing);
            }
            chunk.Value = RecyclableArrayPoolChunk<T>.Empty;
            chunk.Index = 0;
            chunk.Previous = null;
            chunk.Next = null;
            _pool.Return(chunk);
        }
    }
}

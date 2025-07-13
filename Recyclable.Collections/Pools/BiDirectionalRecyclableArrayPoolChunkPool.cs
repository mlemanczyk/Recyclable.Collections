using Microsoft.Extensions.ObjectPool;

namespace Recyclable.Collections.Pools
{
    public static class BiDirectionalRecyclableArrayPoolChunkPool<T>
    {
        private static readonly ObjectPool<BiDirectionalRecyclableArrayPoolChunk<T>> _pool =
            new DefaultObjectPool<BiDirectionalRecyclableArrayPoolChunk<T>>(new DefaultPooledObjectPolicy<BiDirectionalRecyclableArrayPoolChunk<T>>());

        public static BiDirectionalRecyclableArrayPoolChunk<T> Rent() => _pool.Get();

        public static void Return(BiDirectionalRecyclableArrayPoolChunk<T> chunk)
        {
            chunk.Top = 0;
            chunk.Bottom = 0;
            chunk.Previous = null;
            chunk.Next = null;
            _pool.Return(chunk);
        }

        public static void Dispose(BiDirectionalRecyclableArrayPoolChunk<T> chunk, bool needsClearing)
        {
            var buffer = chunk.Buffer;
            if (buffer.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, needsClearing);
            }

            chunk.Buffer = BiDirectionalRecyclableArrayPoolChunk<T>.Empty;
            chunk.Top = 0;
            chunk.Bottom = 0;
            chunk.Previous = null;
            chunk.Next = null;
            _pool.Return(chunk);
        }
    }
}

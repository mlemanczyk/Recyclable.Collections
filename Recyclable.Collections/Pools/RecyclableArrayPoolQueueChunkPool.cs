using Microsoft.Extensions.ObjectPool;

namespace Recyclable.Collections.Pools
{
    public static class RecyclableArrayPoolQueueChunkPool<T>
    {
        private static readonly ObjectPool<RecyclableArrayPoolQueueChunk<T>> _pool =
            new DefaultObjectPool<RecyclableArrayPoolQueueChunk<T>>(new DefaultPooledObjectPolicy<RecyclableArrayPoolQueueChunk<T>>());

        public static RecyclableArrayPoolQueueChunk<T> Rent() => _pool.Get();

        public static void Return(RecyclableArrayPoolQueueChunk<T> chunk)
        {
            chunk.Top = 0;
            chunk.Bottom = 0;
            chunk.Previous = null;
            chunk.Next = null;
            _pool.Return(chunk);
        }

        public static void Dispose(RecyclableArrayPoolQueueChunk<T> chunk, bool needsClearing)
        {
            var buffer = chunk.Buffer;
            if (buffer.Length >= RecyclableDefaults.MinPooledArrayLength)
            {
                RecyclableArrayPool<T>.ReturnShared(buffer, needsClearing);
            }

            chunk.Buffer = RecyclableArrayPoolQueueChunk<T>.Empty;
            chunk.Top = 0;
            chunk.Bottom = 0;
            chunk.Previous = null;
            chunk.Next = null;
            _pool.Return(chunk);
        }
    }
}

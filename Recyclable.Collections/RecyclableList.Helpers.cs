using System.Runtime.CompilerServices;
using Recyclable.Collections.Pools;

namespace Recyclable.Collections
{
	internal static class RecyclableListHelpers<T>
	{
		private static readonly bool NeedsClearing = !typeof(T).IsValueType;

		public static int CalculateIndex(int index, int lowerBound) => index >= 0 ? index + lowerBound : RecyclableDefaults.ItemNotFoundIndex;

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static void EnsureCapacity(RecyclableList<T> list)
		{
			// TODO: Measure performance
			// T[] newMemoryBlock = RecyclableListHelpers<T>._arrayPool.Rent(_capacity <<= 1);
			T[] oldMemoryBlock = list._memoryBlock;
			list._memoryBlock = checked(list._capacity <<= 1) < RecyclableDefaults.MinPooledArrayLength
				? new T[list._capacity]
				: RecyclableArrayPool<T>.RentShared(list._capacity);

			// & WAS SLOWER WITHOUT
			new ReadOnlySpan<T>(oldMemoryBlock, 0, list._count).CopyTo(list._memoryBlock);

			if (oldMemoryBlock.Length >= RecyclableDefaults.MinPooledArrayLength)
			{
				// TODO: Measure gain vs relying on arrayPool to clear
				//if (NeedsClearing)
				//{
				//	Array.Clear(source);
				//}

				RecyclableArrayPool<T>.ReturnShared(oldMemoryBlock, NeedsClearing);
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static int EnsureCapacity(RecyclableList<T> sourceList, int newSize)
		{
			T[] oldMemoryBlock = sourceList._memoryBlock;
			sourceList._memoryBlock = newSize >= RecyclableDefaults.MinPooledArrayLength
				? RecyclableArrayPool<T>.RentShared(newSize)
				: new T[newSize];

			// & WAS SLOWER AS ARRAY
			new ReadOnlySpan<T>(oldMemoryBlock, 0, sourceList._capacity).CopyTo(sourceList._memoryBlock);

			if (oldMemoryBlock.Length >= RecyclableDefaults.MinPooledArrayLength)
			{
				// TODO: Measure gain vs relying on arrayPool to clear
				//if (NeedsClearing)
				//{
				//	Array.Clear(source);
				//}

				// If anything, it has been already cleared above, so we don't need to repeat it.
				RecyclableArrayPool<T>.ReturnShared(oldMemoryBlock, NeedsClearing);
			}

			return newSize;
		}

		[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.AggressiveOptimization)]
		public static int EnsureCapacity(RecyclableList<T> sourceList, int oldSize, int newSize)
		{
			T[] oldMemoryBlock = sourceList._memoryBlock;
			sourceList._memoryBlock = newSize >= RecyclableDefaults.MinPooledArrayLength
				? RecyclableArrayPool<T>.RentShared(newSize)
				: new T[newSize];

			// & WAS SLOWER AS ARRAY
			new ReadOnlySpan<T>(oldMemoryBlock, 0, oldSize).CopyTo(sourceList._memoryBlock);

			if (oldMemoryBlock.Length >= RecyclableDefaults.MinPooledArrayLength)
			{
				// TODO: Measure gain vs relying on arrayPool to clear
				//if (NeedsClearing)
				//{
				//	Array.Clear(source);
				//}

				// If anything, it has been already cleared above, so we don't need to repeat it.
				RecyclableArrayPool<T>.ReturnShared(oldMemoryBlock, NeedsClearing);
			}

			return newSize;
		}
	}
}

using Microsoft.Extensions.ObjectPool;
using System.Runtime.CompilerServices;

namespace Recyclable.Collections
{
	public sealed class ItemRange : IDisposable
	{
		private static readonly ObjectPool<ItemRange> _defaultPool = ItemRangePool.Shared;

		public int BlockIndex;
		public int StartingItemIndex;
		public long ItemsToSearchCount;
		private ObjectPool<ItemRange>? _assignedPool;

		public ItemRange()
		{

		}

		public ItemRange(int blockIndex, int startingItemIndex, long itemsToSearchCount)
		{
			BlockIndex = blockIndex;
			ItemsToSearchCount = itemsToSearchCount;
			StartingItemIndex = startingItemIndex;
		}

		public void Dispose()
		{
			if (_assignedPool != null)
			{
				var assignedPool = _assignedPool;
				_assignedPool = null;
				assignedPool.Return(this);
			}

			GC.SuppressFinalize(this);
		}

		public static ItemRange Create(int blockIndex, int startingItemIndex, long itemsToSearchCount)
		{
			var itemRange = _defaultPool.Get();
			itemRange.BlockIndex = blockIndex;
			itemRange.ItemsToSearchCount = itemsToSearchCount;
			itemRange.StartingItemIndex = startingItemIndex;
			return itemRange;
		}
	}

	public static class ItemRangePool
	{
		[ThreadStatic]
		private static ObjectPool<ItemRange> _shared;
		public static ObjectPool<ItemRange> Shared => _shared;

		static ItemRangePool()
		{
			_shared = new DefaultObjectPool<ItemRange>(new DefaultPooledObjectPolicy<ItemRange>());
		}
	}

	public sealed class BarrierSlimmer : IDisposable
	{

		private static readonly ObjectPool<BarrierSlimmer> _defaultPool = new DefaultObjectPool<BarrierSlimmer>(new DefaultPooledObjectPolicy<BarrierSlimmer>());
		private ObjectPool<BarrierSlimmer>? _assignedPool;
		private readonly object _lock = new();

		public bool HasParticipants;
		public int Participants;

		public void AddParticipant()
		{
			Monitor.Enter(_lock);
			if (++Participants == 1)
			{
				HasParticipants = true;
			}

			Monitor.Exit(_lock);
		}

		public void RemoveParticipant()
		{
			Monitor.Enter(_lock);
			if (Participants == 0)
			{
				Monitor.Exit(_lock);
				ThrowNoMoreParticipants();
			}

			if (--Participants == 0)
			{
				HasParticipants = false;
			}

			Monitor.Exit(_lock);
		}

		private static void ThrowNoMoreParticipants()
		{
			throw new InvalidOperationException("There are no more participants. The operation is not allowed");
		}

		public BarrierSlimmer()
		{
		}

		public BarrierSlimmer(int participants)
		{
			Participants = participants;
			HasParticipants = participants > 0;
		}

		public static BarrierSlimmer Create(int participants)
		{
			var result = _defaultPool.Get();
			result.Participants = participants;
			result.HasParticipants = participants > 0;
			return result;
		}

		public void Dispose()
		{
			var assignedPool = _assignedPool;
			_assignedPool = null;
			assignedPool?.Return(this);

			GC.SuppressFinalize(this);
		}

		public void SignalAndWait()
		{
			RemoveParticipant();

			if (!HasParticipants)
			{
				return;
			}

			if (!HasParticipants)
			{
				return;
			}

			if (!HasParticipants)
			{
				return;
			}

			var waitSpinner = new SpinWait();
			while (HasParticipants)
			{
				waitSpinner.SpinOnce();
			}
		}
	}

	public sealed class ManualResetEventSlimmer : IDisposable
	{
		private static readonly ObjectPool<ManualResetEventSlimmer> _defaultPool = ManualResetEventSlimmerPool.Shared;

		private bool _isSet;
		public bool IsSet => _isSet;
		private ObjectPool<ManualResetEventSlimmer>? _assignedPool;

		public ManualResetEventSlimmer(bool initialState)
		{
			_isSet = initialState;
		}

		public ManualResetEventSlimmer()
		{

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
			var assignedPool = _assignedPool;
			_assignedPool = null;
			assignedPool?.Return(this);

			GC.SuppressFinalize(this);
		}

		public static ManualResetEventSlimmer Create(bool initialState)
		{
			ManualResetEventSlimmer result = _defaultPool.Get();
			result._assignedPool = _defaultPool;
			result._isSet = initialState;
			return result;
		}
	}

	public static class ManualResetEventSlimmerPool
	{
		[ThreadStatic]
		private static ObjectPool<ManualResetEventSlimmer> _shared;
		public static ObjectPool<ManualResetEventSlimmer> Shared => _shared;

		static ManualResetEventSlimmerPool()
		{
			_shared = new DefaultObjectPool<ManualResetEventSlimmer>(new DefaultPooledObjectPolicy<ManualResetEventSlimmer>());
		}
	}

	public static class ItemRangesIterator
	{
		[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.NoInlining)]
		public static void Iterate(int blockIndex, int blockSize, byte blockSizePow2BitShift, long itemsCount, long step, Func<ItemRange, bool> action)
		{
			int itemIndex = 0;
			ItemRange itemRange;
			while (itemsCount > step)
			{
				itemRange = ItemRange.Create(blockIndex, itemIndex, step);
				if (!action(itemRange))
				{
					break;
				}

				itemsCount -= step;
				//blockIndex = (int)Math.DivRem(itemIndex + step, blockSize, out itemIndex);
				blockIndex += (int)((itemIndex + step) >> blockSizePow2BitShift);
				itemIndex = (int)((itemIndex + step) & (blockSize - 1));
				//itemIndex += step;
				//while (itemIndex >= blockSize)
				//{
				//	blockIndex++;
				//	itemIndex -= blockSize;
				//}
			}

			itemRange = ItemRange.Create(blockIndex, itemIndex, itemsCount);
			_ = action(itemRange);
		}
	}
}

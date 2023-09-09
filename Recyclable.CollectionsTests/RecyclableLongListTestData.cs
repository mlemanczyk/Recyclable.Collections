using MoreLinq;
using Open.Numeric.Primes;
using Recyclable.Collections;
using System.Numerics;

namespace Recyclable.CollectionsTests
{
	public static class RecyclableLongListTestData
	{

		private static readonly object[] _sourceRefData = Enumerable.Range(1, 100).Cast<object>().ToArray();
		public static object[] SourceRefData => _sourceRefData;

		public static IEnumerable<long> CreateTestData(long itemsCount)
		{
			for (long index = 1; index <= itemsCount; index++)
			{
				yield return index;
			}
		}

		public static IEnumerable<object> CreateRefTestData(long itemsCount)
		{
			for (long index = 1; index <= itemsCount; index++)
			{
				yield return index;
			}
		}

		public static readonly IEnumerable<long> ItemsCountVariants = new long[]
		{
			//RecyclableDefaults.MinPooledArrayLength - 5, RecyclableDefaults.MinPooledArrayLength - 1, RecyclableDefaults.MinPooledArrayLength, RecyclableDefaults.MinPooledArrayLength + 1, RecyclableDefaults.MinPooledArrayLength + 5, 127, 128, 129, RecyclableDefaults.MinItemsCountForParallelization
			0, 1, 2, 3, 7, 10, 16, RecyclableDefaults.MinPooledArrayLength - 5, RecyclableDefaults.MinPooledArrayLength - 1, RecyclableDefaults.MinPooledArrayLength, RecyclableDefaults.MinPooledArrayLength + 1, RecyclableDefaults.MinPooledArrayLength + 5, RecyclableDefaults.MinPooledArrayLength * 100
		};

		public static readonly IEnumerable<int> BlockSizeVariants = new int[]
		{
			1, 2, 4, 16, RecyclableDefaults.MinPooledArrayLength - 5, (int)BitOperations.RoundUpToPowerOf2(RecyclableDefaults.MinPooledArrayLength)
		};

		private static bool IsValidBlockSize(long itemsCount, long blockSize) => itemsCount is < 1024 || blockSize is > 8;

		private static IEnumerable<object[]> GetTargetDataVariants()
		{
			foreach (var itemsCount in ItemsCountVariants)
			{
				foreach (var targetBlockSize in BlockSizeVariants)
				{
					if (!IsValidBlockSize(itemsCount, targetBlockSize))
					{
						continue;
					}

					yield return new object[] { itemsCount, targetBlockSize };
				}
			}
		}

		public static IEnumerable<object[]> TargetDataVariants => GetTargetDataVariants();

		private static IEnumerable<object[]> CombineSourceDataWithBlockSize(IEnumerable<object[]> sourceData)
		{
			foreach (var testData in sourceData)
			{
				foreach (var targetBlockSize in BlockSizeVariants)
				{
					if (!IsValidBlockSize((long)testData[2], targetBlockSize))
					{
						continue;
					}

					yield return new object[] { testData[0], testData[1], testData[2], targetBlockSize };
				}
			}
		}

		private static IEnumerable<object[]> CreateSourceDataVariants(long itemsCount, bool refType)
		{
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

			IDisposable disposable;

			switch (refType)
			{
				case false:
					long[] testData = CreateTestData(itemsCount).ToArray();

					yield return new object[] { $"int[{itemsCount}]", testData, itemsCount };
					yield return new object[] { $"ReadOnlySpan[{itemsCount}]", testData, itemsCount };
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					yield return new object[] { $"List<int>(itemsCount: {itemsCount}", testData.ToList(), itemsCount };
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					yield return new object[] { $"RecyclableList<int>(itemsCount: {itemsCount})", disposable = testData.ToRecyclableList(), itemsCount };
					//disposable.Dispose();
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					foreach (var sourceBlockSize in BlockSizeVariants)
					{
						if (!IsValidBlockSize(itemsCount, sourceBlockSize))
						{
							continue;
						}

						yield return new object[]
						{
							$"RecyclableLongList<int>(itemsCount: {itemsCount}, minBlockSize: {sourceBlockSize})",
							disposable = testData.ToRecyclableLongList(sourceBlockSize),
							itemsCount
						};

						//disposable.Dispose();
						//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
					}

					yield return new object[] { $"IList<int>(itemsCount: {itemsCount})", new CustomIList<long>(testData), itemsCount };
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					yield return new object[] { $"IEnumerable<int>(itemsCount: {itemsCount}) with non-enumerated count", testData, itemsCount };
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					yield return new object[] { $"IEnumerable<int>(itemsCount: {itemsCount}) without non-enumerated count", new EnumerableWithoutCount<long>(testData), itemsCount };
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					break;

				case true:
					IEnumerable<object> testRefData = CreateRefTestData(itemsCount).ToArray();

					yield return new object[] { $"int[{itemsCount}]", testRefData, itemsCount };
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					yield return new object[] { $"List<int>(itemsCount: {itemsCount}", testRefData.ToList(), itemsCount };
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					yield return new object[] { $"RecyclableList<int>(itemsCount: {itemsCount})", disposable = testRefData.ToRecyclableList(), itemsCount };

					//disposable.Dispose();
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					foreach (var sourceBlockSize in BlockSizeVariants)
					{
						if (!IsValidBlockSize(itemsCount, sourceBlockSize))
						{
							continue;
						}

						yield return new object[]
						{
							$"RecyclableLongList<int>(itemsCount: {itemsCount}, minBlockSize: {sourceBlockSize})",
							disposable = testRefData.ToRecyclableLongList(sourceBlockSize),
							itemsCount
						};

						//disposable.Dispose();
						//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
					}

					yield return new object[] { $"IList<int>(itemsCount: {itemsCount})", new CustomIList<object>(testRefData), itemsCount };
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					yield return new object[] { $"IEnumerable<int>(itemsCount: {itemsCount}) with non-enumerated count", testRefData, itemsCount };
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					yield return new object[] { $"IEnumerable<int>(itemsCount: {itemsCount}) without non-enumerated count", new EnumerableWithoutCount<object>(testRefData), itemsCount };
					//GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);

					break;
			}
		}

		private static IEnumerable<object[]> CreateSourceDataVariants(bool refType)
		{
			foreach (var itemsCount in ItemsCountVariants)
			{
				foreach (var variant in CreateSourceDataVariants(itemsCount, refType))
				{
					yield return variant;
				}
			}
		}

		public static IEnumerable<object[]> EmptySourceDataVariants => CreateSourceDataVariants(0, false);
		public static IEnumerable<object[]> SourceDataVariants => CreateSourceDataVariants(false);
		public static IEnumerable<object[]> SourceDataWithItemIndexVariants
		{
			get
			{
				foreach (var testCase in CreateSourceDataVariants(false).Where(testCase => (long)testCase[2] > 0))
				{
					var itemIndexes = CreateItemIndexVariants((long)testCase[2], RecyclableDefaults.BlockSize).ToArray();
					yield return new object[] { testCase[0], testCase[1], testCase[2], itemIndexes };
				}
			}
		}

		public static IEnumerable<object[]> SourceRefDataWithItemIndexVariants
		{
			get
			{
				IEnumerable<object[]> testCases = CreateSourceDataVariants(true).Where(testCase => (long)testCase[2] > 0);
				foreach (var testCase in testCases)
				{
					var itemIndexes = CreateItemIndexVariants((long)testCase[2], RecyclableDefaults.BlockSize).ToArray();
					
					yield return new object[] { testCase[0], testCase[1], testCase[2], itemIndexes };
				}
			}
		}

		public static IEnumerable<object[]> SourceDataWithBlockSizeVariants => CombineSourceDataWithBlockSize(SourceDataVariants);
		public static IEnumerable<object[]> SourceRefDataVariants => CreateSourceDataVariants(true);
		public static IEnumerable<object[]> SourceRefDataWithBlockSizeVariants { get; } = CombineSourceDataWithBlockSize(SourceRefDataVariants);

		private static IEnumerable<long> _initialItemIndexVariants => new long[] { 0, 1, 2, 3, 4, 5 };
		private static IEnumerable<long> CreateItemIndexVariants(long itemsCount, int blockSize)
		{
			IEnumerable<long> primes = _initialItemIndexVariants.Concat(
				_initialItemIndexVariants.SelectMany(index => new long[] { blockSize - index, blockSize + index })
				//new long[] { blockSize - 3, blockSize - 2, blockSize - 1, blockSize, blockSize + 1, blockSize + 2, blockSize + 3 }
			).ToArray();

			IEnumerable<long> second = Prime.Numbers.StartingAt(7)
										 .TakeUntil(prime => 
											prime > ((blockSize * 2) - 1) ||
											prime >= itemsCount
										);

			primes = primes.Concat(second).ToArray();

			primes = primes.Concat(
					primes.Select(index => itemsCount - index - 1)
				)
				.Where(index => itemsCount > 0 && index >= 0 && index < itemsCount).ToArray()
				.Distinct().ToArray();

			return primes;

			//IEnumerable<long> CreateItemIndexVariants()
			//{
				//if (itemsCount > 0)
				//{
				//	yield return 0;
				//}

				//foreach (long prime in primes)
				//{
				//	yield return prime;

				//	long index = itemsCount - prime;
				//	if (index >= 0)
				//	{
				//		yield return index;
				//	}
				//}

				//for (long index = itemsCount - (blockSize * 2) - 1; index < itemsCount; index++)
				//{
				//	if (index >= 0)
				//	{
				//		yield return index;
				//	}
				//}

				//if ((blockSize / 16 * 16) + 1 < itemsCount)
				//{
				//	yield return (blockSize / 16 * 16) + 1;
				//}
			//}

			//return CreateItemIndexVariants();
		}

		public static IEnumerable<object[]> CombineSourceRefDataWithBlockSizeWithItemIndex(bool refType)
		{
			IEnumerable<object[]> sourceData = refType switch
			{
				false => SourceDataWithBlockSizeVariants,
				true => SourceRefDataWithBlockSizeVariants
			};

			foreach (var testCase in sourceData)
			{
				long itemsCount = (long)testCase[2];
				if (itemsCount == 0)
				{
					continue;
				}

				long[] itemIndexes = CreateItemIndexVariants(itemsCount, (int)testCase[3])
					.Where(_ => IsValidBlockSize((long)testCase[2], (int)testCase[3]))
					.ToArray();

				yield return new object[] { testCase[0], testCase[1], (long)testCase[2], (int)testCase[3], itemIndexes };
			}
		}

		public static IEnumerable<object[]> SourceDataWithBlockSizeWithItemIndexVariants { get; } = CombineSourceRefDataWithBlockSizeWithItemIndex(false);
		public static IEnumerable<object[]> SourceRefDataWithBlockSizeWithItemIndexVariants { get; } = CombineSourceRefDataWithBlockSizeWithItemIndex(true);
	}
}
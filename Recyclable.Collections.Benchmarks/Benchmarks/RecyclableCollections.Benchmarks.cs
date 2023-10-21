using BenchmarkDotNet.Attributes;
using Collections.Benchmarks.Core;

namespace Recyclable.Collections.Benchmarks
{
	public partial class RecyclableCollectionsBenchmarks : DataDrivenBenchmarksBase<CollectionsBenchmarksSource>
	{
		protected override string GetTestMethodName(CollectionsBenchmarksSource benchmarkType) => TestCase switch
		{
			CollectionsBenchmarkType.AddRangeWhenSourceIsSameType => $"{benchmarkType}_AddRangeWhenSourceIs{benchmarkType}",
			_ => $"{benchmarkType}_{TestCase}"
		};

		public RecyclableCollectionsBenchmarks()
		{
			TestObjectCount = 1;
			//BaseDataType = RecyclableCollectionsBenchmarksSource.List;
			DataType = CollectionsBenchmarksSource.RecyclableList;
		}

		// 0 2 4 8 16 32 64 128 256 512 1024 2048 4096 8192 16384 32768 65536 131072 262144 524_288
		// 1048576 2097152 4194304 8388608 16777216 33554432 67108864 134217728 268435456 536870912
		// 1073741824 2147483648

		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 1_000_000)]
		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 1_000_000)]
		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 100_000_000)]
		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 850_000 - 1, 1_000_000, RecyclableDefaults.MaxPooledBlockSize)]
		// [Params(1_000_000, 10_000_000, 100_000_000, 1_000_000_000)]
		// [Params(1, 10, 64, 128, 192, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144)]
		// [Params(524_288, 786_432, 1_048_576)]
		// [Params(262_144, 393_216, 524_288)]
		// [Params(850_000_000)]
		// [Params(100_000_000)]
		// [Params(RecyclableDefaults.MinItemsCountForParallelization)]
		[Params(16_384)]
		// [Params(127)]
		// [Params(16)]
		// [Params(10, 15, 128)]
		// [Params(1)]
		// [Params(RecyclableDefaults.MaxPooledBlockSize)]
		// [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, 50, 60, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144, 524288, 1048576, 10_000_000, 33_554_432, 50_000_000, 100_000_000)]
		// *** [Params(RecyclableDefaults.MinPooledArrayLength - 3, RecyclableDefaults.MinPooledArrayLength - 2, RecyclableDefaults.MinPooledArrayLength - 1, RecyclableDefaults.MinPooledArrayLength, RecyclableDefaults.MinPooledArrayLength + 1, RecyclableDefaults.MinPooledArrayLength + 2, RecyclableDefaults.MinPooledArrayLength + 3)]
		// *** [Params(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, RecyclableDefaults.MinPooledArrayLength - 1, RecyclableDefaults.MinPooledArrayLength, 40, 50, 60, 70, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, RecyclableDefaults.MinItemsCountForParallelization)]
		// *** [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, 32, 40, RecyclableDefaults.MinPooledArrayLength - 1, RecyclableDefaults.MinPooledArrayLength, 40, 50, 60, 70, 80, 90, 100, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, RecyclableDefaults.MinItemsCountForParallelization)]
		// [Params(256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, RecyclableDefaults.MinItemsCountForParallelization)]
		// [Params(RecyclableDefaults.MinItemsCountForParallelization, 1_048_576, 2_097_152, 4_194_304, 8_388_608, 16_777_216, 33_554_432, RecyclableDefaults.MaxPooledBlockSize)]
		// *** [Params(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 30, RecyclableDefaults.MinPooledArrayLength - 1, RecyclableDefaults.MinPooledArrayLength, 40, 50, 60, 64, 70, 80, 90, 100, 127, 128)]
		// [Params(1, 10, 1024, 10240, RecyclableDefaults.MinPooledArrayLength, RecyclableDefaults.MinItemsCountForParallelization)]
		// [Params(RecyclableDefaults.MinItemsCountForParallelization * 3)]
		// [Params(1_048_576)]
		public override int TestObjectCount { get => base.TestObjectCount; set => base.TestObjectCount = value; }

		[Params
		(
			CollectionsBenchmarksSource.Array,
			CollectionsBenchmarksSource.List,
			CollectionsBenchmarksSource.PooledList,
			CollectionsBenchmarksSource.RecyclableList,
			CollectionsBenchmarksSource.RecyclableLongList
		)]
		public override CollectionsBenchmarksSource DataType { get => base.DataType; set => base.DataType = value; }

		[Params
		(
			CollectionsBenchmarkType.Add_WithCapacity,
			CollectionsBenchmarkType.Add,
			CollectionsBenchmarkType.AddRange_WithCapacity,
			CollectionsBenchmarkType.AddRangeWhenSourceIsArray,
			CollectionsBenchmarkType.AddRangeWhenSourceIsIEnumerable,
			CollectionsBenchmarkType.AddRangeWhenSourceIsIList,
			CollectionsBenchmarkType.AddRangeWhenSourceIsList,
			CollectionsBenchmarkType.AddRangeWhenSourceIsSameType,
			CollectionsBenchmarkType.BinarySearch_BestAndWorstCases,
			CollectionsBenchmarkType.Contains_FirstItems,
			CollectionsBenchmarkType.Contains_LastItems,
			CollectionsBenchmarkType.ConvertAll,
			CollectionsBenchmarkType.Count,
			CollectionsBenchmarkType.Create_WithCapacity,
			CollectionsBenchmarkType.Create,
			CollectionsBenchmarkType.Exists_BestAndWorstCases,
			CollectionsBenchmarkType.Find_BestAndWorstCases,
			CollectionsBenchmarkType.FindAll_BestAndWorstCases,
			CollectionsBenchmarkType.FindLast_BestAndWorstCases,
			CollectionsBenchmarkType.FindLastIndex_BestAndWorstCases,
			CollectionsBenchmarkType.ForEach,
			CollectionsBenchmarkType.GetItem,
			CollectionsBenchmarkType.IndexOf_BestAndWorstCases,
			CollectionsBenchmarkType.IndexOf_FirstItems,
			CollectionsBenchmarkType.IndexOf_LastItems,
			CollectionsBenchmarkType.LongCount,
			CollectionsBenchmarkType.Remove_FirstItems,
			CollectionsBenchmarkType.Remove_LastItems,
			CollectionsBenchmarkType.RemoveAt_FirstItems,
			CollectionsBenchmarkType.RemoveAt_LastItems,
			CollectionsBenchmarkType.SetItem,
			CollectionsBenchmarkType.VersionedForEach
		)]

		public virtual CollectionsBenchmarkType TestCase { get; set; }

		public override void Setup()
		{
			base.Setup();
			switch(TestCase)
			{
				case CollectionsBenchmarkType.AddRangeWhenSourceIsIList:
					PrepareData(CollectionsBenchmarksSource.RecyclableLongList);
					break;

				case CollectionsBenchmarkType.AddRangeWhenSourceIsSameType:
					PrepareData(CollectionsBenchmarksSource.RecyclableList);
					break;
			}
		}
	}
}

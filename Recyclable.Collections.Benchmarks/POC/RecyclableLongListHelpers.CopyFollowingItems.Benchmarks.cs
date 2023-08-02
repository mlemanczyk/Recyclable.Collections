using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks.POC
{
	public enum RecyclableLongListHelpersCopyFollowingItemsBenchmarkType
	{
		Undefined,
		Old,
		New,
	}

	public class RecyclableLongListHelpersCopyFollowingItemsBenchmarks : RecyclableBenchmarkBase<RecyclableLongListHelpersCopyFollowingItemsBenchmarkType>
	{
		[Params(RecyclableLongListHelpersCopyFollowingItemsBenchmarkType.Old)]
		public override RecyclableLongListHelpersCopyFollowingItemsBenchmarkType BaseDataType { get => base.BaseDataType; set => base.BaseDataType = value; }

		[Params(RecyclableLongListHelpersCopyFollowingItemsBenchmarkType.New)]
		public override RecyclableLongListHelpersCopyFollowingItemsBenchmarkType DataType { get => base.DataType; set => base.DataType = value; }

		protected override Action GetTestMethod(RecyclableLongListHelpersCopyFollowingItemsBenchmarkType benchmarkType) => benchmarkType switch
        {
            RecyclableLongListHelpersCopyFollowingItemsBenchmarkType.Old => CopyFollowingItemsOld,
            RecyclableLongListHelpersCopyFollowingItemsBenchmarkType.New => CopyFollowingItemsNew,
            _ => throw CreateUnknownBenchmarkTypeException(benchmarkType),
        };

		protected override void PrepareData<RecyclableLongListHelpersCopyFollowingItemsBenchmarkType>(RecyclableLongListHelpersCopyFollowingItemsBenchmarkType benchmarkType)
		{
			base.PrepareData(RecyclableCollectionsBenchmarkSource.Array);
			base.PrepareData(RecyclableCollectionsBenchmarkSource.RecyclableLongList);
		}

		public void CopyFollowingItemsOld() => RecyclableLongList<long>.Helpers.CopyFollowingItemsOld(TestObjectsAsRecyclableLongList, 0);
		public void CopyFollowingItemsNew() => RecyclableLongList<long>.Helpers.CopyFollowingItems(TestObjectsAsRecyclableLongList, 0);
	}
}

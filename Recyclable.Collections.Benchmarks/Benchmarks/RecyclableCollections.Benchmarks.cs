using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public enum RecyclableCollectionsBenchmarkType
	{
		Add_WithCapacity, Add, AddRange_WithCapacity, AddRangeWhenSourceIsArray, AddRangeWhenSourceIsIEnumerable,
		AddRangeWhenSourceIsIList, AddRangeWhenSourceIsList, AddRangeWhenSourceIsSameType, Contains_FirstItems,
		Contains_LastItems, Count, Create_WithCapacity, Create, GetItem, IndexOf_AllItems, IndexOf_FirstItems,
		IndexOf_LastItems, LongCount, Remove_FirstItems, Remove_LastItems, RemoveAt_FirstItems, RemoveAt_LastItems,
		SetItem,
	}

	public partial class RecyclableCollectionsBenchmarks : RecyclableBenchmarkBase<RecyclableCollectionsBenchmarkSource>
	{
        public RecyclableCollectionsBenchmarks() : base()
        {
			TestObjectCount = 1;
			BaselineBenchmarkType = RecyclableCollectionsBenchmarkSource.PooledList;
			BenchmarkType = RecyclableCollectionsBenchmarkSource.RecyclableList;
        }

        [Params
		(
			RecyclableCollectionsBenchmarkType.Add_WithCapacity,
			RecyclableCollectionsBenchmarkType.Add,
			RecyclableCollectionsBenchmarkType.AddRange_WithCapacity,
			RecyclableCollectionsBenchmarkType.AddRangeWhenSourceIsArray,
			RecyclableCollectionsBenchmarkType.AddRangeWhenSourceIsIEnumerable,
			RecyclableCollectionsBenchmarkType.AddRangeWhenSourceIsIList,
			RecyclableCollectionsBenchmarkType.AddRangeWhenSourceIsList,
			RecyclableCollectionsBenchmarkType.AddRangeWhenSourceIsSameType,
			RecyclableCollectionsBenchmarkType.Contains_FirstItems,
			RecyclableCollectionsBenchmarkType.Contains_LastItems,
			RecyclableCollectionsBenchmarkType.Count,
			RecyclableCollectionsBenchmarkType.Create_WithCapacity,
			RecyclableCollectionsBenchmarkType.Create,
			RecyclableCollectionsBenchmarkType.GetItem,
			RecyclableCollectionsBenchmarkType.IndexOf_AllItems,
			RecyclableCollectionsBenchmarkType.IndexOf_FirstItems,
			RecyclableCollectionsBenchmarkType.IndexOf_LastItems,
			RecyclableCollectionsBenchmarkType.LongCount,
			RecyclableCollectionsBenchmarkType.Remove_FirstItems,
			RecyclableCollectionsBenchmarkType.Remove_LastItems,
			RecyclableCollectionsBenchmarkType.RemoveAt_FirstItems,
			RecyclableCollectionsBenchmarkType.RemoveAt_LastItems,
			RecyclableCollectionsBenchmarkType.SetItem
		)]

		public RecyclableCollectionsBenchmarkType TestCase { get; set; } = RecyclableCollectionsBenchmarkType.IndexOf_AllItems;

		[Params
		(
			RecyclableCollectionsBenchmarkSource.Array,
			RecyclableCollectionsBenchmarkSource.List,
			RecyclableCollectionsBenchmarkSource.PooledList,
			RecyclableCollectionsBenchmarkSource.RecyclableArrayList,
			RecyclableCollectionsBenchmarkSource.RecyclableList
		)]
		public override RecyclableCollectionsBenchmarkSource BenchmarkType { get => base.BenchmarkType; set => base.BenchmarkType = value; }

		[Params
		(
			RecyclableCollectionsBenchmarkSource.PooledList
		)]
		public override RecyclableCollectionsBenchmarkSource BaselineBenchmarkType { get => base.BaselineBenchmarkType; set => base.BaselineBenchmarkType = value; }

		protected override Action GetTestMethod(RecyclableCollectionsBenchmarkSource benchmarkType)
			=> (Action?) GetType().GetMethod($"{benchmarkType}_{TestCase}", BindingFlags.Instance | BindingFlags.Public)?.CreateDelegate(typeof(Action), this)
			?? throw CreateMethodNotFoundException($"{benchmarkType}_{TestCase}", GetType().FullName);
	}
}

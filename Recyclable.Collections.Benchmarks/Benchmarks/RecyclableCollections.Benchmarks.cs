using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace Recyclable.Collections.Benchmarks
{
	public enum RecyclableCollectionsBenchmarkType
	{
		Unknown, Add_WithCapacity, Add, AddRange_WithCapacity, AddRangeWhenSourceIsArray, AddRangeWhenSourceIsIEnumerable,
		AddRangeWhenSourceIsIList, AddRangeWhenSourceIsList, AddRangeWhenSourceIsSameType, BinarySearch_BestAndWorstCases, 
		Contains_FirstItems, Contains_LastItems, ConvertAll, Count, Create_WithCapacity, Create, Exists_BestAndWorstCases,
		Find_BestAndWorstCases, FindAll_BestAndWorstCases, FindLast_BestAndWorstCases, FindLastIndex_BestAndWorstCases,
		ForEach, GetItem, IndexOf_BestAndWorstCases,IndexOf_FirstItems, IndexOf_LastItems, LongCount,
		Remove_FirstItems, Remove_LastItems, RemoveAt_FirstItems, RemoveAt_LastItems,
		SetItem, VersionedForEach
	}

	public partial class RecyclableCollectionsBenchmarks : RecyclableBenchmarkBase<RecyclableCollectionsBenchmarkSource>
	{
		public RecyclableCollectionsBenchmarks()
		{
			TestObjectCount = 1;
			//BaseDataType = RecyclableCollectionsBenchmarkSource.List;
			DataType = RecyclableCollectionsBenchmarkSource.RecyclableList;
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
			RecyclableCollectionsBenchmarkType.BinarySearch_BestAndWorstCases,
			RecyclableCollectionsBenchmarkType.Contains_FirstItems,
			RecyclableCollectionsBenchmarkType.Contains_LastItems,
			RecyclableCollectionsBenchmarkType.ConvertAll,
			RecyclableCollectionsBenchmarkType.Count,
			RecyclableCollectionsBenchmarkType.Create_WithCapacity,
			RecyclableCollectionsBenchmarkType.Create,
			RecyclableCollectionsBenchmarkType.Exists_BestAndWorstCases,
			RecyclableCollectionsBenchmarkType.Find_BestAndWorstCases,
			RecyclableCollectionsBenchmarkType.FindAll_BestAndWorstCases,
			RecyclableCollectionsBenchmarkType.FindLast_BestAndWorstCases,
			RecyclableCollectionsBenchmarkType.FindLastIndex_BestAndWorstCases,
			RecyclableCollectionsBenchmarkType.ForEach,
			RecyclableCollectionsBenchmarkType.GetItem,
			RecyclableCollectionsBenchmarkType.IndexOf_BestAndWorstCases,
			RecyclableCollectionsBenchmarkType.IndexOf_FirstItems,
			RecyclableCollectionsBenchmarkType.IndexOf_LastItems,
			RecyclableCollectionsBenchmarkType.LongCount,
			RecyclableCollectionsBenchmarkType.Remove_FirstItems,
			RecyclableCollectionsBenchmarkType.Remove_LastItems,
			RecyclableCollectionsBenchmarkType.RemoveAt_FirstItems,
			RecyclableCollectionsBenchmarkType.RemoveAt_LastItems,
			RecyclableCollectionsBenchmarkType.SetItem,
			RecyclableCollectionsBenchmarkType.VersionedForEach
		)]

		public RecyclableCollectionsBenchmarkType TestCase { get; set; } = RecyclableCollectionsBenchmarkType.IndexOf_BestAndWorstCases;

		[Params
		(
			RecyclableCollectionsBenchmarkSource.List
		)]
		public override RecyclableCollectionsBenchmarkSource BaseDataType { get => base.BaseDataType; set => base.BaseDataType = value; }

		[Params
		(
			RecyclableCollectionsBenchmarkSource.Array,
			RecyclableCollectionsBenchmarkSource.List,
			RecyclableCollectionsBenchmarkSource.PooledList,
			RecyclableCollectionsBenchmarkSource.RecyclableList,
			RecyclableCollectionsBenchmarkSource.RecyclableLongList
		)]
		public override RecyclableCollectionsBenchmarkSource DataType { get => base.DataType; set => base.DataType = value; }

		protected override Action? GetTestMethod(RecyclableCollectionsBenchmarkSource benchmarkType)
			=> TestCase switch
			{
				RecyclableCollectionsBenchmarkType.AddRangeWhenSourceIsSameType => (Action?)GetType().GetMethod($"{benchmarkType}_AddRangeWhenSourceIs{benchmarkType}", BindingFlags.Instance | BindingFlags.Public)?.CreateDelegate(typeof(Action), this),
				_ => (Action?) GetType().GetMethod($"{benchmarkType}_{TestCase}", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public)?.CreateDelegate(typeof(Action), this) ?? throw new InvalidOperationException($"Method {benchmarkType}_{TestCase} not found in class {GetType().FullName}")
			};

		public override void Setup()
		{
			base.Setup();
			switch(TestCase)
			{
				case RecyclableCollectionsBenchmarkType.AddRangeWhenSourceIsIList:
					PrepareData(RecyclableCollectionsBenchmarkSource.RecyclableLongList);
					break;

				case RecyclableCollectionsBenchmarkType.AddRangeWhenSourceIsSameType:
					PrepareData(RecyclableCollectionsBenchmarkSource.RecyclableList);
					break;
			}
		}
	}
}

using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public static class RecyclableLongListTestData
	{
		private static IEnumerable<long> DoCreateTestData(long itemsCount)
		{
			for (long index = 1; index <= itemsCount; index++)
			{
				yield return index;
			}
		}

		public static IEnumerable<long> CreateTestData(long itemsCount) => DoCreateTestData(itemsCount).ToArray();

		public static readonly IEnumerable<long> ItemsCountVariants = new long[]
		{
			0, 1, 2, 3, 10, 16, 32, 64, 128, 256, 333, 512, 1_024, 8_192, 16_384,
			100_000
		};

		public static readonly IEnumerable<int> BlockSizeVariants = new int[]
		{
			1, 2, 4, 8, 10, 16, 32, 64, 128, 256, 512, 1_024, 2_048, 4_096,
			8_192, 16_384
		};

		private static IEnumerable<object[]> GetTargetDataVariants()
		{
			foreach (var itemsCount in ItemsCountVariants)
			{
				foreach (var targetBlockSize in BlockSizeVariants)
				{
					yield return new object[] { itemsCount, targetBlockSize };
				}
			}
		}

		public static IEnumerable<object[]> TargetDataVariants => GetTargetDataVariants().ToArray();

		private static IEnumerable<object[]> CreateSourceDataVariants()
		{
			foreach (var itemsCount in ItemsCountVariants)
			{
				yield return new object[] { $"int[{itemsCount}]", CreateTestData(itemsCount).ToArray(), itemsCount };
				yield return new object[] { $"List<int>(itemsCount: {itemsCount}", CreateTestData(itemsCount).ToList(), itemsCount };
				yield return new object[] { $"RecyclableArrayList<int>(itemsCount: {itemsCount})", CreateTestData(itemsCount).ToRecyclableArrayList(), itemsCount };
				foreach (var sourceBlockSize in BlockSizeVariants)
				{
					yield return new object[]
					{
							$"RecyclableLongList<int>(itemsCount: {itemsCount}, minBlockSize: {sourceBlockSize})",
							CreateTestData(itemsCount).ToRecyclableLongList(sourceBlockSize),
							itemsCount
					};
				}

				yield return new object[] { $"IList<int>(itemsCount: {itemsCount})", new CustomIList<long>(CreateTestData(itemsCount)), itemsCount };
				yield return new object[] { $"IEnumerable<int>(itemsCount: {itemsCount}) with non-enumerated count", CreateTestData(itemsCount), itemsCount };
				yield return new object[] { $"IEnumerable<int>(itemsCount: {itemsCount}) without non-enumerated count", new EnumerableWithoutCount<long>(CreateTestData(itemsCount)), itemsCount };
			}
		}

		public static IEnumerable<object[]> SourceDataVariants => CreateSourceDataVariants().ToArray();

		private static IEnumerable<object[]> CreateSourceEmptyDataVariants() => new[]
		{
			new object[] { "int[]", Array.Empty<long>() },
			new object[] { "List<int>", Array.Empty<long>().ToList() },
			new object[] { "RecyclableArrayList<int>", Array.Empty<long>().ToRecyclableArrayList() },
			new object[] { "RecyclableLongList<int>", Array.Empty<long>().ToRecyclableLongList() },
			new object[] { "IList<int>", new CustomIList<long>(Array.Empty<long>()) },
			new object[] { "IEnumerable<int> with non-enumerated count", Array.Empty<long>().AsEnumerable() },
			new object[] { "IEnumerable<int> without non-enumerated count", new EnumerableWithoutCount<long>(Array.Empty<long>()) }
		};

		public static IEnumerable<object[]> EmptySourceDataVariants => CreateSourceEmptyDataVariants().ToArray();

		private static IEnumerable<object[]> CreateSourceTargetDataVariants()
		{
			foreach (var testData in SourceDataVariants)
			{
				foreach (var targetBlockSize in BlockSizeVariants)
				{
					yield return new object[] { testData[0], testData[1], testData[2], targetBlockSize };
				}
			}
		}

		public static IEnumerable<object[]> SourceTargetDataVariants => CreateSourceTargetDataVariants().ToArray();
	}
}
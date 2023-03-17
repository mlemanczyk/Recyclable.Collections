using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class RecyclableListTestData : IDisposable
	{
		private static void DisposeTestData(IEnumerable<object[]>? testCases)
		{
			if (testCases != null)
			{
				foreach (var testData in testCases)
				{
					foreach (var item in testData)
					{
						if (item is IDisposable disposable)
						{
							disposable.Dispose();
						}
					}
				}
			}
		}

		private static void DisposeTestData(IEnumerable<IEnumerable<long>>? testCases)
		{
			if (testCases != null)
			{
				foreach (var testData in testCases)
				{
					if (testData is IDisposable disposable)
					{
						disposable.Dispose();
					}
				}
			}
		}

		private static Dictionary<long, IEnumerable<long>>? _testCases = new();
		
		public RecyclableListTestData()
		{
			_testCases ??= new();
		}

		private static IEnumerable<long> DoCreateTestData(long itemsCount)
		{
			for (long index = 1; index <= itemsCount; index++)
			{
				yield return index;
			}
		}

		public static IEnumerable<long> CreateTestData(long itemsCount)
		{
			if (_testCases!.TryGetValue(itemsCount, out var testCases))
			{
				return testCases;
			}

			testCases = DoCreateTestData(itemsCount).ToArray();
			_testCases.Add(itemsCount, testCases);
			return testCases;
		}

		public void Dispose()
		{
			DisposeTestData(_testCases?.Values);
			DisposeTestData(_sourceDataVariants);
			DisposeTestData(_emptySourceDataVariants);
			_testCases = null;
			_sourceDataVariants = null;
			_targetDataVariants = null;
			_testDataVariants = null;
			_sourceTargetDataVariants = null;
			_emptySourceDataVariants = null;
			GC.SuppressFinalize(this);
		}

		public static readonly IEnumerable<long> ItemsCountVariants = new long[]
		{
			0, 1, 2, 3, 10, 16, 32, 64, 128, 256, 512, 1_024, 2_048, 4_096, 8_192, 16_384,
			32_768, 65_536, 131_072, 262_144, 524_288, 1_048_576, 10_000_000
		};

		public static readonly IEnumerable<int> BlockSizeVariants = new int[]
		{
			1, 2, 3, 10, 16, 32, 64, 128, 256, 512, 1_024, 2_048, 4_096,
			8_192, 16_384, 32_768, 65_536, 131_072, 262_144, 524_288, 1_048_576
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

		private static IEnumerable<object[]>? _targetDataVariants;
		public static IEnumerable<object[]> TargetDataVariants => _targetDataVariants ??= GetTargetDataVariants().ToArray();

		private static IEnumerable<object[]> GetTestDataVariants()
		{
			foreach (var itemsCount in ItemsCountVariants)
			{
				foreach (var sourceBlockSize in BlockSizeVariants)
				{
					foreach (var targetBlockSize in BlockSizeVariants)
					{
						yield return new object[] { itemsCount, sourceBlockSize, targetBlockSize };
					}
				}
			}
		}

		private static IEnumerable<object[]>? _testDataVariants;
		public static IEnumerable<object[]> TestDataVariants => _testDataVariants ??= GetTestDataVariants().ToArray();

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
							$"RecyclableList<int>(itemsCount: {itemsCount}, minBlockSize: {sourceBlockSize})",
							CreateTestData(itemsCount).ToRecyclableList(sourceBlockSize),
							itemsCount
					};

				}

				yield return new object[] { $"IList<int>(itemsCount: {itemsCount})", new CustomIList<long>(CreateTestData(itemsCount)), itemsCount };
				yield return new object[] { $"IEnumerable<int>(itemsCount: {itemsCount}) with non-enumerated count", CreateTestData(itemsCount), itemsCount };
				yield return new object[] { $"IEnumerable<int>(itemsCount: {itemsCount}) without non-enumerated count", new EnumerableWithoutCount<long>(CreateTestData(itemsCount)), itemsCount };
			}
		}

		private static IEnumerable<object[]>? _sourceDataVariants;
		public static IEnumerable<object[]> SourceDataVariants => _sourceDataVariants ??= CreateSourceDataVariants().ToArray();

		private static IEnumerable<object[]> CreateSourceEmptyDataVariants() => new[]
		{
			new object[] { "int[]", Array.Empty<long>() },
			new object[] { "List<int>", Array.Empty<long>().ToList() },
			new object[] { "RecyclableArrayList<int>", Array.Empty<long>().ToRecyclableArrayList() },
			new object[] { "RecyclableList<int>", Array.Empty<long>().ToRecyclableList() },
			new object[] { "IList<int>", new CustomIList<long>(Array.Empty<long>()) },
			new object[] { "IEnumerable<int> with non-enumerated count", Array.Empty<long>().AsEnumerable() },
			new object[] { "IEnumerable<int> without non-enumerated count", new EnumerableWithoutCount<long>(Array.Empty<long>()) }
		};

		private static IEnumerable<object[]>? _emptySourceDataVariants;
		public static IEnumerable<object[]> EmptySourceDataVariants => _emptySourceDataVariants ??= CreateSourceEmptyDataVariants().ToArray();

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

		private static IEnumerable<object[]>? _sourceTargetDataVariants;
		public static IEnumerable<object[]> SourceTargetDataVariants => _sourceTargetDataVariants ??= CreateSourceTargetDataVariants().ToArray();
	}
}
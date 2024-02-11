using FluentAssertions;
using Recyclable.Collections;
using Recyclable.Collections.TestData.xUnit;
using System.Collections;

namespace Recyclable.CollectionsTests
{

	public class RecyclableLongListHelperTests
	{
		[Theory]
		[ClassData(typeof(SourceDataWithBlockSizeWithItemIndexWithRangeTheoryData))]
		public void MakeRoomAndSetShouldMoveItems(string testCase, IEnumerable<long> testData, int itemsCount, int minBlockSize, in (long ItemIndex, long RangedItemsCount)[] itemRanges)
		{
			// Prepare
			var expectedData = testData.Reverse().ToArray();

			foreach ((var itemIndex, var rangedItemsCount) in itemRanges)
			{
				using var list = new RecyclableLongList<long>(testData.Reverse(), minBlockSize, itemsCount);
				var expected = expectedData.ToList();
				var rangedTestData = expected.GetRange((int)itemIndex, (int)rangedItemsCount).ToArray();
				expected.InsertRange((int)itemIndex, rangedTestData);

				if (testCase.Contains("Array[", StringComparison.OrdinalIgnoreCase))
				{
					list.InsertRange((int)itemIndex, rangedTestData);
				}
				else if (testCase.Contains("ICollection[", StringComparison.OrdinalIgnoreCase))
				{
					return;
					//list.InsertRange((int)itemIndex, (ICollection)testData);
				}
				else if (testCase.Contains("ICollection<T>[", StringComparison.OrdinalIgnoreCase))
				{
					return;
					//list.InsertRange((int)itemIndex, (ICollection<long>)testData);
				}
				else if (testCase.Contains("IEnumerable[", StringComparison.OrdinalIgnoreCase))
				{
					return;
					//list.InsertRange((int)itemIndex, (IEnumerable)testData);
				}
				else if (testCase.Contains("IReadOnlyList<T>[", StringComparison.OrdinalIgnoreCase))
				{
					return;
					//list.InsertRange((int)itemIndex, (IReadOnlyList<long>)testData);
				}
				else if (testCase.Contains("ReadOnlySpan<T>[", StringComparison.OrdinalIgnoreCase))
				{
					return;
					//list.InsertRange((int)itemIndex, new ReadOnlySpan<long>((long[])testData));
				}
				else if (testCase.Contains("Span<T>[", StringComparison.OrdinalIgnoreCase))
				{
					return;
					//list.InsertRange((int)itemIndex, new Span<long>((long[])testData));
				}
				else if (testData is long[] testDataArray)
				{
					return;
					//list.InsertRange((int)itemIndex, testDataArray);
				}
				else if (testData is List<long> testDataList)
				{
					return;
					//list.InsertRange((int)itemIndex, testDataList);
				}
				else if (testData is RecyclableList<long> testDataRecyclableList)
				{
					return;
					//list.InsertRange((int)itemIndex, testDataRecyclableList);
				}
				else if (testData is RecyclableLongList<long> testDataRecyclableLongList)
				{
					return;
					//list.InsertRange((int)itemIndex, testDataRecyclableLongList);
				}
				else if (testData is IList<long> testDataIList)
				{
					return;
					//list.InsertRange((int)itemIndex, testDataIList);
				}
				else if (testData is IEnumerable<long> testDataIEnumerable)
				{
					return;
					//list.InsertRange((int)itemIndex, testDataIEnumerable);
				}
				else
				{
					throw new InvalidCastException("Unknown type of test data");
				}

				// Validate
				list.Count.Should().Be(expected.Count);
				_ = list.Should().Equal(expected);
			}
		}
	}
}

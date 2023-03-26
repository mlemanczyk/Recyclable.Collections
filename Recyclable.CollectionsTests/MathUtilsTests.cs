using FluentAssertions;
using Recyclable.Collections;

namespace Recyclable.CollectionsTests
{
	public class MathUtilsTests
	{
		[Theory]
		[InlineData(int.MaxValue / 1024, int.MaxValue / 10 / 1024)]
		public void DivMod(long testNumber, int blockSize)
		{
			for (var testCase = testNumber; testCase >= 0; testCase--)
			{
				// Prepare
				var expectedResult = testCase / blockSize;
				var expectedRemainder = testCase % blockSize;

				// Act
				var actualResult = MathUtils.DivMod(testCase, blockSize, out long actualRemainder);

				// Validate
				_ = actualResult.Should().Be(expectedResult);
				_ = actualRemainder.Should().Be(expectedRemainder);
			}
		}

		[Theory]
		[InlineData(0, false)]
		[InlineData(1, true)]
		[InlineData(2, true)]
		[InlineData(3, false)]
		[InlineData(4, true)]
		[InlineData(5, false)]
		[InlineData(8, true)]
		[InlineData(16, true)]
		[InlineData(20, false)]
		[InlineData(32, true)]
		[InlineData(64, true)]
		[InlineData(128, true)]
		[InlineData(256, true)]
		[InlineData(512, true)]
		[InlineData(1024, true)]
		[InlineData(2048, true)]
		[InlineData(4096, true)]
		[InlineData(8192, true)]
		[InlineData(16384, true)]
		[InlineData(32768, true)]
		[InlineData(65536, true)]
		[InlineData(131072, true)]
		[InlineData(262144, true)]
		[InlineData(524288, true)]
		[InlineData(1048576, true)]
		[InlineData(2097152, true)]
		[InlineData(4194304, true)]
		[InlineData(8388608, true)]
		[InlineData(16777216, true)]
		[InlineData(33554432, true)]
		[InlineData(67108864, true)]
		[InlineData(134217728, true)]
		[InlineData(268435456, true)]
		[InlineData(536870912, true)]
		[InlineData(1073741824, true)]
		[InlineData(2147483648, true)]
		[InlineData(2147483649, false)]
		[InlineData(4294967296, true)]
		public void IsPow2(long testNumber, bool expected)
		{
			_ = MathUtils.IsPow2(testNumber).Should().Be(expected);
		}

		[Theory]
		[InlineData(-2, -1)]
		[InlineData(-1, -1)]
		[InlineData(0, -1)]
		[InlineData(1, 0)]
		[InlineData(2, 1)]
		[InlineData(3, -1)]
		[InlineData(4, 2)]
		[InlineData(8, 3)]
		public void GetPow2Shift(long testNumber, int expected)
		{
			_ = MathUtils.GetPow2Shift(testNumber).Should().Be(expected);
		}
	}
}

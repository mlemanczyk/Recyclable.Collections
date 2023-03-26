using FluentAssertions;
using Recyclable.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}

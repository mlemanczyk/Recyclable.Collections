namespace Recyclable.Collections
{
	internal class SystemRandomNumberGenerator : IRandomNumberGenerator
	{
		private readonly Random _random = new();

		public int NextInt32(int startIndex, int endIndex) => _random.Next(startIndex, endIndex);
	}
}
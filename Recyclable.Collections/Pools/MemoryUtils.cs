namespace Recyclable.Collections.Pools
{
	internal enum MemoryPressure
	{
		High,
		Medium,
		Low
	}

	internal static class MemoryUtils
	{
		private const double HighPressureThreshold = .90;       // Percent of GC memory pressure threshold we consider "high"
		private const double MediumPressureThreshold = .70;     // Percent of GC memory pressure threshold we consider "medium"

		public static MemoryPressure GetMemoryPressure()
		{
			GCMemoryInfo memoryInfo = GC.GetGCMemoryInfo();

			return memoryInfo.MemoryLoadBytes >= memoryInfo.HighMemoryLoadThresholdBytes * HighPressureThreshold
				? MemoryPressure.High
				: memoryInfo.MemoryLoadBytes >= memoryInfo.HighMemoryLoadThresholdBytes * MediumPressureThreshold
				? MemoryPressure.Medium
				: MemoryPressure.Low;
		}
	}
}
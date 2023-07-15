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
        public static MemoryPressure GetMemoryPressure()
        {
            const double HighPressureThreshold = .90;       // Percent of GC memory pressure threshold we consider "high"
            const double MediumPressureThreshold = .70;     // Percent of GC memory pressure threshold we consider "medium"

            GCMemoryInfo memoryInfo = GC.GetGCMemoryInfo();

            if (memoryInfo.MemoryLoadBytes >= memoryInfo.HighMemoryLoadThresholdBytes * HighPressureThreshold)
            {
                return MemoryPressure.High;
            }

            if (memoryInfo.MemoryLoadBytes >= memoryInfo.HighMemoryLoadThresholdBytes * MediumPressureThreshold)
            {
                return MemoryPressure.Medium;
            }

            return MemoryPressure.Low;
        }
    }
}
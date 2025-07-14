using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Recyclable.Collections.Linq;

namespace Recyclable.Collections
{
    [Obsolete("It'll be moved to Recyclable.Collections.Compatibility.List NuGet package in the next release.")]
    public static class zRecyclableLongListCompatibilityListSorted
    {
        public static void Sort<T>(this RecyclableLongList<T> list)
        {
            QuickSortExtensions<T>.QuickSort(list);
#if WITH_VERSIONING
            list._version++;
#endif
        }

        public static void Sort<T>(this RecyclableLongList<T> list, Comparison<T> comparison)
        {
            QuickSortExtensions<T>.QuickSort(list, new ComparisonToComparerAdapter<T>(comparison), new SystemRandomNumberGenerator());
#if WITH_VERSIONING
            list._version++;
#endif
        }

        public static void Sort<T>(this RecyclableLongList<T> list, IComparer<T>? comparer)
        {
            QuickSortExtensions<T>.QuickSort(list, comparer ?? Comparer<T>.Default, new SystemRandomNumberGenerator());
#if WITH_VERSIONING
            list._version++;
#endif
        }

        public static void Sort<T>(this RecyclableLongList<T> list, long index, long count, IComparer<T>? comparer)
        {
            if (count == 0)
            {
                return;
            }

            QuickSortExtensions<T>.QuickSort(list, (int)index, (int)(index + count - 1), comparer ?? Comparer<T>.Default, new SystemRandomNumberGenerator());
#if WITH_VERSIONING
            list._version++;
#endif
        }
    }
}

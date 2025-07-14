namespace Recyclable.Collections
{
    internal interface IAddRangeProvider<T>
    {
        void AddRangeTo(RecyclableList<T> list);
        void AddRangeTo(RecyclableLongList<T> list);
    }
}


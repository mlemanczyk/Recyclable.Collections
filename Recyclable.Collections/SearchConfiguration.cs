namespace Recyclable.Collections
{
    public readonly struct SearchConfiguration<TList, TItem>
    {
        public TItem ItemToFind { get; init; }
        public TList List { get; init; }
        public int BlockSizeMinus1 { get; init; }
        public byte BlockSizePow2BitShift { get; init; }
        public long ItemsCount { get; init; }
        public int StartingBlockIndex { get; init; }
        public long BlockSize { get; init; }

        // public SearchConfiguration(TList list, in TItem itemToFind)
        // {
        //     ItemToFind = itemToFind;
        //     List = list;
        // }

        public static SearchConfiguration<RecyclableList<TItem>, TItem> CreateFrom(RecyclableList<TItem> list, in TItem itemToFind) => new()
        {
            List = list,
            ItemToFind = itemToFind,
            BlockSizeMinus1 = list._blockSizeMinus1,
            BlockSizePow2BitShift = list._blockSizePow2BitShift,
            ItemsCount = list._longCount,
            BlockSize = list._blockSize
        };
    }
}
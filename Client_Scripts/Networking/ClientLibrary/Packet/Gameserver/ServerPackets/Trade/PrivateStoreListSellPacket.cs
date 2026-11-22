using System.Collections.Generic;

public class PrivateStoreListSellPacket : ServerPacket
{
    public class StoreItem
    {
        public int Type2;
        public int ObjectId;
        public int ItemId;
        public int Count;
        public int Enchant;
        public int BodyPart;
        public int Price;
        public int ReferencePrice;
    }

    public int StorePlayerId { get; private set; }
    public bool IsPackaged { get; private set; }
    public int PlayerAdena { get; private set; }
    public List<StoreItem> Items { get; private set; } = new List<StoreItem>();

    public PrivateStoreListSellPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        StorePlayerId = ReadI();
        IsPackaged = ReadI() == 1;
        PlayerAdena = ReadI();
        int count = ReadI();

        for (int i = 0; i < count; i++)
        {
            var item = new StoreItem();
            item.Type2 = ReadI();
            item.ObjectId = ReadI();
            item.ItemId = ReadI();
            item.Count = ReadI();
            ReadH(); // 0x00
            item.Enchant = ReadH();
            ReadH(); // 0x00
            item.BodyPart = ReadI();
            item.Price = ReadI();
            item.ReferencePrice = ReadI();
            Items.Add(item);
        }

        UnityEngine.Debug.Log($"[PrivateStore] Received {Items.Count} items from sell store {StorePlayerId}");
    }
}

using System.Collections.Generic;

public class PrivateStoreListBuyPacket : ServerPacket
{
    public class BuyItem
    {
        public int ObjectId;
        public int ItemId;
        public int Enchant;
        public int Count;
        public int ReferencePrice;
        public int BodyPart;
        public int Type2;
        public int Price;
        public int Quantity;
    }

    public int StorePlayerId { get; private set; }
    public int PlayerAdena { get; private set; }
    public List<BuyItem> Items { get; private set; } = new List<BuyItem>();

    public PrivateStoreListBuyPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        StorePlayerId = ReadI();
        PlayerAdena = ReadI();
        int count = ReadI();

        for (int i = 0; i < count; i++)
        {
            var item = new BuyItem();
            item.ObjectId = ReadI();
            item.ItemId = ReadI();
            item.Enchant = ReadH();
            item.Count = ReadI();
            item.ReferencePrice = ReadI();
            ReadH(); // 0
            item.BodyPart = ReadI();
            item.Type2 = ReadH();
            item.Price = ReadI();
            item.Quantity = ReadI();
            Items.Add(item);
        }

        UnityEngine.Debug.Log($"[PrivateStore] Received {Items.Count} items from buy store {StorePlayerId}");
    }
}

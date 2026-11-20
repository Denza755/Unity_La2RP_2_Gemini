using System.Collections.Generic;

public class RequestPrivateStoreSellPacket : ClientPacket
{
    public struct RequestedItem
    {
        public int ObjectId;
        public int ItemId;
        public short Enchant;
        public int Count;
        public int Price;

        public RequestedItem(int objectId, int itemId, short enchant, int count, int price)
        {
            ObjectId = objectId;
            ItemId = itemId;
            Enchant = enchant;
            Count = count;
            Price = price;
        }
    }

    public RequestPrivateStoreSellPacket(int storePlayerId, List<RequestedItem> items) : base((byte)GameClientPacketType.RequestPrivateStoreSell)
    {
        WriteI(storePlayerId);
        WriteI(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            WriteI(items[i].ObjectId);
            WriteI(items[i].ItemId);
            WriteH(items[i].Enchant);
            WriteH(0);
            WriteI(items[i].Count);
            WriteI(items[i].Price);
        }
        BuildPacket();
    }
}

using System.Collections.Generic;

public class RequestPrivateStoreBuyPacket : ClientPacket
{
    public struct RequestedItem
    {
        public int ObjectId;
        public int Count;
        public int Price;

        public RequestedItem(int objectId, int count, int price)
        {
            ObjectId = objectId;
            Count = count;
            Price = price;
        }
    }

    public RequestPrivateStoreBuyPacket(int storePlayerId, List<RequestedItem> items) : base((byte)GameClientPacketType.RequestPrivateStoreBuy)
    {
        WriteI(storePlayerId);
        WriteI(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            WriteI(items[i].ObjectId);
            WriteI(items[i].Count);
            WriteI(items[i].Price);
        }
        BuildPacket();
    }
}

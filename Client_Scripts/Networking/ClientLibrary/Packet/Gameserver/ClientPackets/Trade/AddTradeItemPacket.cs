public class AddTradeItemPacket : ClientPacket
{
    public AddTradeItemPacket(int tradeId, int objectId, int count) : base((byte)GameClientPacketType.AddTradeItem)
    {
        WriteD(tradeId);
        WriteD(objectId);
        WriteD(count);
        BuildPacket();
    }
}

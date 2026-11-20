public class TradeRequestPacket : ClientPacket
{
    public TradeRequestPacket(int objectId) : base((byte)GameClientPacketType.TradeRequest)
    {
        WriteD(objectId);
        BuildPacket();
    }
}

public class TradeDonePacket : ClientPacket
{
    public TradeDonePacket(int response) : base((byte)GameClientPacketType.TradeDone)
    {
        WriteD(response);
        BuildPacket();
    }
}

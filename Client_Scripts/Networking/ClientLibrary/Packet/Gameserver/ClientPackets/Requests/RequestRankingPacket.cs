public class RequestRankingPacket : ClientPacket
{
    public RequestRankingPacket(int category) : base((byte)GameClientPacketType.RequestRanking)
    {
        WriteI(category);
        BuildPacket();
    }
}

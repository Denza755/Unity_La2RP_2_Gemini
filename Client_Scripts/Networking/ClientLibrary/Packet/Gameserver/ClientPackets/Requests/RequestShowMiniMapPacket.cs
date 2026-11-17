public class RequestShowMiniMapPacket : ClientPacket
{
    public RequestShowMiniMapPacket() : base((byte)GameClientPacketType.RequestShowMiniMap)
    {
        BuildPacket();
    }
}

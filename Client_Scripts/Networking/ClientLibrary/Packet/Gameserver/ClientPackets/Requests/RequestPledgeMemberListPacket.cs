public class RequestPledgeMemberListPacket : ClientPacket
{
    public RequestPledgeMemberListPacket() : base((byte)GameClientPacketType.RequestPledgeMemberList)
    {
        BuildPacket();
    }
}

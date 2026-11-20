public class AnswerTradeRequestPacket : ClientPacket
{
    public AnswerTradeRequestPacket(int response) : base((byte)GameClientPacketType.AnswerTradeRequest)
    {
        WriteD(response);
        BuildPacket();
    }
}

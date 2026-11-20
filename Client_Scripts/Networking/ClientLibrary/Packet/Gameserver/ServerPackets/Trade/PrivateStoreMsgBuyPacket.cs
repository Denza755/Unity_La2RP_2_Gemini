public class PrivateStoreMsgBuyPacket : ServerPacket
{
    public int PlayerId { get; private set; }
    public string Message { get; private set; }

    public PrivateStoreMsgBuyPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        PlayerId = ReadI();
        Message = ReadS();
    }
}

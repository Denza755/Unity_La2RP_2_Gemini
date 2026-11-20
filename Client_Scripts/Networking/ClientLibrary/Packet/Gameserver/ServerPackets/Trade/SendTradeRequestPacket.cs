using System;

public class SendTradeRequestPacket : ServerPacket
{
    public int SenderId { get; private set; }

    public SendTradeRequestPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        try
        {
            SenderId = ReadI();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error parsing SendTradeRequestPacket: {ex}");
        }
    }
}

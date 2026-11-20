using System;

public class SendTradeDonePacket : ServerPacket
{
    public int Status { get; private set; } // 1 = success, 0 = fail/cancel

    public SendTradeDonePacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        try
        {
            Status = ReadI();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error parsing SendTradeDonePacket: {ex}");
        }
    }
}

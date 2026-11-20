using System;

public class TradeOtherAddPacket : ServerPacket
{
    public int ObjectId { get; private set; }
    public int ItemId { get; private set; }
    public int Count { get; private set; }
    public int Enchant { get; private set; }

    public TradeOtherAddPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        try
        {
            ReadH(); // count (usually 1)
            ReadH(); // type1
            ObjectId = ReadI();
            ItemId = ReadI();
            Count = ReadI();
            ReadH(); // type2
            ReadH(); // unk
            ReadI(); // bodyPart
            Enchant = ReadH();
            ReadH();
            ReadH();
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error parsing TradeOtherAddPacket: {ex}");
        }
    }
}

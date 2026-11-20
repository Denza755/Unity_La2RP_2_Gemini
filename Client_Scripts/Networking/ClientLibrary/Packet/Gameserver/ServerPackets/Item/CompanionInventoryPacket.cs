using System;

public class CompanionInventoryPacket : AbstractItemPacket
{
    public int CompanionObjectId { get; private set; }
    public string CompanionName { get; private set; }
    public int Adena { get; private set; }
    public ItemInstance[] Items { get; private set; }

    public CompanionInventoryPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        CompanionObjectId = ReadI();
        CompanionName = ReadS();
        Adena = ReadI();
        int count = ReadH();
        Items = new ItemInstance[count];
        for (int i = 0; i < count; i++)
        {
            Items[i] = ReadItem();
        }
    }
}

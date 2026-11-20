using System;

public class WarehouseDepositListPacket : AbstractItemPacket
{
    public int WarehouseType { get; private set; }
    public int PlayerAdena { get; private set; }
    public ItemInstance[] Items { get; private set; }

    public WarehouseDepositListPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        try
        {
            WarehouseType = ReadH();
            PlayerAdena = ReadI();
            int count = ReadH();
            Items = new ItemInstance[count];
            for (int i = 0; i < count; i++)
            {
                int type1 = ReadH();
                int objectId = ReadI();
                int itemId = ReadI();
                int itemCount = ReadI();
                int type2 = ReadH();
                int customType1 = ReadH();
                int bodyPart = ReadI();
                int enchant = ReadH();
                int customType2 = ReadH();
                int unk = ReadH();

                Items[i] = new ItemInstance(type1, objectId, itemId, itemCount, type2, customType1, 0, bodyPart, enchant, customType2, 0, 0, i);
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error parsing WarehouseDepositListPacket: {ex}");
        }
    }
}

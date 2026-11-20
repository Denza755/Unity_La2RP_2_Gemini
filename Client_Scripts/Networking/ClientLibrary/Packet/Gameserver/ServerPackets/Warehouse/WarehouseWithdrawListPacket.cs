using System;

public class WarehouseWithdrawListPacket : AbstractItemPacket
{
    public int WarehouseType { get; private set; }
    public int PlayerAdena { get; private set; }
    public ItemInstance[] Items { get; private set; }

    public WarehouseWithdrawListPacket(byte[] d) : base(d)
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
                ReadI(); // objectId duplicate
                ReadL(); // augmentation or 0

                Items[i] = new ItemInstance(objectId, itemId, ItemLocation.Inventory, i, itemCount, (ItemType1)type1, (ItemType2)type2, false, (ItemSlot)bodyPart, enchant, 0L);
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error parsing WarehouseWithdrawListPacket: {ex}");
        }
    }
}

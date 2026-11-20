using System.Collections.Generic;

public class SendWarehouseDepositListPacket : ClientPacket
{
    public SendWarehouseDepositListPacket(List<(int objectId, int count)> items) : base((byte)GameClientPacketType.SendWarehouseDepositList)
    {
        WriteD(items.Count);
        foreach (var item in items)
        {
            WriteD(item.objectId);
            WriteD(item.count);
        }
        BuildPacket();
    }
}

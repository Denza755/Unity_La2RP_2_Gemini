using System.Collections.Generic;

public class SendWarehouseWithdrawListPacket : ClientPacket
{
    public SendWarehouseWithdrawListPacket(List<(int objectId, int count)> items) : base((byte)GameClientPacketType.SendWarehouseWithdrawList)
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

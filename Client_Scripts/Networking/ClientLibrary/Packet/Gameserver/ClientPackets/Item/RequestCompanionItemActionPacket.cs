public class RequestCompanionItemActionPacket : ClientPacket
{
    public RequestCompanionItemActionPacket(byte action, int companionObjectId, int itemObjectId, int count) 
        : base((byte)GameClientPacketType.RequestCompanionItemAction)
    {
        WriteB(action);
        WriteI(companionObjectId);
        WriteI(itemObjectId);
        WriteI(count);
        BuildPacket();
    }
}

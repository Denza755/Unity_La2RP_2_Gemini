public class RequestDeleteMacroPacket : ClientPacket
{
    public RequestDeleteMacroPacket(int macroId) : base((byte)GameClientPacketType.RequestDeleteMacro)
    {
        WriteI(macroId);
        BuildPacket();
    }
}

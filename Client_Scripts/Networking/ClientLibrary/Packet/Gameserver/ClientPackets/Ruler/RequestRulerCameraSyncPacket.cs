public class RequestRulerCameraSyncPacket : ClientPacket
{
    public RequestRulerCameraSyncPacket(bool active, int l2X, int l2Y, int l2Z)
        : base((byte)GameClientPacketType.RequestRulerCameraSync)
    {
        WriteB(active);
        WriteI(l2X);
        WriteI(l2Y);
        WriteI(l2Z);

        BuildPacket();
    }
}

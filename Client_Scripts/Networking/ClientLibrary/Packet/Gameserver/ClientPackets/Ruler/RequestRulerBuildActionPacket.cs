using UnityEngine;

public class RequestRulerBuildActionPacket : ClientPacket
{
    // Build constructor
    public RequestRulerBuildActionPacket(string prefabName, Vector3 pos, float rotY, float scale)
        : base((byte)GameClientPacketType.RequestRulerBuildAction)
    {
        WriteB((byte)1); // Action 1 = build
        WriteS(prefabName);
        WriteD(pos.x);
        WriteD(pos.y);
        WriteD(pos.z);
        WriteD(rotY);
        WriteD(scale);

        BuildPacket();
    }

    // Demolish constructor
    public RequestRulerBuildActionPacket(int buildingId)
        : base((byte)GameClientPacketType.RequestRulerBuildAction)
    {
        WriteB((byte)2); // Action 2 = demolish
        WriteI(buildingId);

        BuildPacket();
    }
}

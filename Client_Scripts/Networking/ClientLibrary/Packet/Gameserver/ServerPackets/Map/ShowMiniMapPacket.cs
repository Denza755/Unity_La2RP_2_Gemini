using UnityEngine;

public class ShowMiniMapPacket : ServerPacket
{
    public int MapId { get; private set; }
    public int SevenSignsPeriod { get; private set; }

    public ShowMiniMapPacket(byte[] data) : base(data)
    {
        Parse();
    }

    public override void Parse()
    {
        // After base constructor skips opcode byte, we need at least 4 bytes for MapId.
        // SevenSignsPeriod (4 bytes) is optional — server may send a short packet
        // if SevenSignsManager throws an exception during writeImpl.
        if (_packetData.Length > _iterator + 3)
        {
            MapId = ReadI();
        }
        else
        {
            MapId = 1665; // REGULAR_MAP fallback
            Debug.LogWarning($"ShowMiniMapPacket: packet too short ({_packetData.Length} bytes), using default mapId={MapId}");
        }

        if (_packetData.Length > _iterator + 3)
        {
            SevenSignsPeriod = ReadI();
        }
        else
        {
            SevenSignsPeriod = 0;
        }
    }
}

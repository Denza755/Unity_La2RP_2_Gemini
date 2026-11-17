using System;
using System.Collections.Generic;
using UnityEngine;

public class ServerBuildingEntry
{
    public int Id;
    public string PrefabName;
    public Vector3 Position;
    public float RotY;
    public float Scale;
    public string OwnerName;
}

public class RulerBuildingsPacket : ServerPacket
{
    public byte Mode { get; private set; }
    public List<ServerBuildingEntry> Buildings { get; private set; } = new List<ServerBuildingEntry>();
    public int DemolishedBuildingId { get; private set; }

    public RulerBuildingsPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        try
        {
            Mode = ReadB();

            if (Mode == 0) // Chunk list of buildings
            {
                int count = ReadH();
                for (int i = 0; i < count; i++)
                {
                    ServerBuildingEntry entry = new ServerBuildingEntry
                    {
                        Id = ReadI(),
                        PrefabName = ReadS(),
                        Position = new Vector3((float)ReadD(), (float)ReadD(), (float)ReadD()),
                        RotY = (float)ReadD(),
                        Scale = (float)ReadD(),
                        OwnerName = ReadS()
                    };
                    Buildings.Add(entry);
                }
            }
            else if (Mode == 1) // Single building added
            {
                ServerBuildingEntry entry = new ServerBuildingEntry
                {
                    Id = ReadI(),
                    PrefabName = ReadS(),
                    Position = new Vector3((float)ReadD(), (float)ReadD(), (float)ReadD()),
                    RotY = (float)ReadD(),
                    Scale = (float)ReadD(),
                    OwnerName = ReadS()
                };
                Buildings.Add(entry);
            }
            else if (Mode == 2) // Single building demolished
            {
                DemolishedBuildingId = ReadI();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[RulerBuildingsPacket] Error parsing packet mode {Mode}: {ex}");
        }
    }
}

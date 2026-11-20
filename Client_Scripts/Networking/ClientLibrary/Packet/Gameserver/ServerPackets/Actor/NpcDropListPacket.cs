using System;

public class NpcDropListPacket : ServerPacket
{
    public int NpcId { get; private set; }
    public string NpcName { get; private set; }
    public int[] ItemIds { get; private set; }

    public NpcDropListPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        try
        {
            NpcId = ReadI();
            NpcName = ReadS();
            int count = ReadI();
            ItemIds = new int[count];
            for (int i = 0; i < count; i++)
            {
                ItemIds[i] = ReadI();
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Error parsing NpcDropListPacket: {ex}");
        }
    }
}

using UnityEngine;

public class MoveToPawnPacket : ServerPacket
{
    public int ObjectId { get; private set; }
    public int TargetId { get; private set; }
    public float Distance { get; private set; }
    public Vector3 Position { get; private set; }

    public MoveToPawnPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        ObjectId = ReadI();
        TargetId = ReadI();
        int distUu = ReadI();
        Distance = distUu / 52.5f;

        Vector3 pos = new Vector3();
        pos.z = ReadI() / 52.5f;
        pos.x = ReadI() / 52.5f;
        pos.y = ReadI() / 52.5f;
        Position = pos;
    }
}

using System.Collections.Generic;

public class RankingListPacket : ServerPacket
{
    public class RankingEntry
    {
        public int Rank;
        public string Name;
        public int ClassId;
        public int Level;
        public long Value;
        public bool IsOnline;
    }

    public int Category { get; private set; }
    public List<RankingEntry> Entries { get; private set; } = new List<RankingEntry>();

    public RankingListPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        Category = ReadI();
        int count = ReadI();

        for (int i = 0; i < count; i++)
        {
            var entry = new RankingEntry();
            entry.Rank = ReadI();
            entry.Name = ReadS();
            entry.ClassId = ReadI();
            entry.Level = ReadI();
            entry.Value = ReadL();
            entry.IsOnline = ReadI() == 1;
            Entries.Add(entry);
        }
    }
}

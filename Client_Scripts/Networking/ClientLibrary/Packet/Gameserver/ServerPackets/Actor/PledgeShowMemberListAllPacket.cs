using System.Collections.Generic;

public class PledgeShowMemberListAllPacket : ServerPacket
{
    public class PledgeMember
    {
        public string Name;
        public int Level;
        public int ClassId;
        public int Sex;
        public int Race;
        public bool IsOnline;
        public bool HasSponsor;
    }

    public int IsSubPledge { get; private set; }
    public int ClanId { get; private set; }
    public int PledgeType { get; private set; }
    public string PledgeName { get; private set; }
    public string LeaderName { get; private set; }
    public int CrestId { get; private set; }
    public int ClanLevel { get; private set; }
    public int CastleId { get; private set; }
    public int ClanHallId { get; private set; }
    public int Rank { get; private set; }
    public int ReputationScore { get; private set; }
    public int DissolvingExpiryTime { get; private set; }
    public int AllyId { get; private set; }
    public string AllyName { get; private set; }
    public int AllyCrestId { get; private set; }
    public bool IsAtWar { get; private set; }
    public List<PledgeMember> Members { get; private set; } = new List<PledgeMember>();

    public PledgeShowMemberListAllPacket(byte[] d) : base(d)
    {
        Parse();
    }

    public override void Parse()
    {
        IsSubPledge = ReadI();
        ClanId = ReadI();
        PledgeType = ReadI();
        PledgeName = ReadS();
        LeaderName = ReadS();

        CrestId = ReadI();
        ClanLevel = ReadI();
        CastleId = ReadI();
        ClanHallId = ReadI();
        Rank = ReadI();
        ReputationScore = ReadI();
        DissolvingExpiryTime = ReadI();
        ReadI(); // 0
        AllyId = ReadI();
        AllyName = ReadS();
        AllyCrestId = ReadI();
        IsAtWar = ReadI() == 1;

        int memberCount = ReadI();
        for (int i = 0; i < memberCount; i++)
        {
            var member = new PledgeMember();
            member.Name = ReadS();
            member.Level = ReadI();
            member.ClassId = ReadI();
            member.Sex = ReadI();
            member.Race = ReadI();
            member.IsOnline = ReadI() != 0;
            member.HasSponsor = ReadI() == 1;
            Members.Add(member);
        }
    }
}

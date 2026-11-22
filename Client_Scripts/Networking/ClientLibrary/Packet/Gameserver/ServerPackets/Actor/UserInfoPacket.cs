using UnityEngine;
using System;

// Sent by server as CharInfo (opcode 0x03) for other players/users in the world.
// Consumed by GameServerPacketHandler.OnUserInfoReceived -> WorldSpawner.OnReceiveUserInfo -> UserSpawner.
public class UserInfoPacket : ServerPacket
{
    public NetworkIdentity Identity { get; private set; }
    public PlayerStatus Status { get; private set; }
    public PlayerStats Stats { get; private set; }
    public PlayerAppearance Appearance { get; private set; }
    public EntityActionInfo EntityActionInfo { get; set; }
    public byte OperateType { get; private set; }

    public UserInfoPacket(byte[] d) : base(d)
    {
        Identity = new NetworkIdentity();
        Status = new PlayerStatus();
        Stats = new PlayerStats();
        Appearance = new PlayerAppearance();
        EntityActionInfo = new EntityActionInfo();
        Parse();
    }

    public override void Parse()
    {
        try
        {
            // Loc X, Y, Z (writeLoc in CharInfo.java)
            Identity.SetPosZ(ReadI() / 52.5f);
            Identity.SetPosX(ReadI() / 52.5f);
            Identity.SetPosY(ReadI() / 52.5f);

            // Boat objectId (writeD((boat == null) ? 0 : boat.getObjectId()))
            ReadI();

            Identity.Id = ReadI();
            Identity.Name = ReadS();
            Appearance.Race = (byte)ReadI();
            Appearance.Sex = (byte)ReadI();
            Identity.PlayerClass = (byte)ReadI();

            // 12 Paperdoll ItemIds (writeD in CharInfo.java):
            ReadI(); // HAIRALL
            ReadI(); // HEAD
            Appearance.RHand = ReadI(); // RHAND
            Appearance.LHand = ReadI(); // LHAND
            Appearance.Gloves = ReadI(); // GLOVES
            Appearance.Chest = ReadI(); // CHEST
            Appearance.Legs = ReadI(); // LEGS
            Appearance.Feet = ReadI(); // FEET
            ReadI(); // CLOAK
            int rhand2 = ReadI(); // RHAND2 (e.g. dual or 2H)
            if (rhand2 != 0) Appearance.RHand = rhand2;
            ReadI(); // HAIR
            ReadI(); // FACE

            // Augmentations & padding in CharInfo.java:
            ReadH(); ReadH(); ReadH(); ReadH(); // 4 shorts
            ReadI(); // AugmentationId RHAND
            for (int i = 0; i < 12; i++) ReadH(); // 12 shorts
            ReadI(); // AugmentationId LHAND
            ReadH(); ReadH(); ReadH(); ReadH(); // 4 shorts

            Identity.PvpFlag = ReadI();
            Stats.Karma = ReadI();
            Stats.MAtkSpd = ReadI();
            Stats.PAtkSpd = ReadI();
            ReadI(); // PvpFlag2
            ReadI(); // Karma2

            // Speeds:
            Stats.RunSpeed = ReadI();
            Stats.WalkSpeed = ReadI();
            ReadI(); // SwimRunSpeed
            ReadI(); // SwimWalkSpeed
            ReadI(); // RunSpeed2
            ReadI(); // WalkSpeed2
            ReadI(); // FlyRunSpeed
            ReadI(); // FlyWalkSpeed

            Stats.MoveSpeedMultiplier = (float)ReadD();
            Stats.AttackSpeedMultiplier = (float)ReadD();

            Appearance.CollisionRadius = (float)ReadD() / 52.5f;
            Appearance.CollisionHeight = (float)ReadD() / 52.5f;

            Appearance.HairStyle = (byte)ReadI();
            Appearance.HairColor = (byte)ReadI();
            Appearance.Face = (byte)ReadI();

            Identity.Title = ReadS();

            ReadI(); // ClanId
            ReadI(); // ClanCrestId
            ReadI(); // AllyId
            ReadI(); // AllyCrestId
            ReadI(); // Relation (0)

            // Actions (5 bytes written in CharInfo.java):
            EntityActionInfo.Sitting = ReadB() == 0;
            EntityActionInfo.Running = ReadB() == 1;
            EntityActionInfo.InCombat = ReadB() == 1;
            EntityActionInfo.AlikeDead = ReadB() == 1;
            EntityActionInfo.Invisible = ReadB() == 1;

            ReadB(); // MountType
            OperateType = (byte)ReadB(); // OperateType

            int cubicCount = ReadH();
            for (int i = 0; i < cubicCount; i++)
            {
                ReadH(); // cubic id
            }

            ReadB(); // IsInPartyMatchRoom
            ReadI(); // AbnormalEffect
            ReadB(); // RecomLeft
            ReadH(); // RecomHave
            Identity.PlayerClass = (byte)ReadI(); // ClassId

            Stats.MaxCp = ReadI();
            Status.Cp = ReadI();

            ReadB(); // EnchantEffect
            ReadB(); // TeamId
            ReadI(); // ClanCrestLargeId
            ReadB(); // IsNoble
            ReadB(); // Hero/GM Aura
            ReadB(); // IsFishing

            ReadI(); // Fishing Loc X
            ReadI(); // Fishing Loc Y
            ReadI(); // Fishing Loc Z

            Appearance.ServerNameColor = ReadI();
            Identity.Heading = ReadI();
            ReadI(); // PledgeClass
            ReadI(); // PledgeType
            Appearance.ServerTitleColor = ReadI();
            ReadI(); // CursedWeaponStage
            Stats.AttackRange = ReadI() / 52.5f;

            Identity.IsMage = CharacterClassParser.IsMage((CharacterClass)Identity.PlayerClass);

            Stats.RunSpeed = (int)(Stats.MoveSpeedMultiplier > 0 ? Stats.RunSpeed * Stats.MoveSpeedMultiplier : Stats.RunSpeed);
            Stats.WalkSpeed = (int)(Stats.MoveSpeedMultiplier > 0 ? Stats.WalkSpeed * Stats.MoveSpeedMultiplier : Stats.WalkSpeed);

            Debug.Log(ToString());
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public override string ToString()
    {
        return $"UserInfoPacket: {{ " +
               $"Identity: {{ ID: {Identity.Id}, Position: {Identity.Position},\n " +
               $"  Name: {Identity.Name}\n" +
               $"  NameColor: {Appearance.ServerNameColor}\n" +
               $"  Title: {Identity.Title}\n" +
               $"  TitleColor: {Appearance.ServerTitleColor}\n" +
               $"  FlagTime: {Identity.PvpFlag}\n" +
               $"Class: {Identity.PlayerClass}, IsMage: {Identity.IsMage}, Heading: {Identity.Heading} }}, " +
               $"Status: {{ CP: {Status.Cp} }}, " +
               $"Stats: {{ Karma: {Stats.Karma}, PAtkSpd: {Stats.PAtkSpd}, MAtkSpd: {Stats.MAtkSpd}, RunSpeed: {Stats.RunSpeed}, " +
               $"WalkSpeed: {Stats.WalkSpeed}, MoveSpeedMultiplier: {Stats.MoveSpeedMultiplier}, AttackSpeedMultiplier: {Stats.AttackSpeedMultiplier}, " +
               $"MaxCp: {Stats.MaxCp}, AttackRange: {Stats.AttackRange} }}, " +
               $"Appearance: {{ Race: {Appearance.Race}, Sex: {Appearance.Sex}, HairStyle: {Appearance.HairStyle}, HairColor: {Appearance.HairColor}, " +
               $"Face: {Appearance.Face}, CollisionRadius: {Appearance.CollisionRadius}, CollisionHeight: {Appearance.CollisionHeight}, " +
               $"RHand: {Appearance.RHand}, LHand: {Appearance.LHand}, Gloves: {Appearance.Gloves}, Chest: {Appearance.Chest}, " +
               $"Legs: {Appearance.Legs}, Feet: {Appearance.Feet} }}, " +
               $"Running: {EntityActionInfo.Running}, Sitting: {EntityActionInfo.Sitting}, InCombat: {EntityActionInfo.InCombat}, AlikeDead: {EntityActionInfo.AlikeDead}, Invisible: {EntityActionInfo.Invisible} " +
               $"}}";
    }
}

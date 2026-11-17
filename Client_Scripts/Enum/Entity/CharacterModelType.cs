using System;
using System.Collections.Generic;

public enum CharacterModelType : byte
{
    FElf = 0,
    MElf = 1,
    MDarkElf = 2,
    FDarkElf = 3,
    FDwarf = 4,
    MDwarf = 5,
    MShaman = 6,
    FShaman = 7,
    MOrc = 8,
    FOrc = 9,
    FMagic = 10,
    MMagic = 11,
    FFighter = 12,
    MFighter = 13
}

public class CharacterModelHelper
{
    private static readonly Dictionary<CharacterModelType, float> modelColHeights = new()
    {
        { CharacterModelType.FElf, 0.438f },
        { CharacterModelType.MElf, 0.457f },
        { CharacterModelType.MDarkElf, 0.457f },
        { CharacterModelType.FDarkElf, 0.447f },
        { CharacterModelType.FDwarf, 0.361f },
        { CharacterModelType.MDwarf, 0.342f },
        { CharacterModelType.MShaman, 0.523f },
        { CharacterModelType.FShaman, 0.485f },
        { CharacterModelType.MOrc, 0.533f },
        { CharacterModelType.FOrc, 0.514f },
        { CharacterModelType.FMagic, 0.428f },
        { CharacterModelType.MMagic, 0.434f },
        { CharacterModelType.FFighter, 0.447f },
        { CharacterModelType.MFighter, 0.438f }
    };

    public static float GetColHeight(CharacterModelType modelType)
    {
        return modelColHeights.TryGetValue(modelType, out float height) ? height : 0.438f;
    }

    private static readonly Dictionary<CharacterModelType, float> modelColRadius = new()
    {
        { CharacterModelType.FElf, 0.143f },     // 7.5 / 52.5
        { CharacterModelType.MElf, 0.143f },     // 7.5 / 52.5
        { CharacterModelType.MDarkElf, 0.143f }, // 7.5 / 52.5
        { CharacterModelType.FDarkElf, 0.143f }, // 7.5 / 52.5
        { CharacterModelType.FDwarf, 0.152f },   // 8.0 / 52.5
        { CharacterModelType.MDwarf, 0.171f },   // 9.0 / 52.5
        { CharacterModelType.MShaman, 0.133f },  // 7.0 / 52.5
        { CharacterModelType.FShaman, 0.152f },  // 8.0 / 52.5
        { CharacterModelType.MOrc, 0.210f },     // 11.0 / 52.5
        { CharacterModelType.FOrc, 0.133f },     // 7.0 / 52.5
        { CharacterModelType.FMagic, 0.143f },   // 7.5 / 52.5
        { CharacterModelType.MMagic, 0.143f },   // 7.5 / 52.5
        { CharacterModelType.FFighter, 0.152f }, // 8.0 / 52.5
        { CharacterModelType.MFighter, 0.171f }  // 9.0 / 52.5
    };

    public static float GetColRadius(CharacterModelType modelType)
    {
        return modelColRadius.TryGetValue(modelType, out float radius) ? radius : 0.171f;
    }

    /// <summary>
    /// Exact scale factors calculated so Orc 3D models strictly match their server collisionHeight
    /// relative to MFighter baseline (height 23.0 -> 1.1672m):
    /// MOrc (28.0) -> target 1.4210m / raw 1.3370m = 1.063f (strictly 1.217x MFighter)
    /// FOrc (27.0) -> target 1.3702m / raw 1.2286m = 1.115f (strictly 1.174x MFighter)
    /// MShaman (27.5) -> target 1.3956m / raw 1.2701m = 1.099f (strictly 1.196x MFighter)
    /// FShaman (25.5) -> target 1.2941m / raw 1.1919m = 1.086f (strictly 1.109x MFighter)
    /// </summary>
    public static float GetOrcScaleFactor(CharacterModelType modelType)
    {
        switch (modelType)
        {
            case CharacterModelType.MOrc:
                //return 1.063f;
		return 0.763f;
            case CharacterModelType.FOrc:
                //return 1.115f;
		return 0.800f;
            case CharacterModelType.MShaman:
                //return 1.099f;
                return 0.789f;
            case CharacterModelType.FShaman:
                //return 1.086f;
		return 0.780f;
            default:
                //return 1.0f;
		return 1.0f;
        }
    }

    public static float GetControllerHeight(CharacterModelType modelType)
    {
        switch (modelType)
        {
            case CharacterModelType.MOrc:
                return 0.791f; // 0.65 * (28.0 / 23.0)
            case CharacterModelType.FOrc:
                return 0.763f; // 0.65 * (27.0 / 23.0)
            case CharacterModelType.MShaman:
                return 0.777f; // 0.65 * (27.5 / 23.0)
            case CharacterModelType.FShaman:
                return 0.721f; // 0.65 * (25.5 / 23.0)
            default:
                return 0.65f;
        }
    }

    public static float GetControllerRadius(CharacterModelType modelType)
    {
        switch (modelType)
        {
            case CharacterModelType.MOrc:
                return 0.12f;
            case CharacterModelType.FOrc:
                return 0.08f;
            case CharacterModelType.MShaman:
                return 0.08f;
            case CharacterModelType.FShaman:
                return 0.09f;
            default:
                return 0.10f;
        }
    }
}

public static class CharacterModelTypeParser
{
    public static CharacterModelType ParseRace(CharacterRace race, byte sex, bool isMage)
    {
        switch (race)
        {
            case CharacterRace.Elf:
                return sex == 0 ? CharacterModelType.MElf : CharacterModelType.FElf;
            case CharacterRace.DarkElf:
                return sex == 0 ? CharacterModelType.MDarkElf : CharacterModelType.FDarkElf;
            case CharacterRace.Dwarf:
                return sex == 0 ? CharacterModelType.MDwarf : CharacterModelType.FDwarf;
            case CharacterRace.Orc:
                if (isMage)
                {
                    return sex == 0 ? CharacterModelType.MShaman : CharacterModelType.FShaman;
                }
                else
                {
                    return sex == 0 ? CharacterModelType.MOrc : CharacterModelType.FOrc;
                }
            case CharacterRace.Human:
                if (isMage)
                {
                    return sex == 0 ? CharacterModelType.MMagic : CharacterModelType.FMagic;
                }
                else
                {
                    return sex == 0 ? CharacterModelType.MFighter : CharacterModelType.FFighter;
                }
            default:
                return CharacterModelType.FDwarf;
        }
    }

    public static CharacterModelType ParseRace(string value)
    {
        return (CharacterModelType)Enum.Parse(typeof(CharacterModelType), value, ignoreCase: true);
    }
}

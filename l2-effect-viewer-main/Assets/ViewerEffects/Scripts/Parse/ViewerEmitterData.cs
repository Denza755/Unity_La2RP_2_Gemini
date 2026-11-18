using System;
using System.Collections.Generic;
using UnityEngine;

public enum ViewerEmitterKind
{
    Sprite,
    Mesh,
    Beam,
    Spark,
    Trail,
    Unknown
}

[Serializable]
public struct ViewerRange
{
    public float Min;
    public float Max;

    public static ViewerRange One => new ViewerRange { Min = 1f, Max = 1f };
    public static ViewerRange Zero => new ViewerRange { Min = 0f, Max = 0f };

    public ViewerRange(float min, float max)
    {
        Min = min;
        Max = max;
    }

    public Vector2 ToVector2() => new Vector2(Min, Max);
}

[Serializable]
public struct ViewerColorKey
{
    public float RelativeTime;
    public Color Color;
}

[Serializable]
public struct ViewerSizeKey
{
    public float RelativeTime;
    public float RelativeSize;
}

[Serializable]
public class ViewerEmitterData
{
    public ViewerEmitterKind Kind;
    public string ObjectName;
    public string DisplayName;
    public string StaticMesh;
    public string Texture;
    public int MaxParticles = 1;
    public float InitialParticlesPerSecond = 0f;
    public bool AutomaticInitialSpawning = true;
    public bool RespawnDeadParticles = true;
    public ViewerRange Lifetime = new ViewerRange(1f, 1f);
    public ViewerRange InitialDelay;
    public ViewerRange StartSizeX = ViewerRange.One;
    public ViewerRange StartSizeY = ViewerRange.One;
    public ViewerRange StartSizeZ = ViewerRange.One;
    public bool UniformSize;
    public bool UseSizeScale;
    public bool UseRegularSizeScale = true;
    public float SizeScaleRepeats;
    public List<ViewerSizeKey> SizeScale = new List<ViewerSizeKey>();
    public ViewerRange StartLocationX;
    public ViewerRange StartLocationY;
    public ViewerRange StartLocationZ;
    public Vector3 StartLocationOffset;
    public string StartLocationShape = "";
    public ViewerRange PolarTheta;
    public ViewerRange PolarPhi;
    public ViewerRange PolarRadius;
    public ViewerRange StartVelocityX;
    public ViewerRange StartVelocityY;
    public ViewerRange StartVelocityZ;
    public Vector3 Acceleration;
    public string GetVelocityDirectionFrom = "";
    /// <summary>UC UseDirectionAs (e.g. PTDU_Normal, PTDU_Up). Empty = face camera.</summary>
    public string UseDirectionAs = "";
    public bool SpinParticles;
    public ViewerRange StartSpinX;
    public ViewerRange StartSpinY;
    public ViewerRange StartSpinZ;
    public ViewerRange SpinsPerSecondX;
    public ViewerRange SpinsPerSecondY;
    public ViewerRange SpinsPerSecondZ;
    public Vector3 SpinCCWorCW = Vector3.one;
    public string UseRotationFrom = "";
    public bool FadeIn;
    public bool FadeOut;
    public float FadeInEndTime;
    public float FadeOutStartTime;
    public float Opacity = 1f;
    public Vector3 ColorMulMin = Vector3.one;
    public Vector3 ColorMulMax = Vector3.one;
    public bool UseColorScale;
    public float ColorScaleRepeats;
    public List<ViewerColorKey> ColorScale = new List<ViewerColorKey>();
    public string DrawStyle = "PTDS_Translucent";
    public bool RenderTwoSided = true;
    public int TextureUSubdivisions = 1;
    public int TextureVSubdivisions = 1;
    public int SubdivisionStart;
    public int SubdivisionEnd;
    public bool UseRandomSubdivision;
    public bool BlendBetweenSubdivisions;
    public float SpawnDeltaTime = 0.012f;

    // BeamEmitter (MVP: PTEP_Offset + LF/HF strip, no noise/branching yet).
    public string DetermineEndPointBy = "PTEP_Offset";
    public int LowFrequencyPoints = 3;
    public int HighFrequencyPoints = 10;
    public float BeamTextureUScale = 1f;
    public float BeamTextureVScale = 1f;
    public ViewerRange BeamEndOffsetX;
    public ViewerRange BeamEndOffsetY;
    public ViewerRange BeamEndOffsetZ;
    public ViewerRange BeamDistanceRange;

    public bool IsPolar =>
        StartLocationShape.IndexOf("Polar", StringComparison.OrdinalIgnoreCase) >= 0;

    public bool IsBeamOffsetEnd =>
        DetermineEndPointBy.IndexOf("Offset", StringComparison.OrdinalIgnoreCase) >= 0 &&
        DetermineEndPointBy.IndexOf("Absolute", StringComparison.OrdinalIgnoreCase) < 0 &&
        DetermineEndPointBy.IndexOf("Trace", StringComparison.OrdinalIgnoreCase) < 0;

    public bool IsBurst =>
        !AutomaticInitialSpawning || InitialParticlesPerSecond >= 1000f;

    public float MaxLifetime => Mathf.Max(Lifetime.Max, Lifetime.Min, 0.05f);
}

[Serializable]
public class ViewerEffectData
{
    public string ClassName;
    public string SourcePath;
    public float DrawScale = 1f;
    public List<ViewerEmitterData> Emitters = new List<ViewerEmitterData>();
}

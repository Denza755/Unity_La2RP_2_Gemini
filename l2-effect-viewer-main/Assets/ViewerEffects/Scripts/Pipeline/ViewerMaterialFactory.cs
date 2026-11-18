using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public static class ViewerMaterialFactory
{
    public const string MeshShaderName = "L2/Viewer/MeshEmitter";
    public const string SpriteShaderName = "L2/Viewer/SpriteEmitter";
    public const string BeamShaderName = "L2/Viewer/BeamEmitter";

    static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
    static readonly int DstBlend = Shader.PropertyToID("_DstBlend");

    public static Material Create(ViewerEffectData effect, ViewerEmitterData emitter, Texture texture)
    {
        string shaderName = emitter.Kind switch
        {
            ViewerEmitterKind.Mesh => MeshShaderName,
            ViewerEmitterKind.Beam => BeamShaderName,
            _ => SpriteShaderName
        };
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
            throw new InvalidOperationException("Shader not found: " + shaderName + ". Wait for Unity import of ViewerEffects/Shaders.");

        var mat = new Material(shader)
        {
            name = emitter.DisplayName + "_mat",
            hideFlags = HideFlags.DontSave
        };
        mat.enableInstancing = false;
        if (texture != null)
        {
            mat.SetTexture("_MainTex", texture);
            mat.mainTexture = texture;
        }
        else
        {
            mat.SetTexture("_MainTex", Texture2D.whiteTexture);
            mat.mainTexture = Texture2D.whiteTexture;
        }

        ApplyDrawStyle(mat, emitter.DrawStyle);
        float worldCalibration = emitter.Kind switch
        {
            ViewerEmitterKind.Mesh => ViewerPaths.Config.worldCalibrationMesh,
            ViewerEmitterKind.Beam => ViewerPaths.Config.worldCalibrationBeam,
            _ => ViewerPaths.Config.worldCalibrationSprite
        };
        mat.SetFloat("_L2FxWorldCalibration", worldCalibration);
        mat.SetVector("_LifetimeRange", Vec(emitter.Lifetime));
        mat.SetVector("_InitialDelayRange", Vec(emitter.InitialDelay));
        mat.SetFloat("_UseSizeScale", emitter.UseSizeScale ? 1f : 0f);
        mat.SetFloat("_SizeScaleRepeats", emitter.SizeScaleRepeats);
        SetSizeKeys(mat, emitter);
        mat.SetFloat("_FadeIn", emitter.FadeIn ? 1f : 0f);
        mat.SetFloat("_FadeInEndTime", emitter.FadeInEndTime);
        mat.SetFloat("_FadeOut", emitter.FadeOut ? 1f : 0f);
        mat.SetFloat("_FadeOutStartTime", emitter.FadeOutStartTime);
        mat.SetFloat("_Fadeout", emitter.FadeOut ? 1f : 0f);
        mat.SetFloat("_FadeoutStartTime", emitter.FadeOutStartTime);
        mat.SetFloat("_Opacity", emitter.Opacity);
        mat.SetFloat("_L2SpriteColorGammaToLinear", GammaToLinearEnabled(emitter));
        mat.SetFloat("_RgbBoost", 1f);
        mat.renderQueue = 3000;

        if (emitter.Kind == ViewerEmitterKind.Mesh)
            ApplyMesh(mat, emitter);
        else if (emitter.Kind == ViewerEmitterKind.Beam)
        {
            // L2 beam atlases scroll along U; Wrap=Repeat (see heal_ta fx_m_t0004 notes).
            if (texture != null)
            {
                texture.wrapMode = TextureWrapMode.Repeat;
                mat.mainTexture = texture;
                mat.SetTexture("_MainTex", texture);
            }
            ApplyBeam(mat, emitter);
        }
        else
            ApplySprite(mat, emitter);

        bool profileApplied = ApplyValidatedMaterialProfile(effect, emitter, mat);
        ViewerDiagnostics.Material(effect, emitter, mat, profileApplied);
        return mat;
    }

    static float GammaToLinearEnabled(ViewerEmitterData _)
    {
        // Default ON: FX atlases load Linear (sRGB OFF), then ColorMul/ColorScale
        // stay L2 display-gamma numbers. Decompile_Common applies pow(rgb, 2)
        // after ColorFade. Verified path for kirakira / Wave.
        return 1f;
    }

    static bool ApplyValidatedMaterialProfile(
        ViewerEffectData effect,
        ViewerEmitterData emitter,
        Material mat)
    {
        if (effect == null ||
            !string.Equals(effect.ClassName, "it_healing_potion_ta", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Exact values from the working reference materials under
        // Resources/Data/Effects/it_healing_potion/it_healing_potion_ta.
        // These are Unity material settings not represented by the UC file;
        // no Decompile_Common formula is replaced or reimplemented here.
        if (string.Equals(emitter.ObjectName, "MeshEmitter2", StringComparison.OrdinalIgnoreCase))
        {
            mat.SetFloat("_L2SpriteColorGammaToLinear", 1f);
            mat.SetFloat("_Opacity", 0.6f);
            return true;
        }
        if (string.Equals(emitter.ObjectName, "MeshEmitter0", StringComparison.OrdinalIgnoreCase))
        {
            mat.SetFloat("_L2SpriteColorGammaToLinear", 1f);
            mat.SetFloat("_RgbBoost", 1f);
            return true;
        }
        if (string.Equals(emitter.ObjectName, "SpriteEmitter7", StringComparison.OrdinalIgnoreCase))
        {
            mat.SetFloat("_L2SpriteColorGammaToLinear", 1f);
            mat.SetFloat("_RgbBoost", 1f);
            // Global sprite K is already 1.1 (ViewerConfig); keep explicit for this profile.
            mat.SetFloat("_L2FxWorldCalibration", 1.1f);
            mat.SetFloat("_SubdivisionStart", 7f);
            mat.SetFloat("_SubdivisionEnd", 8f);
            // The working material has ColorKey0.a=1 even though the raw
            // umodel UC dump explicitly serialized A=0. Its verified shader
            // also calls SpriteColorFade with alphaBlend=1.
            mat.SetColor("_ColorKey0", Color.white);
            mat.SetFloat("_ColorFadeAlphaBlend", 1f);
            return true;
        }

        return false;
    }

    // PTDS_DrawStyle_Reference F90D4F0 + L2FxPTDS_DrawStyle.hlsl.
    // Unity BlendMode: Zero=0 One=1 DstColor=2 SrcColor=3 OneMinusDstColor=4
    // SrcAlpha=5 OneMinusSrcColor=6 OneMinusSrcAlpha=7
    static int ParsePtds(string style)
    {
        if (HasIdent(style, "AlphaModulate")) return 4;
        if (HasIdent(style, "AlphaBlend")) return 1;
        if (HasIdent(style, "Modulated")) return 2;
        if (HasIdent(style, "Translucent")) return 3;
        if (HasIdent(style, "Darken")) return 5;
        if (HasIdent(style, "Brighten")) return 6;
        if (HasIdent(style, "Regular")) return 0;
        // UE2 ParticleEmitter defaultproperties: DrawStyle=PTDS_Translucent.
        // Live client shaders: omitted DrawStyle → Blend One One (not Regular).
        return 3;
    }

    static bool HasIdent(string value, string ident)
    {
        return !string.IsNullOrEmpty(value) &&
               value.IndexOf(ident, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static void ApplyMesh(Material mat, ViewerEmitterData e)
    {
        float sizeXY = 0.5f * (e.StartSizeX.Min + e.StartSizeX.Max);
        mat.SetFloat("_StartSizeXY", sizeXY);
        mat.SetFloat("_StartSize", sizeXY);
        mat.SetVector("_StartSizeZRange", Vec(e.StartSizeZ));
        mat.SetVector("_StartLocationZRangeUU", Vec(e.StartLocationZ));
        mat.SetVector("_StartVelocityZRangeUU", Vec(e.StartVelocityZ));
        mat.SetFloat("_UsePtrsActor", e.UseRotationFrom.IndexOf("PTRS_Actor", StringComparison.OrdinalIgnoreCase) >= 0 ? 1f : 0f);
        mat.SetFloat("_SpinParticles", e.SpinParticles ? 1f : 0f);
        mat.SetVector("_StartSpinYawRangeUc", Vec(e.StartSpinX));
        mat.SetVector("_StartSpinPitchRangeUc", Vec(e.StartSpinY));
        mat.SetVector("_StartSpinRollRangeUc", Vec(e.StartSpinZ));
        mat.SetVector("_SpsYawPitchRollUc", new Vector4(Mid(e.SpinsPerSecondX), Mid(e.SpinsPerSecondY), Mid(e.SpinsPerSecondZ), 0f));
        mat.SetVector("_SpinCCWorCW", new Vector4(e.SpinCCWorCW.x, e.SpinCCWorCW.y, e.SpinCCWorCW.z, 0f));
        mat.SetVector("_ColorMultiplier", new Vector4(e.ColorMulMin.x, e.ColorMulMin.y, e.ColorMulMin.z, 0f));
        SetColorKeys(mat, e);
        mat.SetInt("_Cull", e.RenderTwoSided ? (int)CullMode.Off : (int)CullMode.Back);
    }

    static void ApplySprite(Material mat, ViewerEmitterData e)
    {
        mat.SetVector("_SizeRange", Vec(e.StartSizeX));
        mat.SetVector("_StartLocationRangeUU", new Vector4(
            Math.Max(Math.Abs(e.StartLocationX.Min), Math.Abs(e.StartLocationX.Max)),
            Math.Max(Math.Abs(e.StartLocationY.Min), Math.Abs(e.StartLocationY.Max)),
            Math.Max(Math.Abs(e.StartLocationZ.Min), Math.Abs(e.StartLocationZ.Max)), 0f));
        mat.SetVector("_StartLocationRangeXUc", Vec(e.StartLocationX));
        mat.SetVector("_StartLocationRangeYUc", Vec(e.StartLocationY));
        mat.SetVector("_StartLocationRangeZUc", Vec(e.StartLocationZ));
        mat.SetFloat("_UsePolar", e.IsPolar ? 1f : 0f);
        mat.SetVector("_PolarThetaRangeUc", Vec(e.PolarTheta));
        mat.SetVector("_PolarPhiRangeUc", Vec(e.PolarPhi));
        mat.SetVector("_PolarRadiusRangeUc", Vec(e.PolarRadius));
        mat.SetVector("_StartVelocityRangeXUc", Vec(e.StartVelocityX));
        mat.SetVector("_StartVelocityRangeYUc", Vec(e.StartVelocityY));
        mat.SetVector("_StartVelocityRangeZUc", Vec(e.StartVelocityZ));
        mat.SetVector("_AccelerationUc", new Vector4(e.Acceleration.x, e.Acceleration.y, e.Acceleration.z, 0f));
        mat.SetFloat("_SpawnDeltaTime", e.SpawnDeltaTime);
        mat.SetFloat("_PtvdMode", PtvdMode(e.GetVelocityDirectionFrom));
        mat.SetFloat("_PtduDirectionMode", PtduDirectionMode(e.UseDirectionAs));
        mat.SetVector("_SpriteSpinStartRangeUc", Vec(e.StartSpinX));
        mat.SetVector("_SpriteSpinSpsRangeUc", Vec(e.SpinsPerSecondX));
        mat.SetVector("_SpriteSpinCcwOrCw", new Vector4(e.SpinCCWorCW.x, e.SpinCCWorCW.y, e.SpinCCWorCW.z, 0f));
        mat.SetFloat("_ColorScaleCount", e.ColorScale.Count);
        mat.SetFloat("_ColorScaleParam", e.ColorScaleRepeats);
        SetColorKeys(mat, e);
        mat.SetVector("_ColorMulMin", new Vector4(e.ColorMulMin.x, e.ColorMulMin.y, e.ColorMulMin.z, 0f));
        mat.SetVector("_ColorMulMax", new Vector4(e.ColorMulMax.x, e.ColorMulMax.y, e.ColorMulMax.z, 0f));
        mat.SetFloat("_TextureUSubdivisions", Mathf.Max(1, e.TextureUSubdivisions));
        mat.SetFloat("_TextureVSubdivisions", Mathf.Max(1, e.TextureVSubdivisions));
        mat.SetFloat("_SubdivisionStart", e.SubdivisionStart);
        mat.SetFloat("_SubdivisionEnd", e.SubdivisionEnd);
        mat.SetFloat("_UseRandomSubdivision", e.UseRandomSubdivision ? 1f : 0f);
        mat.SetFloat("_BlendBetweenSubdivisions", e.BlendBetweenSubdivisions ? 1f : 0f);
    }

    static void ApplyBeam(Material mat, ViewerEmitterData e)
    {
        mat.SetVector("_SizeRange", Vec(e.StartSizeX));
        mat.SetVector("_StartLocationOffsetUc", new Vector4(
            e.StartLocationOffset.x, e.StartLocationOffset.y, e.StartLocationOffset.z, 0f));
        mat.SetVector("_StartLocationRangeXUc", Vec(e.StartLocationX));
        mat.SetVector("_StartLocationRangeYUc", Vec(e.StartLocationY));
        mat.SetVector("_StartLocationRangeZUc", Vec(e.StartLocationZ));
        mat.SetFloat("_UsePolar", e.IsPolar ? 1f : 0f);
        mat.SetVector("_PolarThetaRangeUc", Vec(e.PolarTheta));
        mat.SetVector("_PolarPhiRangeUc", Vec(e.PolarPhi));
        mat.SetVector("_PolarRadiusRangeUc", Vec(e.PolarRadius));
        mat.SetVector("_BeamEndOffsetXUc", Vec(e.BeamEndOffsetX));
        mat.SetVector("_BeamEndOffsetYUc", Vec(e.BeamEndOffsetY));
        mat.SetVector("_BeamEndOffsetZUc", Vec(e.BeamEndOffsetZ));
        mat.SetVector("_BeamDistanceRangeUc", Vec(e.BeamDistanceRange));
        mat.SetFloat("_BeamEndpointMode", BeamEndpointMode(e.DetermineEndPointBy));
        mat.SetFloat("_ColorScaleCount", e.ColorScale.Count);
        mat.SetFloat("_ColorScaleParam", e.ColorScaleRepeats);
        SetColorKeys(mat, e);
        mat.SetVector("_ColorMulMin", new Vector4(e.ColorMulMin.x, e.ColorMulMin.y, e.ColorMulMin.z, 0f));
        mat.SetVector("_ColorMulMax", new Vector4(e.ColorMulMax.x, e.ColorMulMax.y, e.ColorMulMax.z, 0f));

        // Live UBeamEmitter::UpdateParticles writes its final additive draw RGB directly
        // to runtimeColorA8 after ColorScale * ColorMultiplier, Fade and Opacity.
        // wh_heal_ta proof (slot 0, t=0.2899):
        //   runtime BGRA=(15,15,15,147), exactly the raw formula result.
        // Applying GammaToLinear after that turns 15/255 into ~1/255 and makes the
        // beam appear abruptly near its peak. D3D9 PTDS_Translucent consumes the
        // runtime bytes as additive contributions, so preserve them for Beam.
        if (ParsePtds(e.DrawStyle) == 3)
            mat.SetFloat("_L2SpriteColorGammaToLinear", 0f);
    }

    static float BeamEndpointMode(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0f;
        if (value.IndexOf("OffsetAsAbsolute", StringComparison.OrdinalIgnoreCase) >= 0)
            return 2f;
        if (value.IndexOf("Distance", StringComparison.OrdinalIgnoreCase) >= 0)
            return 1f;
        // PTEP_Offset (and TraceOffset falls back to offset for MVP).
        return 0f;
    }

    static void SetSizeKeys(Material mat, ViewerEmitterData e)
    {
        for (int i = 0; i < 5; i++)
        {
            float t = 1f;
            float s = 1f;
            if (i < e.SizeScale.Count)
            {
                t = e.SizeScale[i].RelativeTime;
                s = e.SizeScale[i].RelativeSize;
            }
            else if (e.SizeScale.Count > 0)
            {
                t = e.SizeScale[e.SizeScale.Count - 1].RelativeTime;
                s = e.SizeScale[e.SizeScale.Count - 1].RelativeSize;
            }
            mat.SetVector("_SizeKey" + i, new Vector4(t, s, 0f, 0f));
        }
    }

    static void SetColorKeys(Material mat, ViewerEmitterData e)
    {
        Color c0 = e.ColorScale.Count > 0 ? e.ColorScale[0].Color : Color.white;
        mat.SetColor("_ColorKey0", c0);
        for (int i = 1; i <= 4; i++)
        {
            Color c = c0;
            float t = 1f;
            if (i < e.ColorScale.Count)
            {
                c = e.ColorScale[i].Color;
                t = e.ColorScale[i].RelativeTime;
            }
            else if (e.ColorScale.Count > 0)
            {
                c = e.ColorScale[e.ColorScale.Count - 1].Color;
                t = e.ColorScale[e.ColorScale.Count - 1].RelativeTime;
            }
            mat.SetColor("_ColorKey" + i, c);
            mat.SetFloat("_ColorKey" + i + "Time", t);
        }
    }

    static void ApplyDrawStyle(Material mat, string style)
    {
        // Source: Decompile_Common/PTDS_DrawStyle_Reference/README.txt (F90D4F0).
        // 0 Regular      Blend One Zero
        // 1 AlphaBlend   Blend SrcAlpha OneMinusSrcAlpha
        // 2 Modulated    Blend DstColor SrcColor
        // 3 Translucent  Blend One One
        // 4 AlphaMod     Blend One OneMinusSrcAlpha
        // 5 Darken       Blend Zero OneMinusSrcColor
        // 6 Brighten     Blend One OneMinusSrcColor
        int ptds = ParsePtds(style);
        int src = 1;
        int dst = 0;
        switch (ptds)
        {
            case 1: src = 5; dst = 7; break;
            case 2: src = 2; dst = 3; break;
            case 3: src = 1; dst = 1; break;
            case 4: src = 1; dst = 7; break;
            case 5: src = 0; dst = 6; break;
            case 6: src = 1; dst = 6; break;
            default: src = 1; dst = 0; break;
        }
        mat.SetFloat(SrcBlend, src);
        mat.SetFloat(DstBlend, dst);
        mat.SetInt(SrcBlend, src);
        mat.SetInt(DstBlend, dst);
        mat.SetInt("_ZWrite", 0);
        mat.SetOverrideTag("RenderType", "Transparent");
        // SpriteColorFade: blending 1/2/4 scales A; Brighten scales RGB.
        mat.SetFloat("_ColorFadeAlphaBlend", (ptds == 1 || ptds == 2 || ptds == 4) ? 1f : 0f);
    }

    static float PtvdMode(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0f;
        if (value.IndexOf("OwnerAndStartPosition", StringComparison.OrdinalIgnoreCase) >= 0)
            return 2f;
        if (value.IndexOf("StartPositionAndOwner", StringComparison.OrdinalIgnoreCase) >= 0)
            return 1f;
        return 0f;
    }

    // 0 = face camera (default), 1 = PTDU_Normal (quad in particle-normal plane / ground).
    static float PtduDirectionMode(string value)
    {
        if (string.IsNullOrEmpty(value))
            return 0f;
        if (value.IndexOf("PTDU_Normal", StringComparison.OrdinalIgnoreCase) >= 0)
            return 1f;
        return 0f;
    }

    static Vector4 Vec(ViewerRange r) => new Vector4(r.Min, r.Max, 0f, 0f);
    static float Mid(ViewerRange r) => 0.5f * (r.Min + r.Max);

    public static void WriteSidecarMat(string destPath, ViewerEmitterData emitter, string textureFile)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
        using var w = new StreamWriter(destPath);
        w.WriteLine("kind=" + emitter.Kind);
        w.WriteLine("name=" + emitter.DisplayName);
        w.WriteLine("mesh=" + emitter.StaticMesh);
        w.WriteLine("texture=" + emitter.Texture);
        w.WriteLine("textureFile=" + textureFile);
        w.WriteLine("drawStyle=" + emitter.DrawStyle);
        w.WriteLine("ptds=" + ParsePtds(emitter.DrawStyle));
        w.WriteLine("maxParticles=" + emitter.MaxParticles);
        w.WriteLine("lifetime=" + emitter.Lifetime.Min + "," + emitter.Lifetime.Max);
        w.WriteLine("fadeIn=" + emitter.FadeIn + " " + emitter.FadeInEndTime);
        w.WriteLine("fadeOut=" + emitter.FadeOut + " " + emitter.FadeOutStartTime);
        w.WriteLine("opacity=" + emitter.Opacity);
        w.WriteLine("colorMul=" + emitter.ColorMulMin.x + "," + emitter.ColorMulMin.y + "," + emitter.ColorMulMin.z);
        w.WriteLine("gammaToLinear=" + GammaToLinearEnabled(emitter));
        w.WriteLine("sizeScale=" + emitter.UseSizeScale);
        w.WriteLine("spin=" + emitter.SpinParticles);
        w.WriteLine("polar=" + emitter.IsPolar);
        w.WriteLine("useDirectionAs=" + emitter.UseDirectionAs);
        string shader = emitter.Kind switch
        {
            ViewerEmitterKind.Mesh => MeshShaderName,
            ViewerEmitterKind.Beam => BeamShaderName,
            _ => SpriteShaderName
        };
        w.WriteLine("shader=" + shader);
        if (emitter.Kind == ViewerEmitterKind.Beam)
        {
            w.WriteLine("determineEndPointBy=" + emitter.DetermineEndPointBy);
            w.WriteLine("hfPoints=" + emitter.HighFrequencyPoints);
            w.WriteLine("beamEndOffsetZ=" + emitter.BeamEndOffsetZ.Min + "," + emitter.BeamEndOffsetZ.Max);
        }
    }
}

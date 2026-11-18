using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public static class ViewerDiagnostics
{
    static readonly object Sync = new object();
    static string _path;

    public static string LogPath
    {
        get
        {
            if (!string.IsNullOrEmpty(_path))
                return _path;

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                                 ?? Application.persistentDataPath;
            _path = Path.Combine(projectRoot, "Logs", "ViewerEffectsRuntime.log");
            return _path;
        }
    }

    public static void Begin(ViewerEffectData effect)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.WriteAllText(LogPath, string.Empty);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ViewerEffectsDiag] Cannot reset log: " + ex.Message);
        }

        Camera camera = Camera.main;
        Write(
            "BEGIN effect=" + Q(effect?.ClassName) +
            " source=" + Q(effect?.SourcePath) +
            " unity=" + Application.unityVersion +
            " colorSpace=" + QualitySettings.activeColorSpace +
            " graphics=" + SystemInfo.graphicsDeviceType +
            " camera=" + Q(camera != null ? camera.name : null) +
            " hdr=" + (camera != null && camera.allowHDR) +
            " log=" + LogPath);
    }

    public static void Emitter(ViewerEmitterData e)
    {
        var text = new StringBuilder();
        text.Append("EMITTER object=").Append(Q(e.ObjectName))
            .Append(" display=").Append(Q(e.DisplayName))
            .Append(" kind=").Append(e.Kind)
            .Append(" mesh=").Append(Q(e.StaticMesh))
            .Append(" texture=").Append(Q(e.Texture))
            .Append(" drawStyle=").Append(Q(e.DrawStyle))
            .Append(" max=").Append(e.MaxParticles)
            .Append(" rate=").Append(F(e.InitialParticlesPerSecond))
            .Append(" autoInitial=").Append(e.AutomaticInitialSpawning)
            .Append(" respawn=").Append(e.RespawnDeadParticles)
            .Append(" lifetime=").Append(R(e.Lifetime))
            .Append(" opacity=").Append(F(e.Opacity))
            .Append(" colorMin=").Append(V(e.ColorMulMin))
            .Append(" colorMax=").Append(V(e.ColorMulMax))
            .Append(" fadeIn=").Append(e.FadeIn).Append('@').Append(F(e.FadeInEndTime))
            .Append(" fadeOut=").Append(e.FadeOut).Append('@').Append(F(e.FadeOutStartTime))
            .Append(" colorKeys=").Append(e.ColorScale.Count)
            .Append(" colorRepeats=").Append(F(e.ColorScaleRepeats))
            .Append(" sizeKeys=").Append(e.SizeScale.Count)
            .Append(" subdivisions=").Append(e.TextureUSubdivisions).Append('x').Append(e.TextureVSubdivisions)
            .Append(':').Append(e.SubdivisionStart).Append("..").Append(e.SubdivisionEnd);
        Write(text.ToString());

        for (int i = 0; i < e.ColorScale.Count; i++)
            Write("COLOR_KEY object=" + Q(e.ObjectName) + " i=" + i +
                  " time=" + F(e.ColorScale[i].RelativeTime) +
                  " rgba=" + C(e.ColorScale[i].Color));
    }

    public static void Assets(ViewerEmitterData e, ViewerResolvedAssets assets)
    {
        Texture texture = assets.Texture;
        string srgb = "n/a";
        if (texture is Texture2D tex2d)
            srgb = tex2d.isDataSRGB ? "on" : "off";
        string textureInfo = texture == null
            ? "null"
            : Q(texture.name) + " " + texture.width + "x" + texture.height +
              " dimension=" + texture.dimension + " wrap=" + texture.wrapMode +
              " filter=" + texture.filterMode + " format=" + texture.graphicsFormat +
              " sRGB=" + srgb;
        Write(
            "ASSETS object=" + Q(e.ObjectName) +
            " meshSource=" + Q(assets.MeshSource) +
            " mesh=" + Q(assets.Mesh != null ? assets.Mesh.name : null) +
            " textureSource=" + Q(assets.TextureSource) +
            " texture=" + textureInfo);
    }

    public static void Material(
        ViewerEffectData effect,
        ViewerEmitterData e,
        Material mat,
        bool validatedProfileApplied)
    {
        Write(
            "MATERIAL effect=" + Q(effect?.ClassName) +
            " object=" + Q(e.ObjectName) +
            " profileApplied=" + validatedProfileApplied +
            " shader=" + Q(mat.shader != null ? mat.shader.name : null) +
            " supported=" + (mat.shader != null && mat.shader.isSupported) +
            " queue=" + mat.renderQueue +
            " mainTex=" + Q(mat.mainTexture != null ? mat.mainTexture.name : null) +
            " srcBlend=" + P(mat, "_SrcBlend") +
            " dstBlend=" + P(mat, "_DstBlend") +
            " gamma=" + P(mat, "_L2SpriteColorGammaToLinear") +
            " rgbBoost=" + P(mat, "_RgbBoost") +
            " opacity=" + P(mat, "_Opacity") +
            " fadeIn=" + P(mat, "_FadeIn") + "@" + P(mat, "_FadeInEndTime") +
            " fadeOut=" + P(mat, "_Fadeout") + "@" + P(mat, "_FadeoutStartTime") +
            " life=" + PV(mat, "_LifetimeRange") +
            " colorRepeats=" + P(mat, "_ColorScaleParam") +
            " calibration=" + P(mat, "_L2FxWorldCalibration") +
            " subdivision=" + P(mat, "_SubdivisionStart") + ".." + P(mat, "_SubdivisionEnd") +
            " colorFadeAlphaBlend=" + P(mat, "_ColorFadeAlphaBlend") +
            " colorKey0=" + PC(mat, "_ColorKey0") +
            " colorMultiplier=" + PV(mat, "_ColorMultiplier") +
            " colorMulMin=" + PV(mat, "_ColorMulMin") +
            " colorMulMax=" + PV(mat, "_ColorMulMax"));
    }

    public static void Group(
        ViewerEmitterData e,
        int maxCount,
        bool burst,
        float rate,
        float duration)
    {
        Write(
            "GROUP object=" + Q(e.ObjectName) +
            " max=" + maxCount +
            " burst=" + burst +
            " rate=" + F(rate) +
            " duration=" + F(duration) +
            " fixedDelta=" + F(Time.fixedDeltaTime));
    }

    public static void Activate(
        string groupName,
        int slot,
        int spawned,
        float now,
        float startedAt,
        Material mat)
    {
        Write(
            "ACTIVATE group=" + Q(groupName) +
            " slot=" + slot +
            " spawned=" + spawned +
            " time=" + F(now) +
            " sincePlay=" + F(now - startedAt) +
            " frame=" + Time.frameCount +
            " fixedTime=" + F(Time.fixedTime) +
            " startTime=" + P(mat, "_StartTime") +
            " gamma=" + P(mat, "_L2SpriteColorGammaToLinear") +
            " rgbBoost=" + P(mat, "_RgbBoost") +
            " opacity=" + P(mat, "_Opacity"));
    }

    public static void Play(Vector3 position)
    {
        Write("PLAY position=" + V(position) + " time=" + F(Time.time));
    }

    public static void Error(string context, Exception ex)
    {
        Write("ERROR context=" + Q(context) + " exception=" + Q(ex.ToString()));
    }

    static void Write(string message)
    {
        string line = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) +
                      " " + message;
        try
        {
            lock (Sync)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath, line + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ViewerEffectsDiag] Cannot append log: " + ex.Message);
        }
    }

    static string P(Material mat, string property) =>
        mat != null && mat.HasProperty(property) ? F(mat.GetFloat(property)) : "n/a";

    static string PV(Material mat, string property) =>
        mat != null && mat.HasProperty(property) ? V(mat.GetVector(property)) : "n/a";

    static string PC(Material mat, string property) =>
        mat != null && mat.HasProperty(property) ? C(mat.GetColor(property)) : "n/a";

    static string F(float value) => value.ToString("0.######", CultureInfo.InvariantCulture);
    static string R(ViewerRange value) => F(value.Min) + ".." + F(value.Max);
    static string V(Vector3 value) => "(" + F(value.x) + "," + F(value.y) + "," + F(value.z) + ")";
    static string V(Vector4 value) => "(" + F(value.x) + "," + F(value.y) + "," + F(value.z) + "," + F(value.w) + ")";
    static string C(Color value) => "(" + F(value.r) + "," + F(value.g) + "," + F(value.b) + "," + F(value.a) + ")";
    static string Q(string value) => "\"" + (value ?? "<null>").Replace("\"", "'") + "\"";
}

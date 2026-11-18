using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class UcEffectParser
{
    static readonly Regex BeginObject = new Regex(
        @"Begin Object Class=(?<class>\w+)\s+Name=(?<name>\w+)",
        RegexOptions.Compiled);
    static readonly Regex AssetRef = new Regex(
        @"(?:StaticMesh|Mesh|Texture)\s*=\s*(?:StaticMesh|Mesh|Texture)?'(?<ref>[^']+)'",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);
    static readonly Regex ColorScale = new Regex(
        @"ColorScale\((?<i>\d+)\)\s*=\s*\((?<body>(?:[^()]|\([^()]*\))*)\)",
        RegexOptions.Compiled);
    static readonly Regex SizeScale = new Regex(
        @"SizeScale\((?<i>\d+)\)\s*=\s*\((?<body>(?:[^()]|\([^()]*\))*)\)",
        RegexOptions.Compiled);
    static readonly Regex AxisRange = new Regex(
        @"(?<axis>[XYZ])\s*=\s*\((?<body>[^)]*)\)",
        RegexOptions.Compiled);

    public static ViewerEffectData ParseFile(string path)
    {
        string text = File.ReadAllText(path);
        var effect = new ViewerEffectData
        {
            ClassName = Path.GetFileNameWithoutExtension(path),
            SourcePath = path
        };

        Match drawScale = Regex.Match(text, @"DrawScale\s*=\s*([0-9.\-]+)");
        if (drawScale.Success)
            effect.DrawScale = ParseFloat(drawScale.Groups[1].Value, 1f);

        foreach (Match begin in BeginObject.Matches(text))
        {
            int start = begin.Index;
            int end = text.IndexOf("End Object", start, StringComparison.Ordinal);
            if (end < 0)
                end = text.Length;
            string block = text.Substring(start, end - start);
            ViewerEmitterData emitter = ParseEmitter(begin.Groups["class"].Value, begin.Groups["name"].Value, block);
            if (emitter != null)
                effect.Emitters.Add(emitter);
        }

        return effect;
    }

    static ViewerEmitterData ParseEmitter(string className, string objectName, string block)
    {
        var kind = className switch
        {
            "SpriteEmitter" => ViewerEmitterKind.Sprite,
            "MeshEmitter" => ViewerEmitterKind.Mesh,
            "BeamEmitter" => ViewerEmitterKind.Beam,
            "SparkEmitter" => ViewerEmitterKind.Spark,
            "TrailEmitter" => ViewerEmitterKind.Trail,
            _ => ViewerEmitterKind.Unknown
        };
        if (kind == ViewerEmitterKind.Unknown)
            return null;

        var e = new ViewerEmitterData
        {
            Kind = kind,
            ObjectName = objectName,
            DisplayName = ReadString(block, "Name") ?? objectName
        };

        foreach (Match asset in AssetRef.Matches(block))
        {
            string full = asset.Value;
            string value = asset.Groups["ref"].Value;
            if (full.IndexOf("StaticMesh", StringComparison.OrdinalIgnoreCase) >= 0 ||
                (full.StartsWith("Mesh", StringComparison.OrdinalIgnoreCase) && kind == ViewerEmitterKind.Mesh))
                e.StaticMesh = value;
            else
                e.Texture = value;
        }

        e.MaxParticles = ReadInt(block, "MaxParticles", 1);
        e.InitialParticlesPerSecond = ReadFloat(block, "InitialParticlesPerSecond", 0f);
        e.AutomaticInitialSpawning = ReadBool(block, "AutomaticInitialSpawning", true);
        e.RespawnDeadParticles = ReadBool(block, "RespawnDeadParticles", true);
        e.UniformSize = ReadBool(block, "UniformSize", false);
        e.UseSizeScale = ReadBool(block, "UseSizeScale", false);
        e.UseRegularSizeScale = ReadBool(block, "UseRegularSizeScale", true);
        e.SizeScaleRepeats = ReadFloat(block, "SizeScaleRepeats", 0f);
        e.SpinParticles = ReadBool(block, "SpinParticles", false);
        e.FadeIn = ReadBool(block, "FadeIn", false);
        e.FadeOut = ReadBool(block, "FadeOut", false);
        e.FadeInEndTime = ReadFloat(block, "FadeInEndTime", 0f);
        e.FadeOutStartTime = ReadFloat(block, "FadeOutStartTime", 0f);
        e.Opacity = ReadFloat(block, "Opacity", 1f);
        e.UseColorScale = ReadBool(block, "UseColorScale", false);
        e.ColorScaleRepeats = ReadFloat(block, "ColorScaleRepeats", 0f);
        e.RenderTwoSided = ReadBool(block, "RenderTwoSided", true);
        e.UseRandomSubdivision = ReadBool(block, "UseRandomSubdivision", false);
        e.BlendBetweenSubdivisions = ReadBool(block, "BlendBetweenSubdivisions", false);
        e.TextureUSubdivisions = ReadInt(block, "TextureUSubdivisions", 1);
        e.TextureVSubdivisions = ReadInt(block, "TextureVSubdivisions", 1);
        e.SubdivisionStart = ReadInt(block, "SubdivisionStart", 0);
        e.SubdivisionEnd = ReadInt(block, "SubdivisionEnd", Math.Max(0, e.TextureUSubdivisions * e.TextureVSubdivisions - 1));
        e.DrawStyle = ReadIdent(block, "DrawStyle") ?? "PTDS_Translucent";
        e.UseRotationFrom = ReadIdent(block, "UseRotationFrom") ?? "";
        e.GetVelocityDirectionFrom = ReadIdent(block, "GetVelocityDirectionFrom") ?? "";
        e.UseDirectionAs = ReadIdent(block, "UseDirectionAs") ?? "";
        e.StartLocationShape = ReadIdent(block, "StartLocationShape") ?? "";
        e.DetermineEndPointBy = ReadIdent(block, "DetermineEndPointBy") ?? e.DetermineEndPointBy;
        e.LowFrequencyPoints = ReadInt(block, "LowFrequencyPoints", e.LowFrequencyPoints);
        e.HighFrequencyPoints = ReadInt(block, "HighFrequencyPoints", e.HighFrequencyPoints);
        e.BeamTextureUScale = ReadFloat(block, "BeamTextureUScale", 1f);
        e.BeamTextureVScale = ReadFloat(block, "BeamTextureVScale", 1f);

        ReadRange(block, "LifetimeRange", ref e.Lifetime);
        ReadRange(block, "BeamDistanceRange", ref e.BeamDistanceRange);
        ReadRange(block, "InitialDelayRange", ref e.InitialDelay);
        ReadVectorRange(block, "StartSizeRange", ref e.StartSizeX, ref e.StartSizeY, ref e.StartSizeZ);
        ReadVectorRange(block, "StartLocationRange", ref e.StartLocationX, ref e.StartLocationY, ref e.StartLocationZ);
        ReadVectorRange(block, "StartLocationPolarRange", ref e.PolarTheta, ref e.PolarPhi, ref e.PolarRadius);
        ReadVectorRange(block, "StartVelocityRange", ref e.StartVelocityX, ref e.StartVelocityY, ref e.StartVelocityZ);
        ReadVectorRange(block, "StartSpinRange", ref e.StartSpinX, ref e.StartSpinY, ref e.StartSpinZ);
        ReadVectorRange(block, "SpinsPerSecondRange", ref e.SpinsPerSecondX, ref e.SpinsPerSecondY, ref e.SpinsPerSecondZ);
        ReadColorMul(block, ref e.ColorMulMin, ref e.ColorMulMax);
        e.SpinCCWorCW = ReadXYZ(block, "SpinCCWorCW", Vector3.one);
        e.Acceleration = ReadXYZ(block, "Acceleration", Vector3.zero);
        e.StartLocationOffset = ReadXYZ(block, "StartLocationOffset", Vector3.zero);

        foreach (Match m in ColorScale.Matches(block))
        {
            e.ColorScale.Add(new ViewerColorKey
            {
                RelativeTime = ReadKeyedFloat(m.Groups["body"].Value, "RelativeTime", 0f),
                Color = ReadColor(m.Groups["body"].Value)
            });
        }
        foreach (Match m in SizeScale.Matches(block))
        {
            e.SizeScale.Add(new ViewerSizeKey
            {
                RelativeTime = ReadKeyedFloat(m.Groups["body"].Value, "RelativeTime", 0f),
                RelativeSize = ReadKeyedFloat(m.Groups["body"].Value, "RelativeSize", 1f)
            });
        }

        // First BeamEndPoints entry drives MVP PTEP_Offset (weighted pick later).
        if (TryExtractBeamEndPointOffset(block, out string offsetBody))
        {
            ReadVectorRangeBody(offsetBody,
                ref e.BeamEndOffsetX, ref e.BeamEndOffsetY, ref e.BeamEndOffsetZ);
        }

        if (e.ColorScale.Count == 0)
        {
            e.ColorScale.Add(new ViewerColorKey { RelativeTime = 0f, Color = Color.white });
            e.ColorScale.Add(new ViewerColorKey { RelativeTime = 1f, Color = Color.white });
        }

        return e;
    }

    static bool TryExtractBeamEndPointOffset(string block, out string offsetBody)
    {
        offsetBody = null;
        Match head = Regex.Match(block, @"BeamEndPoints\(\d+\)\s*=\s*\(", RegexOptions.IgnoreCase);
        if (!head.Success)
            return false;

        string epBody = ExtractBalanced(block, head.Index + head.Length - 1);
        if (epBody == null)
            return false;

        Match offsetHead = Regex.Match(epBody, @"offset\s*=\s*\(", RegexOptions.IgnoreCase);
        if (!offsetHead.Success)
            return false;

        offsetBody = ExtractBalanced(epBody, offsetHead.Index + offsetHead.Length - 1);
        return offsetBody != null;
    }

    // Extract contents inside the '(' at openParenIndex (exclusive of the surrounding parens).
    static string ExtractBalanced(string text, int openParenIndex)
    {
        if (openParenIndex < 0 || openParenIndex >= text.Length || text[openParenIndex] != '(')
            return null;
        int depth = 0;
        for (int i = openParenIndex; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '(')
                depth++;
            else if (c == ')')
            {
                depth--;
                if (depth == 0)
                    return text.Substring(openParenIndex + 1, i - openParenIndex - 1);
            }
        }
        return null;
    }

    static void ReadRange(string block, string name, ref ViewerRange range)
    {
        Match m = Regex.Match(block, name + @"\s*=\s*\((?<body>[^)]*)\)");
        if (!m.Success)
            return;
        range = ParseMinMax(m.Groups["body"].Value, range.Min, range.Max);
    }

    static void ReadVectorRange(string block, string name, ref ViewerRange x, ref ViewerRange y, ref ViewerRange z)
    {
        Match m = Regex.Match(block, name + @"\s*=\s*\((?<body>(?:[^()]|\([^()]*\))*)\)");
        if (!m.Success)
            return;
        ReadVectorRangeBody(m.Groups["body"].Value, ref x, ref y, ref z);
    }

    static void ReadVectorRangeBody(string body, ref ViewerRange x, ref ViewerRange y, ref ViewerRange z)
    {
        foreach (Match axis in AxisRange.Matches(body))
        {
            ViewerRange parsed = ParseMinMax(axis.Groups["body"].Value, 0f, 0f);
            switch (axis.Groups["axis"].Value)
            {
                case "X": x = parsed; break;
                case "Y": y = parsed; break;
                case "Z": z = parsed; break;
            }
        }
    }

    // ColorMultiplierRange is FRangeVector: X=R, Y=G, Z=B. Not FColor BGRA.
    static void ReadColorMul(string block, ref Vector3 min, ref Vector3 max)
    {
        Match m = Regex.Match(block, @"ColorMultiplierRange\s*=\s*\((?<body>(?:[^()]|\([^()]*\))*)\)");
        if (!m.Success)
            return;
        min = Vector3.one;
        max = Vector3.one;
        foreach (Match axis in AxisRange.Matches(m.Groups["body"].Value))
        {
            ViewerRange r = ParseMinMax(axis.Groups["body"].Value, 1f, 1f);
            switch (axis.Groups["axis"].Value)
            {
                case "X": min.x = r.Min; max.x = r.Max; break;
                case "Y": min.y = r.Min; max.y = r.Max; break;
                case "Z": min.z = r.Min; max.z = r.Max; break;
            }
        }
    }

    static ViewerRange ParseMinMax(string body, float defaultMin, float defaultMax)
    {
        float min = ReadKeyedFloat(body, "Min", defaultMin);
        float max = ReadKeyedFloat(body, "Max", defaultMax);
        if (!body.Contains("Min") && body.Contains("Max"))
            min = defaultMin;
        if (!body.Contains("Max") && body.Contains("Min"))
            max = min;
        return new ViewerRange(min, max);
    }

    static Vector3 ReadXYZ(string block, string name, Vector3 fallback)
    {
        Match m = Regex.Match(block, name + @"\s*=\s*\((?<body>[^)]*)\)");
        if (!m.Success)
            return fallback;
        string body = m.Groups["body"].Value;
        float x = ReadKeyedFloat(body, "X", fallback.x);
        float y = ReadKeyedFloat(body, "Y", fallback.y);
        float z = ReadKeyedFloat(body, "Z", fallback.z);
        return new Vector3(x, y, z);
    }

    // Unreal FColor in .uc is struct field order B,G,R,A (BGRA in text).
    // Unity Color is RGBA — map by field name, never by list position.
    // Missing Color= → (0,0,0,0). Present Color= with omitted A → opaque 255.
    // ColorMultiplierRange X/Y/Z stays RGB (X=R,Y=G,Z=B); do not swap to BGR.
    static Color ReadColor(string body)
    {
        Match inner = Regex.Match(body, @"Color\s*=\s*\((?<c>[^)]*)\)");
        if (!inner.Success)
            return new Color(0f, 0f, 0f, 0f);

        string c = inner.Groups["c"].Value;
        float r = ReadByteChannel(c, "R", 0f);
        float g = ReadByteChannel(c, "G", 0f);
        float b = ReadByteChannel(c, "B", 0f);
        float a = ReadByteChannel(c, "A", 255f);
        return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
    }

    static float ReadByteChannel(string body, string channel, float fallback)
    {
        Match m = Regex.Match(body, @"(?<![A-Za-z])" + channel + @"\s*=\s*([+\-0-9.]+)");
        return m.Success ? ParseFloat(m.Groups[1].Value, fallback) : fallback;
    }

    static float ReadKeyedFloat(string body, string key, float fallback)
    {
        Match m = Regex.Match(body, @"(?<![A-Za-z_])" + key + @"\s*=\s*([+\-0-9.]+)");
        return m.Success ? ParseFloat(m.Groups[1].Value, fallback) : fallback;
    }

    static string ReadString(string block, string name)
    {
        Match m = Regex.Match(block, name + @"\s*=\s*""([^""]*)""");
        if (m.Success)
            return m.Groups[1].Value;
        return null;
    }

    static string ReadIdent(string block, string name)
    {
        Match m = Regex.Match(block, name + @"\s*=\s*([A-Za-z0-9_]+)");
        return m.Success ? m.Groups[1].Value : null;
    }

    static bool ReadBool(string block, string name, bool fallback)
    {
        Match m = Regex.Match(block, name + @"\s*=\s*(True|False)", RegexOptions.IgnoreCase);
        if (!m.Success)
            return fallback;
        return m.Groups[1].Value.Equals("True", StringComparison.OrdinalIgnoreCase);
    }

    static int ReadInt(string block, string name, int fallback)
    {
        Match m = Regex.Match(block, name + @"\s*=\s*([+\-0-9]+)");
        return m.Success && int.TryParse(m.Groups[1].Value, out int v) ? v : fallback;
    }

    static float ReadFloat(string block, string name, float fallback)
    {
        Match m = Regex.Match(block, name + @"\s*=\s*([+\-0-9.]+)");
        return m.Success ? ParseFloat(m.Groups[1].Value, fallback) : fallback;
    }

    static float ParseFloat(string s, float fallback)
    {
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : fallback;
    }
}

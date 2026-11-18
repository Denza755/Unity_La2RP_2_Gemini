using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CPU side of BeamEmitter MVP (PTEP_Offset, LF/HF strip, no noise yet).
/// Builds a unit strip mesh; HLSL expands it to world start→end with camera-facing width.
/// Geometry contract matches captured L2 beam mesh CSV: UV.x along beam, UV.y = 0/1 edges.
/// </summary>
public static class ViewerBeamBuilder
{
    const int MaxSegments = 32;

    public static Mesh BuildStripMesh(ViewerEmitterData emitter)
    {
        int hf = Mathf.Clamp(emitter != null ? emitter.HighFrequencyPoints : 2, 2, MaxSegments + 1);
        int segments = hf - 1;
        int vertCount = (segments + 1) * 2;
        int triCount = segments * 6;

        var verts = new Vector3[vertCount];
        var uvs = new Vector2[vertCount];
        var tris = new int[triCount];

        float uScale = emitter != null ? Mathf.Max(emitter.BeamTextureUScale, 0.0001f) : 1f;
        float vScale = emitter != null ? Mathf.Max(emitter.BeamTextureVScale, 0.0001f) : 1f;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            int v0 = i * 2;
            // Unit strip: Z = along beam (0..1), X = across (-0.5..0.5). Y unused.
            verts[v0] = new Vector3(-0.5f, 0f, t);
            verts[v0 + 1] = new Vector3(0.5f, 0f, t);
            uvs[v0] = new Vector2(t * uScale, 0f * vScale);
            uvs[v0 + 1] = new Vector2(t * uScale, 1f * vScale);
        }

        int ti = 0;
        for (int i = 0; i < segments; i++)
        {
            int a = i * 2;
            int b = a + 1;
            int c = a + 2;
            int d = a + 3;
            // Two-sided facing: both winding orders so Cull Off is enough.
            tris[ti++] = a;
            tris[ti++] = c;
            tris[ti++] = b;
            tris[ti++] = b;
            tris[ti++] = c;
            tris[ti++] = d;
        }

        var mesh = new Mesh
        {
            name = "ViewerBeamStrip_HF" + hf,
            hideFlags = HideFlags.DontSave
        };
        mesh.SetVertices(new List<Vector3>(verts));
        mesh.SetUVs(0, new List<Vector2>(uvs));
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        // Generous bounds — actual world extent comes from start/end in the shader.
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 50f);
        return mesh;
    }

    public static int SegmentCount(ViewerEmitterData emitter)
    {
        int hf = Mathf.Clamp(emitter != null ? emitter.HighFrequencyPoints : 2, 2, MaxSegments + 1);
        return hf - 1;
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class PskMeshLoader
{
    struct Chunk
    {
        public string Id;
        public int DataSize;
        public int DataCount;
        public long DataStart;
    }

    public static Mesh Load(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        var chunks = ReadChunks(bytes);
        if (!chunks.TryGetValue("PNTS0000", out Chunk points) ||
            !chunks.TryGetValue("VTXW0000", out Chunk wedges) ||
            !TryGetFace(chunks, out Chunk faces))
            throw new InvalidDataException("PSK missing PNTS/VTXW/FACE: " + path);

        var verts = new Vector3[points.DataCount];
        for (int i = 0; i < points.DataCount; i++)
        {
            int o = (int)points.DataStart + i * 12;
            float x = BitConverter.ToSingle(bytes, o);
            float y = BitConverter.ToSingle(bytes, o + 4);
            float z = BitConverter.ToSingle(bytes, o + 8);
            // UE Z-up UU -> Unity Y-up meters.
            // Same basis as L2Fx_UcPositionToUnityMeters / L2FxMeshSpin:
            //   Unity(X,Y,Z) = UE(X,Z,Y) * 0.01
            // (magiccircle* rings are authored in UE XY = ground plane).
            verts[i] = new Vector3(x, z, y) * 0.01f;
        }

        int wedgeStride = wedges.DataCount > 0 ? wedges.DataSize : 16;
        var meshVerts = new List<Vector3>(wedges.DataCount);
        var meshUv = new List<Vector2>(wedges.DataCount);
        var pointIndex = new int[wedges.DataCount];
        for (int i = 0; i < wedges.DataCount; i++)
        {
            int o = (int)wedges.DataStart + i * wedgeStride;
            int idx = wedgeStride >= 16 && wedges.DataSize >= 16
                ? BitConverter.ToUInt16(bytes, o)
                : BitConverter.ToInt32(bytes, o);
            if (idx < 0 || idx >= verts.Length)
                idx = 0;
            pointIndex[i] = idx;
            float u = BitConverter.ToSingle(bytes, o + 4);
            float v = BitConverter.ToSingle(bytes, o + 8);
            meshVerts.Add(verts[idx]);
            meshUv.Add(new Vector2(u, 1f - v));
        }

        int faceStride = faces.DataCount > 0 ? faces.DataSize : 12;
        var tris = new List<int>(faces.DataCount * 3);
        for (int i = 0; i < faces.DataCount; i++)
        {
            int o = (int)faces.DataStart + i * faceStride;
            int a = BitConverter.ToUInt16(bytes, o);
            int b = BitConverter.ToUInt16(bytes, o + 2);
            int c = BitConverter.ToUInt16(bytes, o + 4);
            if (a >= meshVerts.Count || b >= meshVerts.Count || c >= meshVerts.Count)
                continue;
            // Flip winding after the improper Y↔Z basis swap (det = -1).
            tris.Add(a);
            tris.Add(c);
            tris.Add(b);
        }

        EnsureGroundPlaneLiesOnXZ(meshVerts);

        var mesh = new Mesh { name = Path.GetFileNameWithoutExtension(path) };
        mesh.SetVertices(meshVerts);
        mesh.SetUVs(0, meshUv);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        Debug.Log(
            "[ViewerEffects] PSK " + Path.GetFileName(path) +
            " bounds=" + mesh.bounds.size +
            " (thin axis should be Y for ground rings)");
        return mesh;
    }

    /// <summary>
    /// Ground FX meshes (magiccircle*, rings) are authored in the UE XY plane.
    /// After UE→Unity remap the thin axis must be Unity Y. If a mesh still has
    /// its thin axis on Z or X, rotate it flat onto XZ so rings stay horizontal.
    /// </summary>
    static void EnsureGroundPlaneLiesOnXZ(List<Vector3> verts)
    {
        if (verts == null || verts.Count < 3)
            return;

        Vector3 min = verts[0];
        Vector3 max = verts[0];
        for (int i = 1; i < verts.Count; i++)
        {
            min = Vector3.Min(min, verts[i]);
            max = Vector3.Max(max, verts[i]);
        }

        Vector3 size = max - min;
        float sx = Mathf.Abs(size.x);
        float sy = Mathf.Abs(size.y);
        float sz = Mathf.Abs(size.z);
        float largest = Mathf.Max(sx, Mathf.Max(sy, sz));
        if (largest < 1e-6f)
            return;

        // Already horizontal (thin ≈ Y), or not planar enough to decide.
        float thinThreshold = largest * 0.2f;
        bool thinY = sy <= thinThreshold;
        bool thinZ = sz <= thinThreshold;
        bool thinX = sx <= thinThreshold;
        if (thinY || (!thinZ && !thinX))
            return;

        for (int i = 0; i < verts.Count; i++)
        {
            Vector3 v = verts[i];
            if (thinZ)
            {
                // UE XY left in Unity XY → Rx -90: (x,y,z) -> (x,z,-y)
                verts[i] = new Vector3(v.x, v.z, -v.y);
            }
            else // thinX: ring in Unity YZ → Ry +90: (x,y,z) -> (z,y,-x)
            {
                verts[i] = new Vector3(v.z, v.y, -v.x);
            }
        }
    }

    static bool TryGetFace(Dictionary<string, Chunk> chunks, out Chunk faces)
    {
        if (chunks.TryGetValue("FACE0000", out faces))
            return true;
        if (chunks.TryGetValue("FACE3200", out faces))
            return true;
        faces = default;
        return false;
    }

    static Dictionary<string, Chunk> ReadChunks(byte[] bytes)
    {
        var map = new Dictionary<string, Chunk>(StringComparer.Ordinal);
        int offset = 0;
        while (offset + 32 <= bytes.Length)
        {
            string id = Encoding.ASCII.GetString(bytes, offset, 20).TrimEnd('\0', ' ');
            int dataSize = BitConverter.ToInt32(bytes, offset + 24);
            int dataCount = BitConverter.ToInt32(bytes, offset + 28);
            offset += 32;
            if (dataSize < 0 || dataCount < 0)
                break;
            long dataBytes = (long)dataSize * dataCount;
            if (offset + dataBytes > bytes.Length)
                break;
            map[id] = new Chunk
            {
                Id = id,
                DataSize = dataSize,
                DataCount = dataCount,
                DataStart = offset
            };
            offset += (int)dataBytes;
        }
        return map;
    }
}

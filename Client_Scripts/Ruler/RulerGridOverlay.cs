using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RulerGridOverlay : MonoBehaviour
{
    private static RulerGridOverlay _instance;
    public static RulerGridOverlay Instance => _instance;

    [Header("Grid Size")]
    [SerializeField] private int _gridRadius = 12; // 24x24 cells
    [SerializeField] private float _heightOffset = 0.08f;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;
    private Material _gridMaterial;

    private Vector3 _lastCenterGridPos = new Vector3(float.MaxValue, 0, float.MaxValue);
    private bool _isVisible = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        CreateGridMaterial();

        _mesh = new Mesh();
        _mesh.name = "RulerGridMesh";
        _meshFilter.mesh = _mesh;

        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void CreateGridMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }
        if (shader == null)
        {
            shader = Shader.Find("Hidden/Internal-Colored");
        }

        _gridMaterial = new Material(shader);
        Color gridColor = new Color(0.85f, 0.9f, 1.0f, 0.45f);
        _gridMaterial.color = gridColor;
        if (_gridMaterial.HasProperty("_Color"))
        {
            _gridMaterial.SetColor("_Color", gridColor);
        }
        if (_gridMaterial.HasProperty("_BaseColor"))
        {
            _gridMaterial.SetColor("_BaseColor", gridColor);
        }

        _gridMaterial.SetFloat("_Surface", 1f); // 1 = Transparent
        _gridMaterial.SetFloat("_Blend", 0f); // 0 = Alpha
        _gridMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _gridMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        _gridMaterial.SetInt("_ZWrite", 0);
        _gridMaterial.renderQueue = 3000;
        _gridMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        _gridRendererSetup();
    }

    private void _gridRendererSetup()
    {
        _meshRenderer.material = _gridMaterial;
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _meshRenderer.receiveShadows = false;
    }

    public void SetVisible(bool visible)
    {
        _isVisible = visible;
        if (_meshRenderer != null)
        {
            _meshRenderer.enabled = visible;
        }

        if (!visible)
        {
            _lastCenterGridPos = new Vector3(float.MaxValue, 0, float.MaxValue);
        }
    }

    public void UpdateGridPosition(Vector3 worldCursorPos)
    {
        if (!_isVisible) return;

        float cellSize = RulerManager.Instance != null ? RulerManager.Instance.CellSize : 1.0f;
        float snappedX = Mathf.Round(worldCursorPos.x / cellSize) * cellSize;
        float snappedZ = Mathf.Round(worldCursorPos.z / cellSize) * cellSize;
        Vector3 snappedCenter = new Vector3(snappedX, 0, snappedZ);

        if (Vector3.SqrMagnitude(snappedCenter - _lastCenterGridPos) < 0.01f)
        {
            return;
        }

        _lastCenterGridPos = snappedCenter;
        RebuildGridMesh(snappedCenter, cellSize);
    }

    private void RebuildGridMesh(Vector3 center, float cellSize)
    {
        LayerMask mask = RulerManager.GetTerrainRaycastMask();

        int linesCount = (_gridRadius * 2 + 1);
        int totalSegments = linesCount * 2; // lines in X and Z
        int totalVertices = totalSegments * (_gridRadius * 2 + 1);

        // We generate lines as a series of connected points conforming to terrain
        System.Collections.Generic.List<Vector3> verts = new System.Collections.Generic.List<Vector3>();
        System.Collections.Generic.List<int> indices = new System.Collections.Generic.List<int>();

        float minCoord = -_gridRadius * cellSize;
        float maxCoord = _gridRadius * cellSize;

        // Lines parallel to Z (varying X)
        for (int i = -_gridRadius; i <= _gridRadius; i++)
        {
            float lineX = center.x + i * cellSize;
            int startIndex = verts.Count;

            for (int step = -_gridRadius; step <= _gridRadius; step++)
            {
                float lineZ = center.z + step * cellSize;
                float y = SampleTerrainHeight(lineX, lineZ, mask);
                verts.Add(new Vector3(lineX, y + _heightOffset, lineZ));

                if (step > -_gridRadius)
                {
                    indices.Add(verts.Count - 2);
                    indices.Add(verts.Count - 1);
                }
            }
        }

        // Lines parallel to X (varying Z)
        for (int i = -_gridRadius; i <= _gridRadius; i++)
        {
            float lineZ = center.z + i * cellSize;

            for (int step = -_gridRadius; step <= _gridRadius; step++)
            {
                float lineX = center.x + step * cellSize;
                float y = SampleTerrainHeight(lineX, lineZ, mask);
                verts.Add(new Vector3(lineX, y + _heightOffset, lineZ));

                if (step > -_gridRadius)
                {
                    indices.Add(verts.Count - 2);
                    indices.Add(verts.Count - 1);
                }
            }
        }

        _mesh.Clear();
        _mesh.SetVertices(verts);

        Color32[] lineColors = new Color32[verts.Count];
        Color32 col32 = new Color(0.85f, 0.9f, 1.0f, 0.45f);
        for (int c = 0; c < lineColors.Length; c++)
        {
            lineColors[c] = col32;
        }
        _mesh.SetColors(lineColors);

        _mesh.SetIndices(indices, MeshTopology.Lines, 0);
    }

    private float SampleTerrainHeight(float x, float z, LayerMask mask)
    {
        Ray ray = new Ray(new Vector3(x, 1500f, z), Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, 3000f, mask);
        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h.collider == null || h.collider.isTrigger) continue;
                if (BuildingPlacementSystem.Instance != null && BuildingPlacementSystem.Instance.IsGhostTransform(h.transform)) continue;
                return h.point.y;
            }
        }

        if (World.Instance != null)
        {
            return World.Instance.GetGroundHeight(new Vector3(x, 0, z));
        }

        return 0f;
    }
}

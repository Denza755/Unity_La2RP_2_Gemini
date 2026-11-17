using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class PlacedBuildingData
{
    public string prefabName;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public float scaleX, scaleY, scaleZ;
}

[Serializable]
public class PlacedBuildingList
{
    public List<PlacedBuildingData> buildings = new List<PlacedBuildingData>();
}

public class BuildingPlacementSystem : MonoBehaviour
{
    private static BuildingPlacementSystem _instance;
    public static BuildingPlacementSystem Instance => _instance;

    [Header("Ghost Materials")]
    private Material _validGhostMaterial;
    private Material _invalidGhostMaterial;

    [Header("Current Selection")]
    [SerializeField] private string _selectedPrefabName;
    private GameObject _selectedPrefab;
    private GameObject _ghostInstance;
    private float _currentRotation = 0f;
    private bool _isPlacementActive = false;
    private bool _isValidPlacement = false;
    private Vector3 _currentSnappedPos;

    [Header("Validation Limits")]
    [SerializeField] private float _maxSlopeHeightDelta = 1.6f;

    private Transform _placedContainer;
    private Camera _cam;

    private static readonly string[] AvailableBuildingPrefabs = new string[]
    {
        "Preset_Blacksmith_01",
        "Preset_Church_01_A",
        "Preset_Church_01_B",
        "Preset_House_01_A",
        "Preset_House_01_B",
        "Preset_House_02_A",
        "Preset_House_02_B",
        "Preset_House_02_C",
        "Preset_House_03",
        "Preset_House_04",
        "Preset_House_05",
        "Preset_House_06",
        "Preset_House_07",
        "Preset_House_08",
        "Preset_House_09_A",
        "Preset_House_09_B",
        "Preset_House_09_C",
        "Preset_House_09_D",
        "Preset_House_10",
        "Preset_House_Archway_01",
        "Preset_House_Archway_02",
        "Preset_House_Windmill_01",
        "Preset_Hut_01",
        "Preset_Hut_02",
        "Preset_Outhouse_01",
        "Preset_Shelter_01",
        "Preset_Shelter_02",
        "Preset_Stables_01",
        "Preset_Tavern_01",
        "Preset_Tower_01"
    };

    private static List<string> _cachedPrefabNames;

    public static IReadOnlyList<string> GetAvailableBuildingPrefabNames()
    {
        if (_cachedPrefabNames == null || _cachedPrefabNames.Count == 0)
        {
            _cachedPrefabNames = new List<string>();
            if (AvailableBuildingPrefabs != null && AvailableBuildingPrefabs.Length > 0)
            {
                _cachedPrefabNames.AddRange(AvailableBuildingPrefabs);
            }

            if (_cachedPrefabNames.Count == 0)
            {
                GameObject[] loaded = Resources.LoadAll<GameObject>("Data/Buildings");
                for (int i = 0; i < loaded.Length; i++)
                {
                    if (loaded[i] != null && !string.IsNullOrEmpty(loaded[i].name))
                    {
                        _cachedPrefabNames.Add(loaded[i].name);
                    }
                }
            }
        }
        return _cachedPrefabNames;
    }

    public IReadOnlyList<string> BuildingPrefabNames => GetAvailableBuildingPrefabNames();
    public string SelectedPrefabName => _selectedPrefabName;
    public bool HasSelection => !string.IsNullOrEmpty(_selectedPrefabName) && _ghostInstance != null;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "placed_buildings.json");

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

        InitMaterials();
        EnsureContainer();
        _cam = Camera.main;
    }

    private void Start()
    {
        LoadPlacedBuildings();
    }

    private void OnDestroy()
    {
        ClearGhost();
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void EnsureContainer()
    {
        if (_placedContainer == null)
        {
            GameObject containerGo = GameObject.Find("PlacedBuildings");
            if (containerGo == null)
            {
                containerGo = new GameObject("PlacedBuildings");
            }
            _placedContainer = containerGo.transform;
        }
    }

    public bool IsGhostTransform(Transform t)
    {
        return _ghostInstance != null && (t == _ghostInstance.transform || t.IsChildOf(_ghostInstance.transform));
    }

    private void InitMaterials()
    {
        _validGhostMaterial = CreateURPTransparentMaterial(new Color(0.7f, 0.72f, 0.75f, 0.55f));
        _invalidGhostMaterial = CreateURPTransparentMaterial(new Color(0.85f, 0.25f, 0.25f, 0.55f));
    }

    private Material CreateURPTransparentMaterial(Color col)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Hidden/Internal-Colored");

        Material mat = new Material(shader);
        mat.color = col;
        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", col);
        }

        if (mat.HasProperty("_Color"))
        {
            mat.SetColor("_Color", col);
        }

        mat.SetFloat("_Surface", 1f); // 1 = Transparent
        mat.SetFloat("_Blend", 0f); // 0 = Alpha blend
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        return mat;
    }

    public void SelectBuildingByIndex(int index)
    {
        var list = GetAvailableBuildingPrefabNames();
        if (list != null && index >= 0 && index < list.Count)
        {
            SelectBuilding(list[index]);
        }
    }

    public void SetPlacementActive(bool active)
    {
        _isPlacementActive = active;
        if (!active)
        {
            ClearGhost();
        }
        else
        {
            if (string.IsNullOrEmpty(_selectedPrefabName))
            {
                SelectBuildingByIndex(0);
            }
            else
            {
                SelectBuilding(_selectedPrefabName);
            }
        }
    }

    public void SelectBuilding(string prefabName)
    {
        _selectedPrefabName = prefabName;
        ClearGhost();

        if (!_isPlacementActive || string.IsNullOrEmpty(_selectedPrefabName))
        {
            return;
        }

        string path = $"Data/Buildings/{_selectedPrefabName}";
        _selectedPrefab = Resources.Load<GameObject>(path);
        if (_selectedPrefab == null)
        {
            Debug.LogError($"[BuildingPlacementSystem] Failed to load prefab at {path}");
            return;
        }

        CreateGhost();
    }

    public void CancelSelection()
    {
        _selectedPrefabName = null;
        _selectedPrefab = null;
        ClearGhost();
    }

    private void ClearGhost()
    {
        if (_ghostInstance != null)
        {
            Destroy(_ghostInstance);
            _ghostInstance = null;
        }
    }

    private void CreateGhost()
    {
        if (_selectedPrefab == null) return;

        _ghostInstance = Instantiate(_selectedPrefab);
        _ghostInstance.name = "Ghost_" + _selectedPrefab.name;

        // Disable all colliders on ghost (so raycasts don't hit it and physics don't interact)
        foreach (var col in _ghostInstance.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }

        // Force LOD 0 so ghost never disappears or reduces quality from high RTS camera
        foreach (var lod in _ghostInstance.GetComponentsInChildren<LODGroup>(true))
        {
            lod.ForceLOD(0);
        }

        foreach (var anim in _ghostInstance.GetComponentsInChildren<Animation>(true))
        {
            Destroy(anim);
        }
        foreach (var anim in _ghostInstance.GetComponentsInChildren<Animator>(true))
        {
            Destroy(anim);
        }
        foreach (var audio in _ghostInstance.GetComponentsInChildren<AudioSource>(true))
        {
            Destroy(audio);
        }

        float scale = RulerManager.Instance != null ? RulerManager.Instance.BuildingScale : 0.65f;
        _ghostInstance.transform.localScale = Vector3.one * scale;
        _ghostInstance.transform.rotation = Quaternion.Euler(0, _currentRotation, 0);

        _isValidPlacement = true;
        ApplyGhostMaterial(_ghostInstance, _validGhostMaterial);
    }

    private void ApplyGhostMaterial(GameObject root, Material mat)
    {
        if (root == null || mat == null) return;
        var renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderers[i].receiveShadows = false;
            Material[] mats = new Material[renderers[i].sharedMaterials.Length];
            for (int m = 0; m < mats.Length; m++)
            {
                mats[m] = mat;
            }
            renderers[i].materials = mats;
        }
    }

    private Camera GetActiveGameCamera()
    {
        if (RulerCameraController.Instance != null && RulerCameraController.Instance.Cam != null && RulerCameraController.Instance.Cam.isActiveAndEnabled)
        {
            return RulerCameraController.Instance.Cam;
        }
        if (CameraController.Instance != null)
        {
            Camera c = CameraController.Instance.GetComponent<Camera>();
            if (c != null && c.isActiveAndEnabled) return c;
        }
        if (Camera.main != null && Camera.main.isActiveAndEnabled)
        {
            return Camera.main;
        }
        return _cam != null ? _cam : Camera.current;
    }

    private void Update()
    {
        if (!_isPlacementActive) return;

        _cam = GetActiveGameCamera();
        if (_cam == null) return;

        HandleRotationInput();
        UpdateGhostAndGrid();
        HandlePlacementAndDeletion();
    }

    private void HandleRotationInput()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.rKey.wasPressedThisFrame)
            {
                float step = (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed) ? 45f : 90f;
                _currentRotation = Mathf.Repeat(_currentRotation + step, 360f);
                if (_ghostInstance != null)
                {
                    _ghostInstance.transform.rotation = Quaternion.Euler(0, _currentRotation, 0);
                }
            }

            if (kb.escapeKey.wasPressedThisFrame)
            {
                CancelSelection();
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.rightButton.wasPressedThisFrame && HasSelection)
        {
            CancelSelection();
        }
    }

    private bool RaycastTerrainFromCamera(out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;
        Camera cam = GetActiveGameCamera();
        if (cam == null) return false;

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(mousePos);
        LayerMask mask = RulerManager.GetTerrainRaycastMask();

        RaycastHit[] hits = Physics.RaycastAll(ray, 2000f, mask);
        if (hits == null || hits.Length == 0) return false;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Primary: find terrain / ground mesh
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.collider == null || h.collider.isTrigger) continue;
            if (IsGhostTransform(h.transform)) continue;
            if (_placedContainer != null && h.transform.IsChildOf(_placedContainer)) continue;

            hitPoint = h.point;
            return true;
        }

        // Secondary: if on placed building or other static collider
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            if (h.collider == null || h.collider.isTrigger) continue;
            if (IsGhostTransform(h.transform)) continue;

            hitPoint = h.point;
            return true;
        }

        return false;
    }

    private float SampleGroundElevation(float x, float sampleFromY, float z)
    {
        LayerMask mask = RulerManager.GetTerrainRaycastMask();
        Ray ray = new Ray(new Vector3(x, sampleFromY + 50f, z), Vector3.down);
        RaycastHit[] hits = Physics.RaycastAll(ray, 150f, mask);
        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            for (int i = 0; i < hits.Length; i++)
            {
                var h = hits[i];
                if (h.collider == null || h.collider.isTrigger) continue;
                if (IsGhostTransform(h.transform)) continue;
                if (_placedContainer != null && h.transform.IsChildOf(_placedContainer)) continue;
                return h.point.y;
            }
        }

        if (World.Instance != null)
        {
            return World.Instance.GetGroundHeight(new Vector3(x, sampleFromY, z));
        }

        return sampleFromY;
    }

    private void UpdateGhostAndGrid()
    {
        if (RaycastTerrainFromCamera(out Vector3 hitPoint))
        {
            if (RulerGridOverlay.Instance != null)
            {
                RulerGridOverlay.Instance.UpdateGridPosition(hitPoint);
            }

            if (_ghostInstance != null)
            {
                float cellSize = RulerManager.Instance != null ? RulerManager.Instance.CellSize : 1.0f;
                float snappedX = Mathf.Round(hitPoint.x / cellSize) * cellSize;
                float snappedZ = Mathf.Round(hitPoint.z / cellSize) * cellSize;

                float groundY = SampleGroundElevation(snappedX, hitPoint.y, snappedZ);

                _currentSnappedPos = new Vector3(snappedX, groundY, snappedZ);
                _ghostInstance.transform.position = _currentSnappedPos;
                _ghostInstance.transform.rotation = Quaternion.Euler(0, _currentRotation, 0);

                bool valid = ValidatePlacement(_currentSnappedPos, _ghostInstance.transform.rotation);
                if (valid != _isValidPlacement || _ghostInstance.GetComponentInChildren<Renderer>()?.sharedMaterial != (_isValidPlacement ? _validGhostMaterial : _invalidGhostMaterial))
                {
                    _isValidPlacement = valid;
                    ApplyGhostMaterial(_ghostInstance, _isValidPlacement ? _validGhostMaterial : _invalidGhostMaterial);
                }
            }
        }
    }

    private bool ValidatePlacement(Vector3 centerPos, Quaternion rotation)
    {
        if (_ghostInstance == null) return false;

        Bounds bounds = CalculateBounds(_ghostInstance);
        Vector3 extents = bounds.extents;
        extents.x = Mathf.Max(extents.x, 0.5f);
        extents.z = Mathf.Max(extents.z, 0.5f);

        // 1. Slope check: check 4 corners of foundation
        Vector3[] corners = new Vector3[4]
        {
            centerPos + rotation * new Vector3(-extents.x, 0, -extents.z),
            centerPos + rotation * new Vector3(extents.x, 0, -extents.z),
            centerPos + rotation * new Vector3(-extents.x, 0, extents.z),
            centerPos + rotation * new Vector3(extents.x, 0, extents.z),
        };

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        for (int i = 0; i < 4; i++)
        {
            float cornerY = SampleGroundElevation(corners[i].x, centerPos.y, corners[i].z);
            if (cornerY < minY) minY = cornerY;
            if (cornerY > maxY) maxY = cornerY;
        }

        if ((maxY - minY) > _maxSlopeHeightDelta)
        {
            return false;
        }

        // 2. Overlap check with other buildings, players, npcs
        Vector3 boxCenter = centerPos + Vector3.up * (extents.y * 0.9f);
        Vector3 halfExtents = new Vector3(extents.x * 0.9f, extents.y * 0.8f, extents.z * 0.9f);

        Collider[] colliders = Physics.OverlapBox(boxCenter, halfExtents, rotation);
        for (int i = 0; i < colliders.Length; i++)
        {
            var col = colliders[i];
            if (col == null || col.isTrigger) continue;
            // Ignore terrain collider itself
            if (col is TerrainCollider) continue;
            if (IsGhostTransform(col.transform)) continue;

            // If it's a placed building or player/monster/npc/obstacle
            if ((_placedContainer != null && col.transform.IsChildOf(_placedContainer)) ||
                col.CompareTag("Player") ||
                col.CompareTag("Monster") ||
                col.CompareTag("Npc"))
            {
                return false;
            }
        }

        return true;
    }

    private Bounds CalculateBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(go.transform.position, Vector3.one * 2f);
        }

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            b.Encapsulate(renderers[i].bounds);
        }
        return b;
    }

    private void HandlePlacementAndDeletion()
    {
        // Don't place or delete if pointer is over UI elements or Ruler UI
        if ((L2GameUI.Instance != null && L2GameUI.Instance.MouseOverUI) ||
            (RulerHUDWindow.Instance != null && RulerHUDWindow.Instance.IsPointerOverRulerUI()))
        {
            return;
        }

        Mouse mouse = Mouse.current;
        Keyboard kb = Keyboard.current;

        // Left Click to place
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (HasSelection && _isValidPlacement)
            {
                RequestPlaceBuilding(_selectedPrefabName, _currentSnappedPos, _currentRotation);
            }
        }

        // Delete or Del key to remove hovered placed building
        if (kb != null && (kb.deleteKey.wasPressedThisFrame || kb.numpadPeriodKey.wasPressedThisFrame))
        {
            TryDemolishHoveredBuilding();
        }
    }

    public void RequestPlaceBuilding(string prefabName, Vector3 position, float rotY)
    {
        float scale = RulerManager.Instance != null ? RulerManager.Instance.BuildingScale : 0.65f;
        if (GameClient.Instance != null && GameClient.Instance.ClientPacketHandler != null)
        {
            GameClient.Instance.ClientPacketHandler.SendRulerBuild(prefabName, position, rotY, scale);
        }
        else
        {
            // Offline fallback
            PlaceBuilding(prefabName, position, Quaternion.Euler(0, rotY, 0), true);
        }
    }

    public void HandleServerRulerBuildings(RulerBuildingsPacket packet)
    {
        if (packet == null) return;

        EnsureContainer();

        if (packet.Mode == 0) // Chunk list
        {
            for (int i = 0; i < packet.Buildings.Count; i++)
            {
                var b = packet.Buildings[i];
                SpawnOrUpdateBuildingFromNetwork(b.Id, b.PrefabName, b.Position, b.RotY, b.Scale, b.OwnerName);
            }
        }
        else if (packet.Mode == 1) // Single building added
        {
            for (int i = 0; i < packet.Buildings.Count; i++)
            {
                var b = packet.Buildings[i];
                SpawnOrUpdateBuildingFromNetwork(b.Id, b.PrefabName, b.Position, b.RotY, b.Scale, b.OwnerName);
            }
        }
        else if (packet.Mode == 2) // Single building demolished
        {
            int targetId = packet.DemolishedBuildingId;
            for (int i = _placedContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = _placedContainer.GetChild(i);
                PlacedBuildingInstance pbi = child.GetComponent<PlacedBuildingInstance>();
                if (pbi != null && pbi.ServerBuildingId == targetId)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    public GameObject SpawnOrUpdateBuildingFromNetwork(int serverId, string prefabName, Vector3 pos, float rotY, float scale, string ownerName)
    {
        EnsureContainer();

        // Check if building already exists
        for (int i = 0; i < _placedContainer.childCount; i++)
        {
            Transform child = _placedContainer.GetChild(i);
            PlacedBuildingInstance pbi = child.GetComponent<PlacedBuildingInstance>();
            if (pbi != null && pbi.ServerBuildingId == serverId)
            {
                child.position = pos;
                child.rotation = Quaternion.Euler(0, rotY, 0);
                float s = scale > 0 ? scale : (RulerManager.Instance != null ? RulerManager.Instance.BuildingScale : 0.65f);
                child.localScale = Vector3.one * s;
                pbi.OwnerName = ownerName;
                return child.gameObject;
            }
        }

        string path = $"Data/Buildings/{prefabName}";
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"[BuildingPlacementSystem] Unable to load building prefab: {path}");
            return null;
        }

        Quaternion rotation = Quaternion.Euler(0, rotY, 0);
        GameObject instance = Instantiate(prefab, pos, rotation, _placedContainer);
        instance.name = $"{prefabName}_{serverId}";
        float finalScale = scale > 0 ? scale : (RulerManager.Instance != null ? RulerManager.Instance.BuildingScale : 0.65f);
        instance.transform.localScale = Vector3.one * finalScale;

        PlacedBuildingInstance newPbi = instance.AddComponent<PlacedBuildingInstance>();
        newPbi.ServerBuildingId = serverId;
        newPbi.PrefabName = prefabName;
        newPbi.OwnerName = ownerName;

        SetupBuildingColliders(instance);
        return instance;
    }

    public void PlaceBuilding(string prefabName, Vector3 position, Quaternion rotation, bool save = true)
    {
        string path = $"Data/Buildings/{prefabName}";
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null) return;

        EnsureContainer();

        GameObject instance = Instantiate(prefab, position, rotation, _placedContainer);
        instance.name = prefabName;
        float scale = RulerManager.Instance != null ? RulerManager.Instance.BuildingScale : 0.65f;
        instance.transform.localScale = Vector3.one * scale;

        PlacedBuildingInstance pbi = instance.AddComponent<PlacedBuildingInstance>();
        pbi.ServerBuildingId = 0;
        pbi.PrefabName = prefabName;
        pbi.OwnerName = "Local";

        // Ensure colliders exist for players/physics
        SetupBuildingColliders(instance);

        if (save)
        {
            SavePlacedBuildings();
        }
    }

    private void SetupBuildingColliders(GameObject building)
    {
        // Check if there is already a collider
        var existingColliders = building.GetComponentsInChildren<Collider>();
        if (existingColliders.Length > 0)
        {
            return;
        }

        // Check for simplified mesh *_SP
        Transform spChild = null;
        foreach (var t in building.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.EndsWith("_SP", StringComparison.OrdinalIgnoreCase) ||
                t.name.Contains("_col", StringComparison.OrdinalIgnoreCase))
            {
                spChild = t;
                break;
            }
        }

        if (spChild != null && spChild.GetComponent<MeshFilter>() != null)
        {
            MeshCollider mc = spChild.gameObject.AddComponent<MeshCollider>();
            mc.convex = false;
        }
        else
        {
            // Add BoxCollider based on render bounds
            Bounds b = CalculateBounds(building);
            BoxCollider box = building.AddComponent<BoxCollider>();
            box.center = building.transform.InverseTransformPoint(b.center);
            Vector3 worldSize = b.size;
            Vector3 localScale = building.transform.localScale;
            box.size = new Vector3(
                localScale.x != 0 ? worldSize.x / localScale.x : worldSize.x,
                localScale.y != 0 ? worldSize.y / localScale.y : worldSize.y,
                localScale.z != 0 ? worldSize.z / localScale.z : worldSize.z
            );
        }
    }

    public bool TryDemolishHoveredBuilding()
    {
        Camera cam = GetActiveGameCamera();
        if (cam == null) return false;

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
        {
            EnsureContainer();
            Transform cur = hit.transform;
            while (cur != null && cur.parent != _placedContainer)
            {
                cur = cur.parent;
            }

            if (cur != null && cur.parent == _placedContainer)
            {
                PlacedBuildingInstance pbi = cur.GetComponent<PlacedBuildingInstance>();
                if (pbi != null && pbi.ServerBuildingId > 0 && GameClient.Instance != null && GameClient.Instance.ClientPacketHandler != null)
                {
                    GameClient.Instance.ClientPacketHandler.SendRulerDemolish(pbi.ServerBuildingId);
                    return true;
                }

                // Fallback for offline/local
                Destroy(cur.gameObject);
                StartCoroutine(SaveAfterDestroyNextFrame());
                return true;
            }
        }

        return false;
    }

    private System.Collections.IEnumerator SaveAfterDestroyNextFrame()
    {
        yield return null;
        SavePlacedBuildings();
    }

    public void SavePlacedBuildings()
    {
        EnsureContainer();
        PlacedBuildingList list = new PlacedBuildingList();

        for (int i = 0; i < _placedContainer.childCount; i++)
        {
            Transform child = _placedContainer.GetChild(i);
            if (child == null) continue;

            PlacedBuildingData data = new PlacedBuildingData
            {
                prefabName = child.name.Replace("(Clone)", "").Trim(),
                posX = child.position.x,
                posY = child.position.y,
                posZ = child.position.z,
                rotX = child.rotation.x,
                rotY = child.rotation.y,
                rotZ = child.rotation.z,
                rotW = child.rotation.w,
                scaleX = child.localScale.x,
                scaleY = child.localScale.y,
                scaleZ = child.localScale.z
            };
            list.buildings.Add(data);
        }

        try
        {
            string json = JsonUtility.ToJson(list, true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BuildingPlacementSystem] Failed to save buildings: {ex.Message}");
        }
    }

    public void LoadPlacedBuildings()
    {
        EnsureContainer();

        // Clear existing
        for (int i = _placedContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(_placedContainer.GetChild(i).gameObject);
        }

        if (!File.Exists(SaveFilePath))
        {
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            PlacedBuildingList list = JsonUtility.FromJson<PlacedBuildingList>(json);
            if (list == null || list.buildings == null) return;

            for (int i = 0; i < list.buildings.Count; i++)
            {
                var data = list.buildings[i];
                Vector3 pos = new Vector3(data.posX, data.posY, data.posZ);
                Quaternion rot = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
                PlaceBuilding(data.prefabName, pos, rot, false);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[BuildingPlacementSystem] Failed to load buildings: {ex.Message}");
        }
    }
}

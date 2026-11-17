using System;
using System.Collections.Generic;
using UnityEngine;

public enum RulerSubMode
{
    Observe,
    Build
}

public class RulerManager : MonoBehaviour
{
    private static RulerManager _instance;
    public static RulerManager Instance => _instance;

    [Header("Permissions")]
    [SerializeField] private bool _allowAllPlayers = true;
    [SerializeField] private List<string> _allowedAccounts = new List<string> { "ruler", "admin" };
    [SerializeField] private bool _forceEnableRulerForTesting = false;

    [Header("Building Settings")]
    [SerializeField] private float _cellSize = 1.0f;
    [SerializeField] private float _buildingScale = 0.65f;
    [SerializeField] private LayerMask _terrainLayer = ~0;

    [Header("State")]
    [SerializeField] private bool _isRtsActive = false;
    [SerializeField] private RulerSubMode _subMode = RulerSubMode.Observe;

    public event Action<bool> OnRtsCameraToggled;
    public event Action<RulerSubMode> OnSubModeChanged;
    public event Action<bool> OnRulerAccessChanged;

    public bool IsRtsActive => _isRtsActive;
    public RulerSubMode CurrentSubMode => _subMode;
    public float CellSize => _cellSize;
    public float BuildingScale => _buildingScale;
    public LayerMask TerrainLayer => _terrainLayer;

    public bool AllowAllPlayers
    {
        get => _allowAllPlayers;
        set => _allowAllPlayers = value;
    }

    public List<string> AllowedAccounts
    {
        get => _allowedAccounts;
        set => _allowedAccounts = value;
    }

    public static LayerMask GetTerrainRaycastMask()
    {
        int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        int ui = LayerMask.NameToLayer("UI");
        int player = LayerMask.NameToLayer("Player");
        int entity = LayerMask.NameToLayer("Entity");
        int entityClick = LayerMask.NameToLayer("EntityClick");
        int water = LayerMask.NameToLayer("Water");
        int transparentFx = LayerMask.NameToLayer("TransparentFX");

        int exclude = 0;
        if (ignoreRaycast >= 0) exclude |= (1 << ignoreRaycast);
        if (ui >= 0) exclude |= (1 << ui);
        if (player >= 0) exclude |= (1 << player);
        if (entity >= 0) exclude |= (1 << entity);
        if (entityClick >= 0) exclude |= (1 << entityClick);
        if (water >= 0) exclude |= (1 << water);
        if (transparentFx >= 0) exclude |= (1 << transparentFx);

        return ~exclude;
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _terrainLayer = GetTerrainRaycastMask();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public bool HasRulerAccess()
    {
        if (_allowAllPlayers || _forceEnableRulerForTesting)
        {
            return true;
        }

        string currentAccount = LoginClient.Instance != null ? LoginClient.Instance.Account : null;
        if (string.IsNullOrEmpty(currentAccount))
        {
            currentAccount = PlayerPrefs.GetString("LastAccountName", "");
        }

        if (string.IsNullOrEmpty(currentAccount))
        {
            // If the window is already opened and player is Queen or authorized
            string charName = PlayerEntity.Instance != null && PlayerEntity.Instance.Identity != null
                ? PlayerEntity.Instance.Identity.Name
                : null;
            if (!string.IsNullOrEmpty(charName) &&
                (string.Equals(charName, "Queen", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(charName, "ruler", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(charName, "admin", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return false;
        }

        currentAccount = currentAccount.Trim().ToLowerInvariant();
        for (int i = 0; i < _allowedAccounts.Count; i++)
        {
            if (string.Equals(_allowedAccounts[i]?.Trim(), currentAccount, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        // Also check if allowed accounts contains Queen or character name
        if (PlayerEntity.Instance != null && PlayerEntity.Instance.Identity != null)
        {
            string charName = PlayerEntity.Instance.Identity.Name?.Trim();
            if (!string.IsNullOrEmpty(charName))
            {
                for (int i = 0; i < _allowedAccounts.Count; i++)
                {
                    if (string.Equals(_allowedAccounts[i]?.Trim(), charName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void SetRtsCamera(bool active)
    {
        _isRtsActive = active;

        if (RulerCameraController.Instance == null)
        {
            RulerSystemBootstrapper.EnsureRulerSystemsInScene();
        }

        if (RulerCameraController.Instance != null)
        {
            RulerCameraController.Instance.SetActiveCamera(active);
        }

        if (CameraController.Instance != null)
        {
            CameraController.Instance.enabled = !active;
        }

        if (RulerGridOverlay.Instance != null)
        {
            RulerGridOverlay.Instance.SetVisible(active && _subMode == RulerSubMode.Build);
        }

        if (BuildingPlacementSystem.Instance != null)
        {
            BuildingPlacementSystem.Instance.SetPlacementActive(active && _subMode == RulerSubMode.Build);
        }

        OnRtsCameraToggled?.Invoke(_isRtsActive);
    }

    public void ToggleRtsCamera()
    {
        SetRtsCamera(!_isRtsActive);
    }

    public void SetSubMode(RulerSubMode newMode)
    {
        _subMode = newMode;

        // If the user clicks Observe Mode or Build Mode while RTS Camera is OFF, auto-activate RTS Camera
        if (!_isRtsActive)
        {
            SetRtsCamera(true);
            return;
        }

        bool isBuild = (_isRtsActive && _subMode == RulerSubMode.Build);

        if (isBuild)
        {
            RulerSystemBootstrapper.EnsureRulerSystemsInScene();
        }

        if (RulerGridOverlay.Instance != null)
        {
            RulerGridOverlay.Instance.SetVisible(isBuild);
        }

        if (BuildingPlacementSystem.Instance != null)
        {
            BuildingPlacementSystem.Instance.SetPlacementActive(isBuild);
        }

        OnSubModeChanged?.Invoke(_subMode);
    }

    public void NotifyAccessCheck()
    {
        OnRulerAccessChanged?.Invoke(HasRulerAccess());
    }
}

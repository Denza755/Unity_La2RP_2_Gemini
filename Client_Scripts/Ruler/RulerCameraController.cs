using UnityEngine;
using UnityEngine.InputSystem;

public class RulerCameraController : MonoBehaviour
{
    private static RulerCameraController _instance;
    public static RulerCameraController Instance => _instance;

    [Header("Speeds")]
    [SerializeField] private float _moveSpeed = 40f;
    [SerializeField] private float _fastMoveMultiplier = 2.5f;
    [SerializeField] private float _panSpeed = 1.2f;
    [SerializeField] private float _rotationSpeed = 140f;
    [SerializeField] private float _zoomSpeed = 25f;
    [SerializeField] private float _smoothTime = 0.08f;

    [Header("Height Limits")]
    [SerializeField] private float _minHeight = 5f;
    [SerializeField] private float _maxHeight = 120f;
    [SerializeField] private float _defaultPitch = 50f;

    private Camera _cam;
    public Camera Cam => _cam;
    private CameraController _originalCamController;

    private Vector3 _currentPivot;
    private Vector3 _targetPivot;
    private float _currentHeight = 30f;
    private float _targetHeight = 30f;
    private float _currentYaw = 0f;
    private float _targetYaw = 0f;
    private float _pitch = 50f;

    private Vector3 _pivotVelocity;
    private float _heightVelocity;
    private float _yawVelocity;

    private bool _isPanning = false;
    private bool _isOrbiting = false;

    private Vector3 _lastSentSyncPivot;
    private float _lastSyncTime;
    private const float SYNC_INTERVAL = 0.5f;
    private const float SYNC_DISTANCE_THRESHOLD = 20f; // 20m

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

        ResolveCameraReference();
        enabled = false;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void ResolveCameraReference()
    {
        if (CameraController.Instance != null)
        {
            _originalCamController = CameraController.Instance;
            _cam = CameraController.Instance.GetComponent<Camera>();
        }

        if (_cam == null)
        {
            _cam = Camera.main;
        }

        if (_cam != null && _originalCamController == null)
        {
            _originalCamController = _cam.GetComponent<CameraController>();
        }
    }

    public void SetActiveCamera(bool active)
    {
        ResolveCameraReference();
        enabled = active;

        if (active)
        {
            // Guaranteed disable of original 3rd-person controller
            if (_originalCamController != null)
            {
                _originalCamController.enabled = false;
            }
            if (CameraController.Instance != null)
            {
                CameraController.Instance.enabled = false;
            }

            // Determine player/target position
            Vector3 centerPos = Vector3.zero;
            if (PlayerEntity.Instance != null)
            {
                centerPos = PlayerEntity.Instance.transform.position;
            }
            else if (CameraController.Instance != null && CameraController.Instance.Target != null)
            {
                centerPos = CameraController.Instance.Target.position;
            }
            else if (_cam != null)
            {
                centerPos = _cam.transform.position;
            }

            _targetPivot = new Vector3(centerPos.x, centerPos.y, centerPos.z);
            _currentPivot = _targetPivot;

            // Set to bird's-eye view (+30m height) with 50° pitch
            _targetHeight = 30f;
            _currentHeight = 30f;

            _targetYaw = _cam != null ? _cam.transform.eulerAngles.y : 0f;
            _currentYaw = _targetYaw;
            _pitch = _defaultPitch;

            // Instantly apply position and rotation
            ApplyCameraTransformInstant();

            _lastSentSyncPivot = _targetPivot;
            _lastSyncTime = Time.time;
            SendCameraSync(true, _targetPivot);

            Debug.Log($"[RulerCameraController] Activated! Camera='{(_cam != null ? _cam.name : "null")}', Pivot={_targetPivot}, CamPos={(_cam != null ? _cam.transform.position.ToString() : "N/A")}");
        }
        else
        {
            if (_originalCamController != null)
            {
                _originalCamController.enabled = true;
            }
            if (CameraController.Instance != null)
            {
                CameraController.Instance.enabled = true;
            }
            SendCameraSync(false, Vector3.zero);
            Debug.Log("[RulerCameraController] Deactivated, returned to standard camera.");
        }
    }

    private void SendCameraSync(bool active, Vector3 pivot)
    {
        if (GameClient.Instance != null && GameClient.Instance.ClientPacketHandler != null)
        {
            int l2x = (int)(pivot.z * 52.5f);
            int l2y = (int)(pivot.x * 52.5f);
            int l2z = (int)(pivot.y * 52.5f);
            GameClient.Instance.ClientPacketHandler.SendRulerCameraSync(active, l2x, l2y, l2z);
        }
    }

    private void Update()
    {
        if (!enabled || (RulerManager.Instance != null && !RulerManager.Instance.IsRtsActive))
        {
            return;
        }

        HandleInput();
    }

    private void LateUpdate()
    {
        if (!enabled || (RulerManager.Instance != null && !RulerManager.Instance.IsRtsActive))
        {
            return;
        }

        // If camera reference was lost or scene changed, re-resolve
        if (_cam == null)
        {
            ResolveCameraReference();
            if (_cam == null) return;
        }

        // Keep 3rd-person controller disabled
        if (_originalCamController != null && _originalCamController.enabled)
        {
            _originalCamController.enabled = false;
        }
        if (CameraController.Instance != null && CameraController.Instance.enabled)
        {
            CameraController.Instance.enabled = false;
        }

        _currentPivot = Vector3.SmoothDamp(_currentPivot, _targetPivot, ref _pivotVelocity, _smoothTime);
        _currentHeight = Mathf.SmoothDamp(_currentHeight, _targetHeight, ref _heightVelocity, _smoothTime);
        _currentYaw = Mathf.SmoothDampAngle(_currentYaw, _targetYaw, ref _yawVelocity, _smoothTime);

        UpdateCameraTransform();

        if (Time.time - _lastSyncTime >= SYNC_INTERVAL)
        {
            if (Vector3.Distance(_currentPivot, _lastSentSyncPivot) >= SYNC_DISTANCE_THRESHOLD)
            {
                _lastSentSyncPivot = _currentPivot;
                _lastSyncTime = Time.time;
                SendCameraSync(true, _currentPivot);
            }
        }
    }

    private void HandleInput()
    {
        // Don't intercept WASD if player is typing in chat input
        if (ChatWindow.Instance != null && ChatWindow.Instance.ChatOpened)
        {
            return;
        }
        if (L2GameUI.Instance != null && L2GameUI.Instance.IsTyping)
        {
            return;
        }

        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;

        // 1. Keyboard movement (WASD / Arrow keys)
        float h = 0f;
        float v = 0f;

        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) h -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) h += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v += 1f;
        }

        float speed = _moveSpeed;
        if (kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed))
        {
            speed *= _fastMoveMultiplier;
        }

        Quaternion yawRot = Quaternion.Euler(0, _currentYaw, 0);
        Vector3 forward = yawRot * Vector3.forward;
        Vector3 right = yawRot * Vector3.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * v + right * h).normalized;
        if (moveDir.sqrMagnitude > 0.001f)
        {
            _targetPivot += moveDir * (speed * Time.unscaledDeltaTime);
        }

        // 2. Zoom via mouse scroll wheel
        if (mouse != null)
        {
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                float scrollStep = Mathf.Sign(scroll) * _zoomSpeed * 0.4f;
                _targetHeight -= scrollStep;
                _targetHeight = Mathf.Clamp(_targetHeight, _minHeight, _maxHeight);
            }
        }

        // 3. Mouse Pan / Orbit
        if (mouse != null)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();

            _isPanning = mouse.middleButton.isPressed;
            _isOrbiting = mouse.rightButton.isPressed;

            if (_isPanning)
            {
                float panFactor = (_currentHeight / 30f) * _panSpeed * 0.04f;
                _targetPivot -= (right * mouseDelta.x + forward * mouseDelta.y) * panFactor;
            }

            if (_isOrbiting)
            {
                _targetYaw += mouseDelta.x * (_rotationSpeed * 0.015f);
            }
        }

        // 4. Q and E rotation
        if (kb != null)
        {
            if (kb.qKey.isPressed)
            {
                _targetYaw -= _rotationSpeed * Time.unscaledDeltaTime;
            }
            if (kb.eKey.isPressed)
            {
                _targetYaw += _rotationSpeed * Time.unscaledDeltaTime;
            }
        }
    }

    private void ApplyCameraTransformInstant()
    {
        if (_cam == null) return;

        Quaternion rot = Quaternion.Euler(_pitch, _targetYaw, 0f);
        float distance = _targetHeight / Mathf.Sin(Mathf.Max(15f, _pitch) * Mathf.Deg2Rad);
        Vector3 camPos = _targetPivot - (rot * Vector3.forward * distance);

        _cam.transform.position = camPos;
        _cam.transform.rotation = rot;
    }

    private void UpdateCameraTransform()
    {
        if (_cam == null) return;

        Quaternion rot = Quaternion.Euler(_pitch, _currentYaw, 0f);
        float distance = _currentHeight / Mathf.Sin(Mathf.Max(15f, _pitch) * Mathf.Deg2Rad);
        Vector3 camPos = _currentPivot - (rot * Vector3.forward * distance);

        _cam.transform.position = camPos;
        _cam.transform.rotation = rot;
    }
}

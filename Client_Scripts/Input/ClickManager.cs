using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickManager : MonoBehaviour
{
    private const float LocatorSurfaceOffset = 0.025f;

    [SerializeField] private GameObject _locator;
    [SerializeField] private float _locatorHideDelay = 5f;
    [SerializeField] private ObjectData _targetObjectData;
    [SerializeField] private ObjectData _hoverObjectData;

    public ObjectData HoverObjectData { get { return _hoverObjectData; } }

    private Vector3 _lastClickPosition = Vector3.zero;
    [SerializeField] private LayerMask _entityMask;
    [SerializeField] private LayerMask _clickThroughMask;
    private Camera _mainCamera;
    private Coroutine _hideLocatorCoroutine;

    private static ClickManager _instance;
    public static ClickManager Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void OnDestroy()
    {
        StopHideLocatorCoroutine();
        _instance = null;
    }

    void Start()
    {
        if (_locator == null)
        {
            _locator = GameObject.Find("Locator");
        }
        _mainCamera = CameraController.Instance.GetComponent<Camera>();

        HideLocator();
    }

    public void SetMasks(LayerMask entityMask, LayerMask clickThroughMask)
    {
        _entityMask = entityMask;
        _clickThroughMask = clickThroughMask;
    }

    void Update()
    {
        if (L2GameUI.Instance.MouseOverUI || PlayerStateMachine.Instance != null && PlayerStateMachine.Instance.State == PlayerState.DEAD)
        {
            return;
        }

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Ray ray = _mainCamera.ScreenPointToRay(mousePos);
        RaycastHit hit;
        bool hitSomething = false;

        // Use a 0.35m radius SphereCast first to easily target low/small creatures like Keltir or Imp
        if (Physics.SphereCast(ray, 0.35f, out RaycastHit sphereHit, 1000f, _entityMask))
        {
            hit = sphereHit;
            hitSomething = true;
        }
        else if (Physics.Raycast(ray, out hit, 1000f, ~_clickThroughMask))
        {
            hitSomething = true;
        }

        if (hitSomething)
        {
            int hitLayer = hit.collider.gameObject.layer;
            if (_entityMask == (_entityMask | (1 << hitLayer)))
            {
                _hoverObjectData = new ObjectData(hit.transform.parent.parent.gameObject); // click area -> model -> entity
            }
            else
            {
                _hoverObjectData = new ObjectData(hit.collider.gameObject);
            }

            if (InputManager.Instance.LeftClickDown &&
                !InputManager.Instance.RightClickHeld)
            {
                _targetObjectData = _hoverObjectData;

                if (_entityMask == (_entityMask | (1 << hitLayer)) && _targetObjectData.ObjectTag != "Player")
                {
                    OnClickOnEntity();
                }
                else if (_targetObjectData != null)
                {
                    if (RulerManager.Instance != null && RulerManager.Instance.IsRtsActive)
                    {
                        // In Ruler RTS mode, prevent local player character from moving on ground clicks!
                    }
                    else
                    {
                        OnClickToMove(hit);
                    }
                }
            }

            if (_hoverObjectData.ObjectTransform != null && _hoverObjectData.ObjectTag == "Pickup")
            {
                CursorManager.Instance.ChangeCursor(CursorManager.CursorType.Pickup);
            }
            else if (_hoverObjectData.ObjectTransform != null &&
                     _hoverObjectData.Entity is NetworkHumanoidEntity humanoid &&
                     (humanoid.IsInPrivateStore || humanoid.PrivateStoreType > 0 || humanoid.IsSitting))
            {
                CursorManager.Instance.ChangeCursor(CursorManager.CursorType.Talk);
            }
            else if (_hoverObjectData.ObjectTransform != null && _targetObjectData.ObjectTransform != null && _targetObjectData.ObjectTransform == _hoverObjectData.ObjectTransform)
            {
                if (_hoverObjectData.ObjectTag == "Monster" && !_hoverObjectData.Entity.Status.IsDead)
                {
                    CursorManager.Instance.ChangeCursor(CursorManager.CursorType.Attack);
                }
                else if (_hoverObjectData.ObjectTag == "Npc")
                {
                    CursorManager.Instance.ChangeCursor(CursorManager.CursorType.Talk);
                }
                else
                {
                    CursorManager.Instance.ChangeCursor(CursorManager.CursorType.Default);
                }
            }
            else
            {
                CursorManager.Instance.ChangeCursor(CursorManager.CursorType.Default);
            }
        }
        else
        {
            CursorManager.Instance.ChangeCursor(CursorManager.CursorType.Default);
        }
    }

    public void OnClickToMove(RaycastHit hit)
    {
        if (PlayerStateMachine.Instance != null && PlayerStateMachine.Instance.State == PlayerState.SKILL)
        {
            return;
        }

        _lastClickPosition = hit.point;
        //  PlayerCombatController.Instance.RunningToTarget = false;

        if (PlayerStateMachine.Instance != null)
        {
            PlayerStateMachine.Instance.NotifyEvent(Event.CLICK_TO_MOVE, _lastClickPosition);
            // PlayerStateMachine.Instance.ChangeIntention(Intention.INTENTION_MOVE_TO, _lastClickPosition);
        }

        if (TargetManager.Instance != null)
        {
            //TODO: Do it in attackstate exit
            TargetManager.Instance.ClearAttackTarget();
        }

        //  PathFinderController.Instance.MoveTo(_lastClickPosition);
        float angle = Vector3.Angle(hit.normal, Vector3.up);
        if (angle < 85f)
        {
            PlaceLocator(_lastClickPosition, hit.normal);
        }
    }

    public void OnClickOnEntity()
    {
        bool isMob = _targetObjectData != null &&
            (_targetObjectData.ObjectTag == "Monster" ||
             (_targetObjectData.Entity != null &&
              _targetObjectData.Entity.Identity != null &&
              _targetObjectData.Entity.Identity.EntityType == EntityType.Monster));

        if (isMob)
        {
            HideLocator();
        }

        // Debug.Log("Click on entity");
        if (TargetManager.Instance.HasTarget() && TargetManager.Instance.Target.transform == _targetObjectData.ObjectTransform)
        {
            PlayerActions.Instance.UseAction(ActionType.Attack);
        }
        else
        {
            TargetManager.Instance.SetTarget(_targetObjectData);
        }
    }

    private void PlaceLocator(Vector3 position, Vector3 normal)
    {
        StopHideLocatorCoroutine();

        if (_locator == null)
        {
            return;
        }

        Vector3 safeNormal = normal.sqrMagnitude > 0.0001f ? normal.normalized : Vector3.up;
        _locator.transform.SetPositionAndRotation(
            position + safeNormal * LocatorSurfaceOffset,
            Quaternion.FromToRotation(Vector3.up, safeNormal));
        _locator.SetActive(true);
    }

    public void ScheduleHideLocator()
    {
        if (!gameObject.activeInHierarchy || _locator == null || !_locator.activeSelf || _hideLocatorCoroutine != null)
        {
            return;
        }

        if (_locatorHideDelay <= 0f)
        {
            HideLocator();
            return;
        }

        _hideLocatorCoroutine = StartCoroutine(HideLocatorAfterDelay(_locatorHideDelay));
    }

    public void HideLocator()
    {
        StopHideLocatorCoroutine();

        if (_locator != null)
        {
            _locator.SetActive(false);
        }
    }

    private IEnumerator HideLocatorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _hideLocatorCoroutine = null;
        HideLocator();
    }

    private void StopHideLocatorCoroutine()
    {
        if (_hideLocatorCoroutine != null)
        {
            StopCoroutine(_hideLocatorCoroutine);
            _hideLocatorCoroutine = null;
        }
    }
}

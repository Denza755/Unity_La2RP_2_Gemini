using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    private Entity _target = null;
    private Entity _attackTarget = null;
    private Transform _playerTransform;
    private bool _hasServerTargetColor;
    private int _serverTargetColor;
    private int _serverTargetEntityId = -1;
    private int _playerLevelAtServerTargetColor;

    [SerializeField] private float _maximumTargetDistance = 15f;
    [SerializeField] private LayerMask _entityMask;
    [SerializeField] private RaycastHit[] _entitiesInRange;
    [SerializeField] private List<Transform> _visibleEntities;
    [SerializeField] private int _nextTargetIndex;

    public Entity Target { get { return _target; } }
    public Entity AttackTarget { get { return _attackTarget; } }

    private static TargetManager _instance;
    public static TargetManager Instance { get { return _instance; } }

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

    private void Start()
    {
        _target = null;
        _attackTarget = null;
    }

    void OnDestroy()
    {
        _instance = null;
    }

    public void SetMask(LayerMask mask)
    {
        _entityMask = mask;
    }

    private void FixedUpdate()
    {
        if (_playerTransform == null)
        {
            if (PlayerEntity.Instance != null && PlayerEntity.Instance.transform != null)
            {
                _playerTransform = PlayerEntity.Instance.transform;
            }
            else
            {
                return;
            }
        }

        VerifyNextTargetIndex();

        _entitiesInRange = Physics.SphereCastAll(_playerTransform.position, _maximumTargetDistance, transform.forward, 0, _entityMask);
        _visibleEntities = GetVisibleEntities();
    }

    private List<Transform> GetVisibleEntities()
    {
        List<Transform> visibleEntities = new List<Transform>();

        for (int i = 0; i < _entitiesInRange.Length; i++)
        {
            Transform entityTransform = _entitiesInRange[i].transform;
            Entity entity = entityTransform.GetComponent<Entity>();
            if (IsTransformVisible(entityTransform) && entity.Identity.EntityType == EntityType.Monster && !entity.IsDead)
            {
                visibleEntities.Add(entityTransform);
            }
        }

        visibleEntities = visibleEntities
            .OrderBy(obj => Vector3.Distance(_playerTransform.position, obj.position))
            .ToList();

        return visibleEntities;
    }

    private bool IsTransformVisible(Transform target)
    {
        bool isTooFar = Vector3.Distance(_playerTransform.position, target.position) > _maximumTargetDistance;
        if (isTooFar)
        {
            return false;
        }

        return CameraController.Instance.IsObjectVisible(target);
    }

    private void VerifyNextTargetIndex()
    {
        // Is the next target out of bounds
        if (_nextTargetIndex >= _visibleEntities.Count)
        {
            _nextTargetIndex = 0;
            return;
        }

        // Is our target visible
        if (HasTarget() && !IsTransformVisible(_target.transform))
        {
            _nextTargetIndex = 0;
            return;
        }

        // Did my next target index change?
        if (HasTarget())
        {
            for (int i = 0; i < _visibleEntities.Count; i++)
            {
                if (_visibleEntities[i] == _target.transform)
                {
                    if (i != _nextTargetIndex)
                    {
                        _nextTargetIndex = 0;
                    }
                }
            }

            return;
        }
    }

    private int GetNextTargetIndex()
    {
        int index = _nextTargetIndex;

        if (_visibleEntities.Count == 0)
        {
            return 0;
        }

        if (index >= _visibleEntities.Count)
        {
            index = 0;
        }

        if (HasTarget() && _visibleEntities[index] == _target.transform)
        {
            index++;
        }

        if (index >= _visibleEntities.Count)
        {
            index = 0;
        }

        return index;
    }

    public void NextTarget()
    {
        if (_visibleEntities.Count == 0)
        {
            return;
        }

        _nextTargetIndex = GetNextTargetIndex();

        SetTarget(new ObjectData(_visibleEntities[_nextTargetIndex].gameObject));
    }

    public void SetTarget(ObjectData target)
    {
        if (target == null)
        {
            ClearTarget();
            return;
        }

        SetTarget(target.ObjectTransform.GetComponent<Entity>());
    }

    public void SetTarget(Entity target)
    {
        if (target == null)
        {
            return;
        }

        bool isNewTarget = _target == null || _target.Identity.Id != target.Identity.Id;
        _target = target;

        if (isNewTarget)
        {
            _hasServerTargetColor = false;
            _serverTargetColor = 0;
            _serverTargetEntityId = -1;
            _playerLevelAtServerTargetColor = 0;
        }

        PlayerCombat.Instance.TargetId = _target.Identity.Id;
        PlayerCombat.Instance.Target = _target;

        ShopWindow.Instance.HideWindow(false);

        TargetWindow.Instance?.RefreshTargetColor();

        GameClient.Instance.ClientPacketHandler.SendRequestSetTarget(_target.Identity.Id);
    }

    public void OnMyTargetSelected(Entity targeted, int serverColor)
    {
        if (targeted == null || targeted.Identity == null)
        {
            _hasServerTargetColor = false;
            _serverTargetColor = 0;
            _serverTargetEntityId = -1;
            _playerLevelAtServerTargetColor = 0;
        }
        else
        {
            _serverTargetEntityId = targeted.Identity.Id;
            _serverTargetColor = serverColor;
            _playerLevelAtServerTargetColor = PlayerEntity.Instance != null && PlayerEntity.Instance.Stats != null
                ? PlayerEntity.Instance.Stats.Level
                : 0;
            _hasServerTargetColor = true;
        }

        TargetWindow.Instance?.RefreshTargetColor();
    }

    public static bool IsMonsterOrAttackable(Entity target)
    {
        if (target == null || target.Identity == null)
        {
            return false;
        }

        EntityType type = target.Identity.EntityType;
        if (type == EntityType.Player || type == EntityType.User || type == EntityType.Item)
        {
            return false;
        }

        return type == EntityType.Monster || target.Identity.IsAttackable;
    }

    public bool TryGetMonsterLevelDiff(Entity target, out int diff)
    {
        diff = 0;
        if (!IsMonsterOrAttackable(target))
        {
            return false;
        }

        if (PlayerEntity.Instance == null || PlayerEntity.Instance.Stats == null)
        {
            return false;
        }

        int currentPlayerLevel = PlayerEntity.Instance.Stats.Level;

        if (_hasServerTargetColor && target.Identity.Id == _serverTargetEntityId)
        {
            // Server MyTargetSelected sends (playerLevel - targetLevel), so (mobLevel - playerLevel) is negated
            // and shifted by any local player level-ups since the packet arrived.
            int playerLevelDelta = currentPlayerLevel - _playerLevelAtServerTargetColor;
            diff = (-_serverTargetColor) - playerLevelDelta;
            return true;
        }

        if (target.Stats != null && target.Stats.Level > 0)
        {
            diff = target.Stats.Level - currentPlayerLevel;
            return true;
        }

        return false;
    }

    public void ClearAttackTarget()
    {
        _attackTarget = null;
    }

    public bool IsAttackTargetSet()
    {
        return _target != null && _attackTarget != null && _target.Identity.Id == _attackTarget.Identity.Id;
    }

    public bool HasTarget()
    {
        return _target != null && _target.transform != null;
    }

    public bool HasAttackTarget()
    {
        return _attackTarget != null;
    }

    public void ClearTarget()
    {
        ClearTarget(true);
    }

    public void ClearTarget(bool request)
    {
        _hasServerTargetColor = false;
        _serverTargetColor = 0;
        _serverTargetEntityId = -1;
        _playerLevelAtServerTargetColor = 0;

        if (HasTarget())
        {
            if (PlayerCombat.Instance.TargetId != -1)
            {
                if (request)
                {
                    GameClient.Instance.ClientPacketHandler.SendRequestCancel(false);
                }
                PlayerCombat.Instance.TargetId = -1;
                PlayerCombat.Instance.Target = null;
            }

            _target = null;
            _attackTarget = null;
        }
    }

    void Update()
    {
        if (PlayerEntity.Instance == null)
        {
            return;
        }

        if (HasTarget())
        {
            // _target.Distance = Vector3.Distance(
            //     PlayerController.Instance.transform.position,
            //     _target.Data.ObjectTransform.position);
        }
        else
        {
            ClearTarget();
        }
    }

    internal void SetAttackTarget(Entity attackTarget)
    {
        _attackTarget = attackTarget;
    }
}

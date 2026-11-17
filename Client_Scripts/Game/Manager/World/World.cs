using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class World : MonoBehaviour
{
    private EventProcessor _eventProcessor;
    private WorldSpawner _worldSpawner;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask _entityMask;
    [SerializeField] private LayerMask _simpleEntityMask;
    [SerializeField] private LayerMask _entityClickAreaMask;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private LayerMask _clickThroughMask;
    [SerializeField] private LayerMask _groundMask;

    [SerializeField] private bool _offlineMode = false;

    public bool OfflineMode { get { return _offlineMode; } }
    public LayerMask GroundMask { get { return _groundMask; } }

    private static World _instance;
    public static World Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(this);
        }

        _eventProcessor = EventProcessor.Instance;
        _worldSpawner = GetComponent<WorldSpawner>();
    }

    void OnDestroy()
    {
        _instance = null;
    }

    void Start()
    {
        UpdateMasks();
    }

    void UpdateMasks()
    {
        NameplatesManagerGame.Instance.SetMask(_entityMask);
        Geodata.Instance.ObstacleMask = _obstacleMask;
        ClickManager.Instance.SetMasks(_entityClickAreaMask, _clickThroughMask);
        CameraController.Instance.SetMask(_obstacleMask);
        TargetManager.Instance.SetMask(_simpleEntityMask);
    }

    public float GetGroundHeight(Vector3 pos)
    {
        RaycastHit hit;
        // Primary: Exact visual surface of the terrain/ground mesh
        if (Physics.Raycast(pos + Vector3.up * 1.5f, Vector3.down, out hit, 4.0f, _groundMask))
        {
            return hit.point.y;
        }

        // Secondary raycast from higher up in case entity is currently submerged
        if (Physics.Raycast(pos + Vector3.up * 3.0f, Vector3.down, out hit, 8.0f, _groundMask))
        {
            return hit.point.y;
        }

        // Fallback: Geodata node elevation if no physics collider is present
        if (Geodata.Instance != null && Geodata.Instance.Loaded)
        {
            try
            {
                Node node = Geodata.Instance.GetClosestNodeAt(pos);
                if (node != null)
                {
                    return node.center.y;
                }
            }
            catch (Exception) { }
        }

        return pos.y;
    }

    public Task UpdateObjectPosition(int id, Vector3 position)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(position);
        });
    }

    public Task AdjustObjectPositionAndRotation(int id, Vector3 position, int heading)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {

            e.Identity.Position = position;
            e.Identity.Heading = heading;

            float rotation = VectorUtils.ConvertRotToUnity(heading);
            if (id == GameClient.Instance.CurrentPlayerId)
            {
                if (PlayerEntity.Instance != null)
                {
                    float dist = VectorUtils.Distance2D(PlayerEntity.Instance.transform.position, position);
                    if (dist > 2.0f)
                    {
                        PlayerTransformReceive.Instance.SetNewPosition(position);
                    }
                }
                NetworkCharacterControllerShare.Instance.Heading = heading;
            }
            else
            {
                NetworkEntityReferenceHolder holder = e.ReferenceHolder as NetworkEntityReferenceHolder;
                if (holder != null && holder.NetworkTransformReceive != null)
                {
                    holder.NetworkTransformReceive.SetFinalRotation(rotation);
                    if (holder.NetworkCharacterControllerReceive == null || !holder.NetworkCharacterControllerReceive.IsMoving())
                    {
                        holder.NetworkTransformReceive.SetNewPosition(position);
                    }
                    else
                    {
                        float dist = VectorUtils.Distance2D(e.transform.position, position);
                        if (dist > 2.0f)
                        {
                            holder.NetworkTransformReceive.SetNewPosition(position);
                        }
                    }
                }
            }
        });
    }

    public Task NpcHtmlReceived(int objectId, string html, int itemId)
    {
        _eventProcessor.QueueEvent(() => NpcHtmlWindow.Instance.RefreshContent(objectId, html, itemId));
        if (objectId == 0)
        {
            return Task.CompletedTask;
        }
        return _worldSpawner.ExecuteWithEntityAsync(objectId, e =>
        {
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.LookAt(PlayerEntity.Instance.transform);
        });
    }

    public Task UpdateObjectRotation(int id, float angle)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetFinalRotation(angle);
        });
    }

    public Task UpdateObjectDestination(int id, Vector3 currentPosition, Vector3 destination)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            e.Identity.Position = currentPosition;

            NetworkEntityReferenceHolder referenceHolder = e.ReferenceHolder as NetworkEntityReferenceHolder;
            if (referenceHolder == null || referenceHolder.NetworkCharacterControllerReceive == null) return;

            float dist = VectorUtils.Distance2D(e.transform.position, currentPosition);
            if (dist > 2.0f)
            {
                currentPosition.y = GetGroundHeight(currentPosition);
                e.transform.position = currentPosition;
            }

            Entity target = e.Combat.AttackTarget ?? e.Combat.Target;
            if (target == null && e.Identity.EntityType == EntityType.Monster && PlayerEntity.Instance != null)
            {
                float distToPlayer = VectorUtils.Distance2D(destination, PlayerEntity.Instance.transform.position);
                if (distToPlayer < 2.0f)
                {
                    target = PlayerEntity.Instance;
                    e.Combat.Target = target;
                    e.Combat.AttackTarget = target;
                }
            }

            float attackRange = target == null ? 0 : WorldCombat.Instance.GetRealAttackRange(e, target);
            referenceHolder.NetworkCharacterControllerReceive.SetDestination(destination, attackRange);
            referenceHolder.NetworkTransformReceive.SetNewPosition(destination);
            referenceHolder.NetworkTransformReceive.LookAt(destination);
        });
    }

    public Task UpdateObjectMoveDirection(int id, Vector3 position, Vector3 direction, float verticalVelocity, long timestamp)
    {

        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkCharacterControllerReceive.UpdateMoveDirection(position, direction, verticalVelocity, timestamp);
        });
    }

    public Task ObjectStoppedMove(int id, Vector3 position, int heading)
    {
        return _worldSpawner.ExecuteWithEntityAsync(id, e =>
        {
            // Debug.LogWarning($"[{e.transform.name}] ObjectStoppedMove");
            e.Identity.Position = position;
            e.Identity.Heading = heading;

            float rotation = VectorUtils.ConvertRotToUnity(heading);

            if (id == GameClient.Instance.CurrentPlayerId)
            {
                if (PlayerEntity.Instance != null)
                {
                    float dist = VectorUtils.Distance2D(PlayerEntity.Instance.transform.position, position);
                    if (dist > 2.0f)
                    {
                        PlayerTransformReceive.Instance.SetNewPosition(position);
                    }
                }
                if (PathFinderController.Instance == null || (!PathFinderController.Instance.HasPath && !PathFinderController.Instance.IsPathfinding))
                {
                    PlayerController.Instance.ResetDestination(true);
                }
            }
            else
            {
                NetworkEntityReferenceHolder holder = e.ReferenceHolder as NetworkEntityReferenceHolder;
                if (holder != null)
                {
                    holder.NetworkTransformReceive?.SetFinalRotation(rotation);
                    holder.NetworkTransformReceive?.SetNewPosition(position);
                    holder.NetworkCharacterControllerReceive?.ResetDestination();
                }
            }
        });
    }

    public Task ChangeWaitType(int owner, ChangeWaitTypePacket.WaitType moveType, Vector3 entityPosition)
    {
        return _worldSpawner.ExecuteWithEntityAsync(owner, e =>
        {
            if (owner != GameClient.Instance.CurrentPlayerId)
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(entityPosition);

                //Verify position on server
                // NetworkTransformShare.Instance.SharePosition();
            }
            else
            {
                PlayerTransformReceive.Instance.SetNewPosition(entityPosition);
            }

            e.UpdateWaitType(moveType);
        });
    }

    public Task ChangeMoveType(int owner, bool running)
    {
        return _worldSpawner.ExecuteWithEntityAsync(owner, e =>
                {
                    e.UpdateMoveType(running);
                });
    }

    public Task EntityTeleported(int entityId, Vector3 teleportTo, bool loadingScreen)
    {
        return _worldSpawner.ExecuteWithEntityAsync(entityId, e =>
        {
            if (entityId != GameClient.Instance.CurrentPlayerId)
            {
                ((NetworkEntityReferenceHolder)e.ReferenceHolder).NetworkTransformReceive.SetNewPosition(teleportTo);
            }
            else
            {
                // if (loadingScreen)
                // {
                //     Debug.LogWarning("TODO: HANDLE TELEPORT LOADING SCREEN");
                // }
                // PlayerTransformReceive.Instance.SetNewPosition(teleportTo);
                // GameClient.Instance.ClientPacketHandler.NotifyAppearing();
                GameManager.Instance.NotifyEvent(GameEvent.TELEPORTING);
            }

            e.Identity.Position = teleportTo;
        });
    }

    public Task SocialActionReceived(int objectId, int action)
    {
        return _worldSpawner.ExecuteWithEntityAsync(objectId, e =>
        {
            if (action == SocialActionPacket.LEVELUP_ACTION)
            {
                Debug.Log("Entity level up!");
                WorldCombat.Instance.EntityCastSkill(e, 2122);
                if ((PlayerEntity.Instance != null && e == PlayerEntity.Instance) ||
                    (TargetManager.Instance != null && TargetManager.Instance.Target == e))
                {
                    TargetWindow.Instance?.RefreshTargetColor();
                }
            }
            else
            {
                e.AnimationController.Emote(action);
            }
        });
    }

    public void DestroyWorld()
    {
        ProjectileManager.Instance.DestroyAllProjectiles();
        ParticleManager.Instance.DestroyAllParticles();
        WorldSpawner.Instance.DestroyEntities();
        WorldSpawner.Instance.ClearEntities();
    }
}

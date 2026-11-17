using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PathFinderController : MonoBehaviour
{
    [Header("Path data")]
    [SerializeField] private Node _startNode;
    [SerializeField] private Node _targetNode;
    [SerializeField] private Vector3 _targetDestination;
    [SerializeField] private float _defaultDestinationThreshold = 0.10f;
    [SerializeField] private float _currentDestinationThreshold = 0.10f;
    [SerializeField] private List<Node> _path;
    [SerializeField] private bool _simplifyPath = true;

    [Header("Ground check")]
    [SerializeField] private ObjectData _collidingWith;
    [SerializeField] private LayerMask _ignoredLayers;
    [SerializeField] private bool _grounded;

    [SerializeField] private bool _debugPathFinder;

    [Header("Obstacle Clearance")]
    [SerializeField] private bool _applyObstacleClearance = true;
    [SerializeField] private float _clearanceMultiplier = 0.5f;

    private CharacterController _characterController;

    private static PathFinderController _instance;
    public static PathFinderController Instance { get { return _instance; } }
    public bool HasPath => _path != null && _path.Count > 0;
    public bool HasIntermediateWaypoints => _path != null && _path.Count > 1;
    public bool IsPathfinding { get; private set; }

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

        if (_path == null)
        {
            _path = new List<Node>();
        }
    }

    void OnDestroy()
    {
        _instance = null;
    }

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    public void Update()
    {
        // Update initial node
        if (_characterController.isGrounded)
        {
            try
            {
                _startNode = GetSafeNodeAt(transform.position);
            }
            catch (Exception)
            {
                _startNode = null;
            }
        }

        // Reset path when user input
        if (InputManager.Instance.Move)
        {
            ClearPath();
        }
    }

    private Node GetSafeNodeAt(Vector3 pos)
    {
        if (Geodata.Instance == null)
            return null;

        try
        {
            Node node = Geodata.Instance.GetNodeAt(pos);
            if (node != null) return node;
        }
        catch (Exception) { }

        try
        {
            return Geodata.Instance.GetClosestNodeAt(pos);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public void ClearPath()
    {
        if (_path != null)
        {
            _path.Clear();
        }
        IsPathfinding = false;
    }

    public void FixedUpdate()
    {
        Vector3 flatTransformPos = VectorUtils.To2D(transform.position);
        Vector3 flatDestPos = VectorUtils.To2D(_targetDestination);

        if (_path != null && _path.Count > 0)
        {
            Vector3 currentWaypoint = (_path.Count > 1) ? VectorUtils.To2D(_path[0].center) : flatDestPos;
            float reachThreshold = (_path.Count > 1) ? Mathf.Max(0.5f, Geodata.Instance != null ? Geodata.Instance.NodeSize : 0.5f) : _currentDestinationThreshold;

            if (Vector3.Distance(currentWaypoint, flatTransformPos) < reachThreshold)
            {
                _path.RemoveAt(0);
                if (_path.Count > 0)
                {
                    currentWaypoint = (_path.Count > 1) ? VectorUtils.To2D(_path[0].center) : flatDestPos;
                    reachThreshold = (_path.Count > 1) ? Mathf.Max(0.5f, Geodata.Instance != null ? Geodata.Instance.NodeSize : 0.5f) : _currentDestinationThreshold;
                }
            }

            if (_path.Count > 0)
            {
                PlayerController.Instance.SetDestination(currentWaypoint, reachThreshold);
            }
        }
        else
        {
            if (Vector3.Distance(flatDestPos, flatTransformPos) < _currentDestinationThreshold)
            {
                if (PlayerController.Instance.RunningToDestination)
                {
                    PlayerController.Instance.ResetDestination(true);
                }
                //   if(PlayerController.Instance.RunningToDestination && PlayerCombatController.Instance.RunningToTarget) {
                //      PlayerCombatController.Instance.OnReachingTarget();
                //   }

            }
        }
    }

    public void MoveTo(Vector3 destination)
    {
        MoveTo(destination, _defaultDestinationThreshold, null);
    }

    public void MoveTo(Vector3 destination, Action callback)
    {
        _currentDestinationThreshold = _defaultDestinationThreshold;
        MoveTo(destination, _defaultDestinationThreshold, callback);
    }

    public void MoveTo(Vector3 destination, float stopAtRange)
    {
        MoveTo(destination, stopAtRange, null);
    }

    public void MoveTo(Vector3 destination, float stopAtRange, Action moveCallback)
    {
        _currentDestinationThreshold = stopAtRange;
        _targetDestination = destination;

        // Immediately resolve current player node from position (do not rely on cached/stale node)
        _startNode = GetSafeNodeAt(transform.position);
        Node node = GetSafeNodeAt(destination);

        if (node != null && _startNode != null)
        {
            _targetNode = node;
            IsPathfinding = true;

            PathFinderFactory.Instance.RequestPathfind(_startNode, _targetNode, (callback) =>
            {
                IsPathfinding = false;
                if (callback == null || callback.Count == 0)
                {
                    ClearPath();
                    PlayerController.Instance.SetDestination(_targetDestination, _currentDestinationThreshold);
                }
                else
                {
                    Debug.Log("Found path with " + callback.Count + " node(s).");
                    if (_simplifyPath)
                    {
                        float clearance = 0f;
                        if (_applyObstacleClearance)
                        {
                            float colRadius = 0.2f;
                            if (_characterController != null && _characterController.radius > 0)
                            {
                                colRadius = _characterController.radius;
                            }
                            else if (PlayerEntity.Instance != null && PlayerEntity.Instance.Appearance != null && PlayerEntity.Instance.Appearance.CollisionRadius > 0)
                            {
                                colRadius = PlayerEntity.Instance.Appearance.CollisionRadius;
                            }
                            clearance = colRadius * _clearanceMultiplier;
                        }

                        _path = PathFinder.SmoothPath(callback, clearance);
                    }
                    else
                    {
                        _path = callback;
                    }

                    if (_path != null && _path.Count > 0)
                    {
                        Vector3 currentWaypoint = (_path.Count > 1) ? VectorUtils.To2D(_path[0].center) : VectorUtils.To2D(_targetDestination);
                        float reachThreshold = (_path.Count > 1) ? Mathf.Max(0.5f, Geodata.Instance != null ? Geodata.Instance.NodeSize : 0.5f) : _currentDestinationThreshold;
                        PlayerController.Instance.SetDestination(currentWaypoint, reachThreshold);
                    }
                    else
                    {
                        PlayerController.Instance.SetDestination(_targetDestination, _currentDestinationThreshold);
                    }
                }

                StartCoroutine(WaitForFixedUpdate(moveCallback));
            });

        }
        else
        {
            _targetNode = null;
            ClearPath();
            PlayerController.Instance.SetDestination(_targetDestination, _currentDestinationThreshold);

            StartCoroutine(WaitForFixedUpdate(moveCallback));
        }
    }

    private IEnumerator WaitForFixedUpdate(Action moveCallback)
    {
        yield return new WaitForFixedUpdate();
        if (moveCallback != null)
        {
            moveCallback();
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (_ignoredLayers != (_ignoredLayers | (1 << hit.gameObject.layer)))
        {
            _collidingWith = new ObjectData(hit.gameObject);
        }
        else
        {
            _collidingWith = null;
        }
    }

    void OnDrawGizmos()
    {
        if (!_debugPathFinder || !Application.isPlaying)
            return;

        float nodeSize = Geodata.Instance.NodeSize;
        Vector3 cubeSize = new Vector3(nodeSize - nodeSize / 10f, 0.1f, nodeSize - nodeSize / 10f);

        Gizmos.color = Color.yellow;

        if (_targetNode != null)
        {
            Gizmos.DrawCube(_targetNode.center + Vector3.up * 0.1f, cubeSize);
        }

        Gizmos.color = Color.green;
        if (_startNode != null)
        {
            Gizmos.DrawCube(_startNode.center + Vector3.up * 0.1f, cubeSize);
        }

        Gizmos.color = Color.white;

        if (_path != null && _path.Count > 0)
        {
            foreach (var node in _path)
            {
                Gizmos.DrawCube(node.center + Vector3.up * 0.1f, cubeSize);
            }
        }
    }
}

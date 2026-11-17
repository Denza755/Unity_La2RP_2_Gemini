using System;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
    private PathFinderFactory.PathfindingJobComplete _completeCallback;
    private List<Node> _foundPath;
    private float _nodeSize;

    private Node _startPosition;
    private Node _endPosition;
    private int _maximumLoopCount;
    private volatile bool jobDone = false;

    public bool JobDone { get { return jobDone; } }

    public PathFinder(Node start, Node target, PathFinderFactory.PathfindingJobComplete callback, float nodeSize, int maximumLoopCount) {
        _startPosition = start;
        _endPosition = target;
        _completeCallback = callback;
        _nodeSize = nodeSize;
        _maximumLoopCount = maximumLoopCount;
    }

    public void FindPath() {
        try {
            _foundPath = SearchByClosest(_startPosition, _endPosition);
        } catch (Exception e) {
            Debug.LogWarning("Error while finding path: " + e.Message);
        }

        jobDone = true;
    }

    public void NotifyComplete() {
        if(_completeCallback != null) {
            _completeCallback(_foundPath);
        }
    }

    private class NodeComparer : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            int cmp = x.fCost.CompareTo(y.fCost);
            if (cmp != 0) return cmp;
            return x.hCost.CompareTo(y.hCost);
        }
    }

    private static readonly NodeComparer _comparer = new NodeComparer();

    public List<Node> SearchByClosest(Node start, Node end)
    {
        HashSet<Vector3> closedSet = new HashSet<Vector3>();
        Dictionary<Vector3, Node> openDict = new Dictionary<Vector3, Node>();
        List<Node> openList = new List<Node>();

        start.parentNode = null;
        start.gCost = 0;
        start.hCost = Vector3.Distance(start.center, end.center);
        start.cost = start.fCost;

        openList.Add(start);
        openDict[start.nodeIndex] = start;

        Node closestNode = start;
        float closestDist = start.hCost;

        int i = 0;
        while (openList.Count > 0 && i++ < _maximumLoopCount)
        {
            Node node = openList[0];
            openList.RemoveAt(0);
            openDict.Remove(node.nodeIndex);

            if (node.nodeIndex.Equals(end.nodeIndex))
            {
                return ConstructPathFull(node);
            }

            closedSet.Add(node.nodeIndex);

            float distToEnd = Vector3.Distance(node.center, end.center);
            if (distToEnd < closestDist)
            {
                closestDist = distToEnd;
                closestNode = node;
            }

            Node[] neighbors = GetNeighbours(node.center);
            if (neighbors == null)
            {
                continue;
            }

            foreach (Node neighbor in neighbors)
            {
                if (neighbor == null)
                {
                    continue;
                }

                if (closedSet.Contains(neighbor.nodeIndex))
                {
                    continue;
                }

                float tentativeGCost = node.gCost + Vector3.Distance(node.center, neighbor.center);

                if (openDict.TryGetValue(neighbor.nodeIndex, out Node existingOpen))
                {
                    if (tentativeGCost < existingOpen.gCost)
                    {
                        openList.Remove(existingOpen);
                        existingOpen.parentNode = new Node(node);
                        existingOpen.gCost = tentativeGCost;
                        existingOpen.cost = existingOpen.fCost;
                        int insertIdx = openList.BinarySearch(existingOpen, _comparer);
                        if (insertIdx < 0) insertIdx = ~insertIdx;
                        openList.Insert(insertIdx, existingOpen);
                    }
                }
                else
                {
                    neighbor.parentNode = new Node(node);
                    neighbor.gCost = tentativeGCost;
                    neighbor.hCost = Vector3.Distance(neighbor.center, end.center);
                    neighbor.cost = neighbor.fCost;

                    int insertIdx = openList.BinarySearch(neighbor, _comparer);
                    if (insertIdx < 0) insertIdx = ~insertIdx;
                    openList.Insert(insertIdx, neighbor);
                    openDict[neighbor.nodeIndex] = neighbor;
                }
            }
        }

        if (closestNode != null && closestNode != start)
        {
            return ConstructPathFull(closestNode);
        }

        return null;
    }

    public List<Node> ConstructPathFull(Node node) {
        List<Node> path = new List<Node>();
        while (node.parentNode != null) {
            path.Insert(0, node);
            node = node.parentNode;
        }
        return path;
    }

    private Node[] GetNeighbours(Vector3 nodePos) {
        Node[] returnList = new Node[8];
        short i = 0;

        for(int x = -1; x <= 1; x++) {
            for(int z = -1; z <= 1; z++) {
                if(x == 0 && z == 0)
                    continue;

                try {
                    Vector3 neighborPos = new Vector3(nodePos.x + x * _nodeSize,
                            nodePos.y,
                            nodePos.z + z * _nodeSize);
                    string mapId = Geodata.Instance.GetCurrentMap(neighborPos);

                    Node node = null;
                    try {
                        node = Geodata.Instance.GetNodeAt(neighborPos, mapId);
                    } catch(Exception) { }

                    if(node == null) {
                        neighborPos.y = nodePos.y + _nodeSize;
                        try {
                            node = Geodata.Instance.GetNodeAt(neighborPos, mapId);
                        } catch(Exception) { }
                    }

                    if(node == null) {
                        neighborPos.y = nodePos.y - _nodeSize;
                        try {
                            node = Geodata.Instance.GetNodeAt(neighborPos, mapId);
                        } catch(Exception) { }
                    }

                    if(node != null) {
                        returnList[i++] = node;
                    }
                } catch(Exception) {
                    Debug.Log("Node neighbor could not be found.");
                }
            }
        }

        return returnList;
    }

    /*private float GetDistance(Node posA, Node posB) {
        return Vector3.Distance(posA.center, posB.center);
    }*/

    private float GetDistance(Node posA, Node posB) {
        //We find the distance between each node
        //not much to explain here

        float distX = Mathf.Abs(posA.worldPosition.x - posB.worldPosition.x);
        float distZ = Mathf.Abs(posA.worldPosition.z - posB.worldPosition.z);
        float distY = Mathf.Abs(posA.worldPosition.y - posB.worldPosition.y);

        if(distX > distZ) {
            return 14 * distZ + 10 * (distX - distZ) + 10 * distY;
        }

        return 14 * distX + 10 * (distZ - distX) + 10 * distY;
    }

    public static List<Node> SmoothPath(List<Node> path, float clearance = 0f) {
        if (path == null || path.Count == 0)
            return path;

        List<Node> waypoints = new List<Node>();

        int currentNode = 0;
        Vector3 yOffset = Vector3.up * (Geodata.Instance != null ? Geodata.Instance.NodeSize * 1.5f : 0.75f);
        LayerMask obstacleMask = Geodata.Instance != null ? Geodata.Instance.ObstacleMask : LayerMask.GetMask("Obstacle");

        for(int i = 0; i < path.Count; i++) {
            Vector3 origin = path[currentNode].center;
            Vector3 destination = path[i].center;

            bool cantSeeTarget = false;
            Vector3 toDest = (destination + yOffset) - (origin + yOffset);
            float dist = toDest.magnitude;

            if (dist > 0.001f)
            {
                if (clearance > 0.01f)
                {
                    // SphereCast with radius = clearance ensures the line of sight maintains clearance from obstacles
                    if (Physics.SphereCast(origin + yOffset, clearance, toDest.normalized, out RaycastHit hit, dist, obstacleMask))
                    {
                        if (hit.distance > 0.01f)
                        {
                            cantSeeTarget = true;
                        }
                    }
                }

                if (!cantSeeTarget)
                {
                    cantSeeTarget = Physics.Linecast(destination + yOffset, origin + yOffset, obstacleMask);
                }
            }

            if(cantSeeTarget) {
                waypoints.Add(new Node(path[i - 1]));
                currentNode = i - 1;
            }
        }

        waypoints.Add(new Node(path[path.Count - 1]));

        // Push intermediate detour waypoints away from obstacles by clearance (half collision radius)
        if (clearance > 0.01f && waypoints.Count > 1)
        {
            ApplyClearanceOffset(waypoints, clearance, yOffset, obstacleMask);
        }

        return waypoints;
    }

    private static void ApplyClearanceOffset(List<Node> waypoints, float clearance, Vector3 yOffset, LayerMask obstacleMask)
    {
        // Only adjust intermediate detour waypoints (exclude final target destination at waypoints.Count - 1)
        for (int k = 0; k < waypoints.Count - 1; k++)
        {
            Vector3 wpCenter = waypoints[k].center;
            Collider[] hits = Physics.OverlapSphere(wpCenter + yOffset, 1.2f, obstacleMask);
            if (hits == null || hits.Length == 0)
                continue;

            Vector3 totalPushDir = Vector3.zero;
            int pushCount = 0;

            foreach (Collider col in hits)
            {
                if (col == null) continue;

                Vector3 closest = col.ClosestPoint(wpCenter + yOffset);
                Vector3 diff = (wpCenter + yOffset) - closest;
                diff.y = 0; // horizontal push on XZ plane
                float dist = diff.magnitude;

                if (dist > 0.001f && dist < 1.0f)
                {
                    // Closer obstacles produce stronger outward push
                    totalPushDir += diff.normalized * (1.0f - (dist / 1.0f));
                    pushCount++;
                }
            }

            if (pushCount > 0 && totalPushDir.sqrMagnitude > 0.001f)
            {
                Vector3 pushDir = totalPushDir.normalized;
                Vector3 candidatePos = wpCenter + pushDir * clearance;

                // Ensure the offset waypoint does not enter another obstacle
                if (!Physics.CheckSphere(candidatePos + yOffset, clearance * 0.8f, obstacleMask))
                {
                    waypoints[k].center = candidatePos;
                }
            }
        }
    }
}
using System;
using UnityEngine;

[System.Serializable]
public class Node : IEquatable<Node> {
	public Vector3 nodeIndex;
	public Vector3 worldPosition;
	public Vector3 center;

	public Node parentNode;
	public float cost;
	public float gCost;
	public float hCost;
	public float fCost => gCost + hCost;

	public Node() {}

	public Node(Node original) {
		this.nodeIndex = original.nodeIndex;
		this.worldPosition = original.worldPosition;
		this.center = original.center;
		this.parentNode = original.parentNode;
		this.cost = original.cost;
		this.gCost = original.gCost;
		this.hCost = original.hCost;
	}

	public Node(Vector3 nodeIndex, Vector3 worldPosition, float size) {
		this.nodeIndex = nodeIndex;
		this.worldPosition = worldPosition;
		center = new Vector3 (worldPosition.x + size / 2f, worldPosition.y, worldPosition.z + size / 2f);
	}

	public bool Equals(Node other) {
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		return nodeIndex.Equals(other.nodeIndex);
	}

	public override bool Equals(object obj) {
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((Node)obj);
	}

	public override int GetHashCode() {
		return nodeIndex.GetHashCode();
	}
}

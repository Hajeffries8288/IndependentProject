using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node>
{

	[HideInInspector] public bool walkable;
	[HideInInspector] public bool isCoreNode;
	[HideInInspector] public Vector2 worldPosition;
	[HideInInspector] public int gridX;
	[HideInInspector] public int gridY;

	[HideInInspector] public int gCost;
	[HideInInspector] public int hCost;
	[HideInInspector] public Node parent;

	private int heapIndex;

	public Node(bool _walkable, Vector2 _worldPos, int _gridX, int _gridY)
	{
		walkable = _walkable;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
	}

	public int fCost
	{
		get
		{
			return gCost + hCost;
		}
	}

	public int HeapIndex
	{
		get
		{
			return heapIndex;
		}
		set
		{
			heapIndex = value;
		}
	}

	public int CompareTo(Node nodeToCompare)
	{
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0)
		{
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingNode : MonoBehaviour               //Maybe find a new name because this isn't actually for pathfinding
{
    [HideInInspector] public int gCost;
    [HideInInspector] public int hCost;
    [HideInInspector] public int fCost;

    [HideInInspector] public PathfindingNode parentNode;
}

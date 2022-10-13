using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingNode : MonoBehaviour
{
    public int gCost;
    public int hCost;
    public int fCost;

    public PathfindingNode parentNode;
}

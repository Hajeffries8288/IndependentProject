using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Grid : MonoBehaviour          //Needs to convert into 2D version
{
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] grid;

    float nodeDiamiter;
    int gridSizeX, gridSizeY;

    private void Start()
    {
        nodeDiamiter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiamiter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiamiter);

        GridGenerate();
    }

    private void GridGenerate()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 localBottomLeftTile = transform.localPosition - Vector3.right * gridWorldSize.x/2 - Vector3.up * gridWorldSize.y/2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = localBottomLeftTile + Vector3.right * (x * nodeDiamiter + nodeRadius) + Vector3.up * (y * nodeDiamiter + nodeRadius);
                bool walkable = Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask);
                print(worldPoint + " " + walkable);
                grid[x, y] = new Node(walkable, worldPoint);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.localPosition, new Vector3(gridWorldSize.x, gridWorldSize.y, 0));

        if (grid != null)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n.worldPosition, new Vector3(1, 1, 1) * (nodeDiamiter -.1f));
            }
        }
    }
}

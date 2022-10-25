using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _Grid : MonoBehaviour
{
    BoxCollider2D boxCollider;

    public int gridWidth;
    public int gridHight;
    public float size;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();

        GridGenerate(gridWidth, gridHight, size);
    }

    private void GridGenerate(int width, int hight, float size)
    {
        Vector3 localBottomLeftTile = transform.localPosition - Vector3.right * width / 2 - Vector3.up * hight / 2;

        for (int x = width; x > 0; x--)
        {
            for (int y = hight; y > 0; y--)
            {
                Vector3 localPoint = localBottomLeftTile - Vector3.right * (x * size) - Vector3.up * (y * size);
                //Needs to check weather or not there is a collider within the given area for the size of the tiles in the grid
            }
        }
    }
}

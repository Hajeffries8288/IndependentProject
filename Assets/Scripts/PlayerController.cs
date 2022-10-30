using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Movement
    [Header("Movement")]
    public float speed;
    public float shiftSpeed;
    public float maxZoomOut;

    private float mouseScroll;
    private GameObject ship;
    private Camera mainCamera;

    //Building                                  NOTE: Try to find a way to have less bools
    [Header("Building")]
    public static bool building;
    public float multible;
    public GameObject[] buildableObjects;

    private GameObject buildingTile;
    private int buildingIndex;
    private int tilesNextToObjectBuildingIndex;
    private bool autoRotate;
    private bool rotate;
    private bool destroy;

    //DestroyPlacedTiles
    [Header("DestroyPlacedTiles")]
    public GameObject destroyGameObject;

    private GameObject instDestroyGameObject;

    //AllObjects
    [HideInInspector] public static List<GameObject> allObjects;

    //Grid
    private _Grid grid;

    private void Start()        // Start is called before the first frame update
    {
        //Movement
        mouseScroll = 5;
        ship = GameObject.Find("Ship");

        //AllObjects
        allObjects = new List<GameObject>(FindObjectsOfType<GameObject>());

        //Camera
        mainCamera = Camera.main;

        //Grid
        grid = GameObject.Find("GridGen").GetComponent<_Grid>();
    }

    private void Awake()        //Awake is called before the start
    {
        
    }

    private void Update()       // Update is called once per frame
    {
        Movement();

        Clicking();

        Building();

        Destroying();

        Debuging();
    }

    private void OnDrawGizmos()
    {
        
    }

    //Below this is the main functions

    private void Movement()
    {
        //WASD 
        bool holdingShift = Input.GetKey(KeyCode.LeftShift);
        mouseScroll += -Input.mouseScrollDelta.y;
        mouseScroll = Mathf.Clamp(mouseScroll, 1, maxZoomOut);
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 direction = input.normalized;
        Vector2 velocity = holdingShift ? direction * shiftSpeed : direction * speed;

        //Scroll
        transform.Translate(velocity * Time.deltaTime);
        mainCamera.orthographicSize = mouseScroll;

        //Follow/Unfollow Ship
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (transform.parent == null)
            {
                transform.parent = ship.transform;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.parent = null;
            }
        }
    }

    private void Clicking()
    {
        if (Input.GetButtonDown("Fire1") && !building)
        {
            RaycastHit2D raycast2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, 1 << 0);
            Collider2D hit2D = raycast2D.collider;
        }
    }

    private void Building()
    {
        if (building)
        {
            if (instDestroyGameObject)
            {
                Destroy(instDestroyGameObject);
                destroy = false;
            }

            //Creates the tile that the player is trying to build
            if (!buildingTile)
            {
                buildingTile = Instantiate(buildableObjects[buildingIndex], ship.transform);
            }

            //Local variables 
            BoxCollider2D tileBuildingBoxCollider = buildingTile.GetComponent<BoxCollider2D>();
            Collider2D tileBuildingCollider = buildingTile.GetComponent<Collider2D>();
            GameObject[] tilesNextToTileBuilding;
            bool canPlace = false;

            //This is how the tile system works for placeing the tiles in the correct positions
            buildingTile.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            buildingTile.transform.localPosition = tileBuildingBoxCollider.size.y % 2 == 0 ? new Vector3(Mathf.Round(buildingTile.transform.localPosition.x / multible) * multible, Mathf.Round(buildingTile.transform.localPosition.y / multible) * multible - .5f, 0) : new Vector3(Mathf.Round(buildingTile.transform.localPosition.x / multible) * multible, Mathf.Round(buildingTile.transform.localPosition.y / multible) * multible, 0);
            tilesNextToTileBuilding = tileBuildingBoxCollider.size.y % 2 == 0 ? FindObjectsNextToObjectDebug(buildingTile, "_Attach", 4, tileBuildingBoxCollider.size.x, tileBuildingBoxCollider.size.y - .5f).ToArray() : FindObjectsNextToObjectDebug(buildingTile, "_Attach", 4, tileBuildingBoxCollider.size.x, tileBuildingBoxCollider.size.y).ToArray();

            //Checks if there is a collider and if so then disable it 
            if (tileBuildingCollider)
            {
                tileBuildingCollider.enabled = false;
            }

            //Checks if this object should be auto rotated to a tile before placeing 
            if (autoRotate)
            {
                GameObject[] tilesNextToTileBuildingAutoRotate = FindObjectsNextToObject(buildingTile, "_Attach", 4);

                if (Input.GetKeyDown(KeyCode.R) && tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex] && tilesNextToObjectBuildingIndex < 4)
                {
                    tilesNextToObjectBuildingIndex++;
                }
                if (tilesNextToObjectBuildingIndex >= 4)
                {
                    tilesNextToObjectBuildingIndex = 0;
                }
                if (!tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex])
                {
                    tilesNextToObjectBuildingIndex++;
                }
                if (tilesNextToObjectBuildingIndex >= 4)
                {
                    tilesNextToObjectBuildingIndex = 0;
                }

                if (tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex] != null)
                {
                    if (tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex].transform.localPosition.x == buildingTile.transform.localPosition.x - 1)
                    {
                        buildingTile.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    }
                    else if (tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex].transform.localPosition.x == buildingTile.transform.localPosition.x + 1)
                    {
                        buildingTile.transform.localRotation = Quaternion.Euler(0, 0, -90);
                    }
                    else if (tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex].transform.localPosition.y == buildingTile.transform.localPosition.y - 1)
                    {
                        buildingTile.transform.localRotation = Quaternion.Euler(0, 0, 180);
                    }
                    else if (tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex].transform.localPosition.y == buildingTile.transform.localPosition.y + 1)
                    {
                        buildingTile.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                }
            }

            //Checks if the object can be rotated by the player
            if (rotate && Input.GetKeyDown(KeyCode.R))
            {
                buildingTile.transform.localRotation *= Quaternion.Euler(0, 0, 90);
            }

            //This is checking if there is a tile next to the tile the player is trying to build
            for (int i = 0; i < tilesNextToTileBuilding.Length; i++)
            {
                if (tilesNextToTileBuilding[i] && tilesNextToTileBuilding[i].name.Contains("_Attach"))
                {
                    canPlace = true;
                }
            }

            GameObject closestTileObjectToInstBuildableObject = FindClosestObjectThatContains(buildingTile, "_Tile");   //This variable needs to be here

            //Places/Builds the tile
            if (!destroy && Input.GetButton("Fire1") && canPlace && buildingTile.transform.localPosition != closestTileObjectToInstBuildableObject.transform.localPosition)
            {
                tileBuildingCollider.enabled = true;
                allObjects.Add(buildingTile);
                buildingTile = null;

                grid.CreateGrid();
            }

            //Stops building script
            if (Input.GetButtonUp("Fire2"))
            {
                Destroy(buildingTile.gameObject);
                buildingTile = null;
                autoRotate = false;
                rotate = false;
                building = false;
            }
        }
    }

    private void Destroying()
    {
        if (destroy)
        {
            if (buildingTile)
            {
                Destroy(buildingTile);
                building = false;
            }

            if (!instDestroyGameObject)
            {
                instDestroyGameObject = Instantiate(destroyGameObject, ship.transform);
            }

            instDestroyGameObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            instDestroyGameObject.transform.localPosition = new Vector2(Mathf.Round(instDestroyGameObject.transform.localPosition.x / multible) * multible, Mathf.Round(instDestroyGameObject.transform.localPosition.y / multible) * multible);

            GameObject closestTileObjectToCheck = FindClosestObjectThatContains(instDestroyGameObject, "_Tile");

            if (Input.GetButtonDown("Fire1") && instDestroyGameObject.transform.localPosition == closestTileObjectToCheck.transform.localPosition)
            {
                allObjects.Remove(closestTileObjectToCheck);
                Destroy(closestTileObjectToCheck);
                grid.CreateGrid();

                GameObject disconnectedObjectParent = new GameObject();
                Rigidbody2D disconnectedObjectParentRB = disconnectedObjectParent.AddComponent<Rigidbody2D>();
                disconnectedObjectParentRB.gravityScale = 0;
                disconnectedObjectParentRB.useAutoMass = true;
                disconnectedObjectParentRB.velocity = ship.GetComponent<Rigidbody2D>().velocity;

                for (int i = 0; i < allObjects.Count; i++)
                {
                    if (allObjects[i].name.Contains("_Tile"))
                    {
                        if (!IsAttached(allObjects[i].transform.localPosition, GameObject.Find("ShipCore_Attach").transform.localPosition))
                        {
                            allObjects[i].transform.parent = disconnectedObjectParent.transform;
                            allObjects[i].layer = 0;
                            allObjects[i].name = "MOTHERGODDAMBFUCKER";
                        }
                    }
                }

                grid.CreateGrid();
            }

            if (Input.GetButtonDown("Fire2"))
            {
                Destroy(instDestroyGameObject);
                destroy = false;
            }
        }
    }

    private void Debuging()
    {
        //SelectingObjects
        if (Input.GetButtonDown("Fire1") && !building)
        {
            Ray mousePosition = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(mousePosition, Mathf.Infinity, 1 << 0);

            if (hit2D)
            {
                print("Player Clicked " + hit2D.collider.name);
            }
        }

        //Mouse movement
        if (Input.GetButton("Fire3"))
        {
            //Make mouse code after disnyland or make the way it would work aka come up with ideas for how it will work at disnyland.
        }

        //Grid
        if (Input.GetKeyDown(KeyCode.Home))
        {
            grid.displayGridGizmos = grid.displayGridGizmos ? false : true;
        }
    }

    //Below this is the secondary functions

    public void SelectedObjectFromGUI(int objectSelectedIndex)
    {
        if (buildingTile)
        {
            Destroy(buildingTile);
        
        }
        buildingIndex = objectSelectedIndex;

        if (buildingIndex == 1) //Slope
        {
            destroy = false;   
        }
        if (buildingIndex == 2) //Tractorbeem
        {
            autoRotate = true;
        }
        if (buildingIndex == 3) //Connector
        {
            autoRotate = true;
        }

        building = true;
    }

    public void DestroySelected()
    {
        destroy = true;
    }

    private GameObject FindClosestObjectThatContains(GameObject fromObject, string contains)
    {
        GameObject closest = null;
        List<GameObject> objectThatContains = new List<GameObject>(0);
        List<float> distances = new List<float>(0);
        float closestF;

        for (int i = 0; i < allObjects.Count; i++)
        {
            if (allObjects[i] != null)
            {
                if (allObjects[i].name.Contains(contains))
                {
                    distances.Add((fromObject.transform.position - allObjects[i].transform.position).magnitude);
                    objectThatContains.Add(allObjects[i]);
                }
            }
        }
        closestF = Mathf.Min(distances.ToArray());
        for (int i = 0; i < objectThatContains.Count; i++)
        {
            if (objectThatContains[i] != null)
            {
                float distance = (fromObject.transform.position - objectThatContains[i].transform.position).magnitude;
                if (distance == closestF) closest = objectThatContains[i];
            }
        }

        return closest;
    }

    private GameObject[] FindObjectsNextToObject(GameObject fromObject, string contains, int ammountOfObjects)
    {
        GameObject[] objectsToReturn = new GameObject[ammountOfObjects];
        List<GameObject> objectsThatContains = new List<GameObject>();

        for (int i = 0; i < allObjects.Count; i++)
        {
            if (allObjects[i] && allObjects[i].name.Contains(contains))
            {
                objectsThatContains.Add(allObjects[i]);
            }
            else if (!allObjects[i])
            {
                allObjects.RemoveAt(i);
            }
        }

        for (int i = 0; i < objectsThatContains.Count; i++)
        {
            if ((fromObject.transform.localPosition - objectsThatContains[i].transform.localPosition).magnitude == 1)
            {
                for (int j = 0; j < ammountOfObjects; j++)
                {
                    if (objectsToReturn[j] == null)
                    {
                        objectsToReturn[j] = objectsThatContains[i];
                        break;
                    }
                }
            }
        }

        return objectsToReturn;
    }

    private List<GameObject> FindObjectsNextToObjectDebug(GameObject fromObject, string contains, int ammountOfObjects, float xBoxColliderSize, float yBoxColliderSize)
    {
        List<GameObject> objectsToReturn = new List<GameObject>();
        List<GameObject> objectsThatContains = new List<GameObject>();

        for (int i = 0; i < allObjects.Count; i++)
        {
            if (allObjects[i] && allObjects[i].name.Contains(contains))
            {
                objectsThatContains.Add(allObjects[i]);
            }
            else if (!allObjects[i])
            {
                allObjects.RemoveAt(i);
            }
        }

        for (int i = 0; i < objectsThatContains.Count; i++)
        {
            float distanceBetweenObjects = (fromObject.transform.localPosition - objectsThatContains[i].transform.localPosition).magnitude;

            //if (distanceBetweenObjects != xBoxColliderSize && distanceBetweenObjects != yBoxColliderSize) Debug.DrawLine(fromObject.transform.localPosition, objectsThatContains[i].transform.localPosition, Color.red);
            //if (distanceBetweenObjects == xBoxColliderSize || distanceBetweenObjects == yBoxColliderSize) Debug.DrawLine(fromObject.transform.localPosition, objectsThatContains[i].transform.localPosition, Color.green);

            if (distanceBetweenObjects == xBoxColliderSize || distanceBetweenObjects == yBoxColliderSize)
            {
                objectsToReturn.Add(objectsThatContains[i]);
            }
        }

        return objectsToReturn;
    }

    private GameObject FindObjectInDistanceThatContains(GameObject fromObject, string contains, float distance)
    {
        GameObject objectThatContains = null;

        for (int i = 0; i < allObjects.Count; i++)
        {
            if (allObjects[i] && allObjects[i].name.Contains(contains) && (fromObject.transform.position - allObjects[i].transform.position).magnitude <= distance)
            {
                objectThatContains = allObjects[i];
            }
            if (!allObjects[i])
            {
                allObjects.RemoveAt(i);
            }
        }

        return objectThatContains;
    }

    private bool IsAttached(Vector2 startPos, Vector2 endPos)
    {
        bool attached = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node endNode = grid.NodeFromWorldPoint(endPos);

        List<Node> openPathNodes = new List<Node>();      //Nodes to be evaluated
        HashSet<Node> closedPathNodes = new HashSet<Node>();    //Nodes that have been evaluated
        openPathNodes.Add(startNode);

        while (openPathNodes.Count > 0)
        {
            Node currentNode = openPathNodes[0];

            for (int i = 0; i < openPathNodes.Count; i++)
            {
                if (currentNode.fCost < openPathNodes[i].fCost || currentNode.fCost == openPathNodes[i].fCost && currentNode.hCost < openPathNodes[i].hCost)
                {
                    currentNode = openPathNodes[i];
                }
            }

            openPathNodes.Remove(currentNode);
            closedPathNodes.Add(currentNode);

            if (currentNode == endNode)
            {
                attached = true;
                break;
            }

            foreach (Node neighbour in grid.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedPathNodes.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openPathNodes.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, endNode);
                    neighbour.parent = currentNode;

                    if (!openPathNodes.Contains(neighbour))
                    {
                        openPathNodes.Add(neighbour);
                    }
                }
            }
        }
        return attached;
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        else return 14*dstX + 10 * (dstY - dstX);
    }
}

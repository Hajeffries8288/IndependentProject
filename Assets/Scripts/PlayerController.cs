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
    public float screenDragSpeed;

    float mouseScroll;
    Vector3 lastPosition;
    Vector3 delta;
    GameObject ship;
    Canvas gui;
    Camera mainCamera;

    //Building
    [Header("Building")]
    public static bool building;
    public float multible;
    public GameObject[] buildableObjects;

    GameObject buildingTile;
    int buildingIndex;
    int tilesNextToObjectBuildingIndex;
    bool autoRotate;
    bool rotate;

    //Destroying
    bool destroy;
    GameObject shipCore;
    GameObject unattachedObjectsParent;

    //DestroyPlacedTiles
    [Header("DestroyPlacedTiles")]
    public GameObject destroyGameObject;

    GameObject instDestroyGameObject;

    //AllObjects
    [HideInInspector] public static List<GameObject> allObjects;

    //Grid
    _Grid grid;

    private void Start()        // Start is called before the first frame update            //NOTE: Maybe change this to Awake()
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

        //Destroying
        shipCore = GameObject.Find("ShipCore_Attach");
        unattachedObjectsParent = GameObject.Find("UnattachedObjects");
        gui = GameObject.Find("GUI").GetComponent<Canvas>();
    }

    private void Update()       // Update is called once per frame
    {
        Movement();

        Clicking();

        Building();

        Destroying();

        Debuging();
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

        //Screen Grab
        if (Input.GetButton("Fire3")) transform.localPosition = new Vector3(transform.localPosition.x - Input.GetAxis("Mouse X") * screenDragSpeed * gui.transform.lossyScale.x * Time.deltaTime, transform.localPosition.y - Input.GetAxis("Mouse Y") * screenDragSpeed * gui.transform.lossyScale.y * Time.deltaTime, transform.localPosition.z);
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
            if (!buildingTile) buildingTile = Instantiate(buildableObjects[buildingIndex], ship.transform);

            //Local variables 
            Collider2D tileBuildingCollider = buildingTile.GetComponent<Collider2D>();
            BoxCollider2D tileBuildingBoxCollider = buildingTile.GetComponent<BoxCollider2D>();
            GameObject[] tilesNextToTileBuilding;

            //Checks if there is a collider and if so then disable it 
            if (tileBuildingCollider) tileBuildingCollider.enabled = false;

            //This is how the tile system works for placeing the tiles in the correct positions
            buildingTile.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            //Local variables
            Vector3 buildingTileLocalPosition = buildingTile.transform.localPosition;
            float xBuildingTileMultibleThingy = Mathf.Round(buildingTileLocalPosition.x / multible) * multible;
            float yBuildingTileMultibleThingy = Mathf.Round(buildingTileLocalPosition.y / multible) * multible;
            float xTileBuildingBoxColliderSize = tileBuildingBoxCollider.size.x;
            float yTileBuildingBoxColliderSize = tileBuildingBoxCollider.size.y;

            if (xTileBuildingBoxColliderSize % 2 == 0 && yTileBuildingBoxColliderSize % 2 == 0) buildingTile.transform.localPosition = new Vector2(xBuildingTileMultibleThingy - .5f, yBuildingTileMultibleThingy - .5f);
            else buildingTile.transform.localPosition = xTileBuildingBoxColliderSize % 2 == 0 ? new Vector2(xBuildingTileMultibleThingy - .5f, yBuildingTileMultibleThingy) : yTileBuildingBoxColliderSize % 2 == 0 ? new Vector2(xBuildingTileMultibleThingy, yBuildingTileMultibleThingy - .5f) : new Vector2(xBuildingTileMultibleThingy, yBuildingTileMultibleThingy);
            if (xTileBuildingBoxColliderSize % 2 == 0 && yTileBuildingBoxColliderSize % 2 == 0) tilesNextToTileBuilding = FindObjectsNextToObjectDebug(buildingTile, "_Attach", 4, xTileBuildingBoxColliderSize - .5f, yTileBuildingBoxColliderSize - .5f).ToArray();
            else tilesNextToTileBuilding = xTileBuildingBoxColliderSize % 2 == 0 ? FindObjectsNextToObjectDebug(buildingTile, "_Attach", 4, xTileBuildingBoxColliderSize - .5f, yTileBuildingBoxColliderSize).ToArray() : yTileBuildingBoxColliderSize % 2 == 0 ? FindObjectsNextToObjectDebug(buildingTile, "_Attach", 4, xTileBuildingBoxColliderSize, yTileBuildingBoxColliderSize - .5f).ToArray() : FindObjectsNextToObjectDebug(buildingTile, "_Attach", 4, xTileBuildingBoxColliderSize, yTileBuildingBoxColliderSize).ToArray();

            buildingTileLocalPosition = buildingTile.transform.localPosition;

            //Disables the tile being built box collider
            if (tileBuildingCollider) tileBuildingCollider.enabled = false;

            //Auto rotation
            if (autoRotate)
            {
                GameObject[] tilesNextToTileBuildingAutoRotate = FindObjectsNextToObject(buildingTile, "_Attach", 4);

                if (Input.GetKeyDown(KeyCode.R) && tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex] && tilesNextToObjectBuildingIndex < 4) tilesNextToObjectBuildingIndex++;
                if (tilesNextToObjectBuildingIndex >= 4) tilesNextToObjectBuildingIndex = 0;
                if (!tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex]) tilesNextToObjectBuildingIndex++;
                if (tilesNextToObjectBuildingIndex >= 4) tilesNextToObjectBuildingIndex = 0;

                GameObject tileNextToTileBuildingFromIndex = tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex];

                if (tileNextToTileBuildingFromIndex != null)
                {
                    Vector3 tileNextToTileBuildingFromIndexPosition = tilesNextToTileBuildingAutoRotate[tilesNextToObjectBuildingIndex].transform.localPosition;

                    if (tileNextToTileBuildingFromIndexPosition.x == buildingTileLocalPosition.x - 1)
                    {
                        buildingTile.transform.localRotation = Quaternion.Euler(0, 0, 90);

                        ParticleSystem buildingTileParticleSystem = buildingTile.GetComponent<ParticleSystem>();
                        if (buildingTileParticleSystem)
                        {
                            var buildingTileParticleSystemMain = buildingTileParticleSystem.main;
                            buildingTileParticleSystemMain.startRotation = 90 * Mathf.Deg2Rad;
                        }
                    }
                    else if (tileNextToTileBuildingFromIndexPosition.x == buildingTileLocalPosition.x + 1)
                    {
                        buildingTile.transform.localRotation = Quaternion.Euler(0, 0, -90);

                        ParticleSystem buildingTileParticleSystem = buildingTile.GetComponent<ParticleSystem>();
                        if (buildingTileParticleSystem)
                        {
                            var buildingTileParticleSystemMain = buildingTileParticleSystem.main;
                            buildingTileParticleSystemMain.startRotation = -90 * Mathf.Deg2Rad;
                        }
                    }
                    else if (tileNextToTileBuildingFromIndexPosition.y == buildingTileLocalPosition.y - 1)
                    {
                        buildingTile.transform.localRotation = Quaternion.Euler(0, 0, 180);

                        ParticleSystem buildingTileParticleSystem = buildingTile.GetComponent<ParticleSystem>();
                        if (buildingTileParticleSystem)
                        {
                            var buildingTileParticleSystemMain = buildingTileParticleSystem.main;
                            buildingTileParticleSystemMain.startRotation = 180 * Mathf.Deg2Rad;
                        }
                    }
                    else if (tileNextToTileBuildingFromIndexPosition.y == buildingTileLocalPosition.y + 1)
                    {
                        buildingTile.transform.localRotation = Quaternion.Euler(0, 0, 0);

                        ParticleSystem buildingTileParticleSystem = buildingTile.GetComponent<ParticleSystem>();
                        if (buildingTileParticleSystem)
                        {
                            var buildingTileParticleSystemMain = buildingTileParticleSystem.main;
                            buildingTileParticleSystemMain.startRotation = 0 * Mathf.Deg2Rad;
                        }
                    }
                }
            }

            //Player rotation
            if (rotate && Input.GetKeyDown(KeyCode.R)) buildingTile.transform.localRotation *= Quaternion.Euler(0, 0, 90);

            //Local variables
            bool nextToTile = false;
            for (int i = 0; i < tilesNextToTileBuilding.Length; i++) nextToTile = tilesNextToTileBuilding[i] && tilesNextToTileBuilding[i].name.Contains("_Attach");
            bool inGrid = buildingTileLocalPosition.x <= Mathf.RoundToInt(grid.gridWorldSize.x / 2) && buildingTileLocalPosition.x >= Mathf.RoundToInt(-grid.gridWorldSize.x/2) && buildingTileLocalPosition.y <= Mathf.RoundToInt(grid.gridWorldSize.y/2) && buildingTileLocalPosition.y >= Mathf.RoundToInt(-grid.gridWorldSize.y/2);

            //Places/Builds the tile
            if (!destroy && Input.GetButton("Fire1") && nextToTile && buildingTileLocalPosition != FindClosestObjectThatContains(buildingTile, "_Tile").transform.localPosition && inGrid && TractorBeemScript.astroyidsCollected > 0)
            {
                if (buildingTileLocalPosition.x == Mathf.RoundToInt(grid.gridWorldSize.x / 2) || buildingTileLocalPosition.x == Mathf.RoundToInt(-grid.gridWorldSize.x / 2) || buildingTileLocalPosition.y == Mathf.RoundToInt(grid.gridWorldSize.y / 2) || buildingTileLocalPosition.y == (-grid.gridWorldSize.y / 2))
                {
                    grid.gridWorldSize = new Vector2(grid.gridWorldSize.x + 2, grid.gridWorldSize.y + 2);
                    grid.CreateGrid();
                }

                TractorBeemScript.astroyidsCollected--;
                GUIScript.UpdateResorces();
                tileBuildingCollider.enabled = true;
                allObjects.Add(buildingTile);
                buildingTile = null;

                grid.UpdateGrid();
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

            if (!instDestroyGameObject) instDestroyGameObject = Instantiate(destroyGameObject, ship.transform);

            instDestroyGameObject.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            instDestroyGameObject.transform.localPosition = new Vector2(Mathf.Round(instDestroyGameObject.transform.localPosition.x / multible) * multible, Mathf.Round(instDestroyGameObject.transform.localPosition.y / multible) * multible);

            GameObject closestTileObjectToCheck = FindClosestObjectThatContains(instDestroyGameObject, "_Tile");

            //This got an error: NullReferenceException: Object reference not set to an instance of an object PlayerController.Destroying()(at Assets / Scripts / PlayerController.cs:235) PlayerController.Update()(at Assets / Scripts / PlayerController.cs:68)
            //The error is caused by the rotation of the ship so for now it is restricted in the rigidbody of the ship

            //Make smaller !!!
            bool overDestroyableTile = instDestroyGameObject.transform.localPosition == closestTileObjectToCheck.transform.localPosition || instDestroyGameObject.transform.localPosition.x - .5f == closestTileObjectToCheck.transform.localPosition.x || instDestroyGameObject.transform.localPosition.x + .5f == closestTileObjectToCheck.transform.localPosition.x || instDestroyGameObject.transform.localPosition.y - .5f == closestTileObjectToCheck.transform.localPosition.y || instDestroyGameObject.transform.localPosition.y + .5f == closestTileObjectToCheck.transform.localPosition.y;

            if (Input.GetButtonDown("Fire1") && instDestroyGameObject && overDestroyableTile)
            {
                allObjects.Remove(closestTileObjectToCheck);
                Destroy(closestTileObjectToCheck);
                TractorBeemScript.astroyidsCollected++;
                GUIScript.UpdateResorces();

                grid.UpdateGrid();

                GameObject disconnectedObjectParent = new GameObject("UnattachedObjectParent");
                Rigidbody2D disconnectedObjectParentRB = disconnectedObjectParent.AddComponent<Rigidbody2D>();
                disconnectedObjectParentRB.gravityScale = 0;
                disconnectedObjectParentRB.useAutoMass = true;
                disconnectedObjectParentRB.velocity = ship.GetComponent<Rigidbody2D>().velocity;
                disconnectedObjectParent.transform.parent = unattachedObjectsParent.transform;

                for (int i = 0; i < allObjects.Count; i++)
                {
                    if (allObjects[i] && allObjects[i].name.Contains("_Tile"))
                    {
                        if (!IsAttached(allObjects[i].transform.localPosition, shipCore.transform.localPosition))
                        {
                            allObjects[i].transform.parent = disconnectedObjectParent.transform;
                            allObjects[i].layer = 0;
                            allObjects[i].name = "UslessUnattachedObject";
                        }
                    }
                }

                grid.UpdateGrid();
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

            if (hit2D) print("Player Clicked " + hit2D.collider.name);
        }

        //Grid
        if (Input.GetKeyDown(KeyCode.Home)) grid.displayGridGizmos = grid.displayGridGizmos ? false : true;

        //Resorces
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            TractorBeemScript.astroyidsCollected++;
            GUIScript.UpdateResorces();
        }
    }

    //Below this is the secondary functions

    public void SelectedObjectFromGUI(int objectSelectedIndex)      //NOTE: Put this in GUIScript
    {
        if (buildingTile) Destroy(buildingTile);
        buildingIndex = objectSelectedIndex;

        if (buildingIndex == 1) destroy = false; //Slope
        if (buildingIndex == 2) autoRotate = true; //Tractorbeem
        if (buildingIndex == 3) autoRotate = true; //Connector 

        building = true;
    }

    public void DestroySelected()      //NOTE: Put this in GUIScript
    {
        destroy = true;
    }       //Goes with the SelectedObjectFromGUI method

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
            if (allObjects[i] && allObjects[i].name.Contains(contains)) objectsThatContains.Add(allObjects[i]);
            else if (!allObjects[i]) allObjects.RemoveAt(i);
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
    }       //Maybe rename or something also run tests

    private List<GameObject> FindObjectsNextToObjectDebug(GameObject fromObject, string contains, int ammountOfObjects, float xBoxColliderSize, float yBoxColliderSize)
    {
        List<GameObject> objectsToReturn = new List<GameObject>();
        List<GameObject> objectsThatContains = new List<GameObject>();

        for (int i = 0; i < allObjects.Count; i++)
        {
            if (allObjects[i] && allObjects[i].name.Contains(contains)) objectsThatContains.Add(allObjects[i]);
            else if (!allObjects[i]) allObjects.RemoveAt(i);
        }

        for (int i = 0; i < objectsThatContains.Count; i++)
        {
            float distanceBetweenObjects = (fromObject.transform.localPosition - objectsThatContains[i].transform.localPosition).magnitude;

            if (distanceBetweenObjects == xBoxColliderSize || distanceBetweenObjects == yBoxColliderSize) objectsToReturn.Add(objectsThatContains[i]);
        }

        return objectsToReturn;
    }   //!!!!!

    private GameObject FindObjectInDistanceThatContains(GameObject fromObject, string contains, float distance)
    {
        GameObject objectThatContains = null;

        for (int i = 0; i < allObjects.Count; i++)
        {
            if (allObjects[i] && allObjects[i].name.Contains(contains) && (fromObject.transform.position - allObjects[i].transform.position).magnitude <= distance) objectThatContains = allObjects[i];
            if (!allObjects[i]) allObjects.RemoveAt(i);
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
    }               //This is detecting corner tiles maybe mess with the size of the nodes or something

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

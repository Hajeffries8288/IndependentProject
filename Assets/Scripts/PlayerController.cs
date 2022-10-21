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
    float mouseScroll;
    GameObject ship;
    Camera mainCamera;

    //Building                                  NOTE: Try to find a way to have less bools
    [Header("Building")]
    public static bool building;
    public float multible;
    public GameObject[] buildableObjects;
    GameObject buildingTile;
    int buildingIndex;
    int tilesNextToObjectBuildingIndex;
    bool autoRotate;
    bool rotate;
    bool destroy;

    //Clicking


    //Misc
    [HideInInspector] public static List<GameObject> allObjects;

    //Debuging
    

    // Start is called before the first frame update
    void Start()
    {
        //Movement
        mouseScroll = 5;
        ship = GameObject.Find("Ship");

        //Building
        

        //Clicking


        //Debuging
        mainCamera = Camera.main;

        allObjects = new List<GameObject>(FindObjectsOfType<GameObject>());
    }

    // Update is called once per frame
    void Update()
    {
        Movement();

        Clicking();

        Building();

        Debuging();       
    }
    
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
            else transform.parent = null;
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
            //Creates the tile that the player is trying to build
            if (!buildingTile) buildingTile = Instantiate(buildableObjects[buildingIndex], ship.transform);

            //Local variables 
            GameObject[] tilesNextToTileBuilding = FindObjectsNextToObject(buildingTile, "_Attach", 4);
            Collider2D tileBuildingCollider = buildingTile.GetComponent<Collider2D>();
            BoxCollider2D tileBuildingBoxCollider = buildingTile.GetComponent<BoxCollider2D>();
            bool canPlace = false;

            //Checks if there is a collider and if so then disable it 
            if (tileBuildingCollider) tileBuildingCollider.enabled = false;

            //This is how the tile system works for placeing the tiles in the correct positions
            buildingTile.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (tileBuildingBoxCollider && tileBuildingBoxCollider.size.y % 2 == 0) buildingTile.transform.localPosition = new Vector3(Mathf.Round(buildingTile.transform.localPosition.x / multible) * multible, Mathf.Round(buildingTile.transform.localPosition.y / multible) * multible - 0.5f, 0);
            else buildingTile.transform.localPosition = new Vector3(Mathf.Round(buildingTile.transform.localPosition.x / multible) * multible, Mathf.Round(buildingTile.transform.localPosition.y / multible) * multible, 0);

            //if (Input.GetKeyDown(KeyCode.Home) && tileBuildingBoxCollider) print(tileBuildingBoxCollider.size);

            //Checks if this object should be auto rotated to a tile before placeing 
            if (autoRotate)
            {
                if (Input.GetKeyDown(KeyCode.R) && tilesNextToTileBuilding[tilesNextToObjectBuildingIndex] && tilesNextToObjectBuildingIndex < 4) tilesNextToObjectBuildingIndex++;
                if (tilesNextToObjectBuildingIndex >= 4) tilesNextToObjectBuildingIndex = 0;
                if (!tilesNextToTileBuilding[tilesNextToObjectBuildingIndex]) tilesNextToObjectBuildingIndex++;
                if (tilesNextToObjectBuildingIndex >= 4) tilesNextToObjectBuildingIndex = 0;

                if (tilesNextToTileBuilding[tilesNextToObjectBuildingIndex] != null)
                {
                    if (tilesNextToTileBuilding[tilesNextToObjectBuildingIndex].transform.localPosition.x == buildingTile.transform.localPosition.x - 1) buildingTile.transform.localRotation = Quaternion.Euler(0, 0, 90);
                    else if (tilesNextToTileBuilding[tilesNextToObjectBuildingIndex].transform.localPosition.x == buildingTile.transform.localPosition.x + 1) buildingTile.transform.localRotation = Quaternion.Euler(0, 0, -90);
                    else if (tilesNextToTileBuilding[tilesNextToObjectBuildingIndex].transform.localPosition.y == buildingTile.transform.localPosition.y - 1) buildingTile.transform.localRotation = Quaternion.Euler(0, 0, 180);
                    else if (tilesNextToTileBuilding[tilesNextToObjectBuildingIndex].transform.localPosition.y == buildingTile.transform.localPosition.y + 1) buildingTile.transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
            }

            //Checks if the object can be rotated by the player
            if (rotate && Input.GetKeyDown(KeyCode.R)) buildingTile.transform.localRotation *= Quaternion.Euler(0, 0, 90);

            //This is checking if there is a tile next to the tile the player is trying to build
            for (int i = 0; i < tilesNextToTileBuilding.Length; i++) if (tilesNextToTileBuilding[i] && tilesNextToTileBuilding[i].name.Contains("_Attach")) canPlace = true;

            GameObject closestTileObjectToInstBuildableObject = FindClosestObjectThatContains(buildingTile, "_Tile");       //Figgure out why this variable has to be here

            //Places/Builds the tile
            if (!destroy && Input.GetButton("Fire1") && canPlace && buildingTile.transform.localPosition != closestTileObjectToInstBuildableObject.transform.localPosition)
            {
                if (tileBuildingCollider) tileBuildingCollider.enabled = true;
                allObjects.Add(buildingTile);
                buildingTile = null;
            }
            else if (destroy && Input.GetButtonDown("Fire1") && buildingTile.transform.localPosition == closestTileObjectToInstBuildableObject.transform.localPosition) //Destroyes the tile under the "buildingTile" and disconnects all tiles that are no longer connected to the ship's core                      NOTE: This should be done somewhere else 
            {
                GameObject unattachedObjectsParent = new GameObject("UnattachedObjectsParent");
                Rigidbody2D unattachedObjectsParentRb = unattachedObjectsParent.AddComponent<Rigidbody2D>();

                unattachedObjectsParentRb.gravityScale = 0;
                unattachedObjectsParentRb.velocity = ship.GetComponent<Rigidbody2D>().velocity;
                unattachedObjectsParentRb.useAutoMass = true;

                allObjects.Remove(closestTileObjectToInstBuildableObject);
                Destroy(closestTileObjectToInstBuildableObject);

                for (int i = 0; i < allObjects.Count; i++)
                {
                    if (allObjects[i].name.Contains("_Tile") && !IsAttached(GameObject.Find("ShipCore_Attach").GetComponent<PathfindingNode>(), allObjects[i].GetComponent<PathfindingNode>()))
                    {
                        allObjects[i].name = "UnattachedUslessObject";
                        allObjects[i].transform.parent = unattachedObjectsParent.transform;
                    }
                }
            }

            //Stops building script
            if (Input.GetButtonUp("Fire2"))
            {
                Destroy(buildingTile.gameObject);
                buildingTile = null;
                autoRotate = false;
                rotate = false;
                building = false;
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

        //Mouse movement
        if (Input.GetButton("Fire3"))
        {
            //Make mouse code after disnyland or make the way it would work aka come up with ideas for how it will work at disnyland.
        }
    }

    public void SelectedObjectFromGUI(int objectSelectedIndex)
    {
        if (buildingTile) Destroy(buildingTile);
        buildingIndex = objectSelectedIndex;

        if (buildingIndex == 0) //Square
        {
            destroy = false;
            rotate = false;
            autoRotate = false;
        }
        if (buildingIndex == 1) //Slope
        {
            destroy = false;
            rotate = true;
            autoRotate = false;
        }
        if (buildingIndex == 2) //Tractorbeem
        {
            destroy = false;
            rotate = false;
            autoRotate = true;
        }
        if (buildingIndex == 3) //Destroying placed objects
        {
            destroy = true;
            rotate = false;
            autoRotate = false;
        }
        if (buildingIndex == 4) //Connector
        {
            destroy = false;
            rotate = false;
            autoRotate = true;
        }
        if (buildingIndex == 5) //Door
        {
            destroy = false;
            rotate = false;
            autoRotate = false;
        }

        building = true;
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
            if (allObjects[i] && allObjects[i].name.Contains(contains)) objectsThatContains.Add(allObjects[i]);
            else if (!allObjects[i]) allObjects.RemoveAt(i);
        }

        for (int i = 0; i < objectsThatContains.Count; i++)
        {
            if ((fromObject.transform.localPosition - objectsThatContains[i].transform.localPosition).magnitude == 1) //For testing
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

    private bool IsAttached(PathfindingNode startNode, PathfindingNode endNode)
    {
        bool attached = false;

        List<PathfindingNode> openPathNodes = new List<PathfindingNode>();      //Nodes to be evaluated
        HashSet<PathfindingNode> closedPathNodes = new HashSet<PathfindingNode>();    //Nodes that have been evaluated
        openPathNodes.Add(startNode);

        for (int i = 0; i < allObjects.Count; i++)
        {
            PathfindingNode pathfindingNode;

            if (allObjects[i] && allObjects[i].name.Contains("_Tile"))
            {
                pathfindingNode = allObjects[i].GetComponent<PathfindingNode>();
                pathfindingNode.gCost = (int)((Vector2)allObjects[i].transform.localPosition - (Vector2)startNode.transform.localPosition).magnitude;
                pathfindingNode.hCost = (int)((Vector2)allObjects[i].transform.localPosition - (Vector2)endNode.transform.localPosition).magnitude;
                pathfindingNode.fCost = pathfindingNode.gCost + pathfindingNode.hCost;
            }
            else if (!allObjects[i]) allObjects.RemoveAt(i);
        }

        while (openPathNodes.Count > 0)
        {
            PathfindingNode currentNode = openPathNodes[0];

            for (int i = 0; i < openPathNodes.Count; i++) if (currentNode.fCost < openPathNodes[i].fCost || currentNode.fCost == openPathNodes[i].fCost && currentNode.hCost < openPathNodes[i].hCost) currentNode = openPathNodes[i];

            openPathNodes.Remove(currentNode);
            closedPathNodes.Add(currentNode);

            if (currentNode == endNode)
            {
                attached = true;
                break;
            }

            GameObject[] objectsNextToNode = FindObjectsNextToObject(currentNode.gameObject, "_Tile", 4);
            foreach (GameObject objectNextToCurrentNode in objectsNextToNode)
            {
                if (objectNextToCurrentNode)
                {
                    if (closedPathNodes.Contains(objectNextToCurrentNode.GetComponent<PathfindingNode>())) continue;

                    PathfindingNode nextToCurrentNode = objectNextToCurrentNode.GetComponent<PathfindingNode>();

                    int newMovementCost = currentNode.gCost + (int)((Vector2)currentNode.transform.localPosition - (Vector2)nextToCurrentNode.transform.localPosition).magnitude;
                    if (newMovementCost < nextToCurrentNode.fCost || !openPathNodes.Contains(nextToCurrentNode))
                    {
                        nextToCurrentNode.gCost = newMovementCost;
                        nextToCurrentNode.hCost = (int)((Vector2)nextToCurrentNode.transform.localPosition - (Vector2)endNode.transform.localPosition).magnitude;
                        nextToCurrentNode.parentNode = currentNode;

                        if (!openPathNodes.Contains(nextToCurrentNode)) openPathNodes.Add(nextToCurrentNode);
                    }
                }
            }
        }
        return attached;
    }
}

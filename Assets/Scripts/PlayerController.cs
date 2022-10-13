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
    GameObject ship;
    float mouseScroll;
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
    bool destroy;

    //Clicking
    

    //Debuging
    [Header("Debuging")]
    [HideInInspector] public static List<GameObject> allObjects;

    bool loopShutDown = false;

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

    private void FixedUpdate()
    {
        DebugingPhysics();
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
            if (!buildingTile) buildingTile = Instantiate(buildableObjects[buildingIndex], ship.transform);
            Collider2D tileBuildingCollider = buildingTile.GetComponent<Collider2D>();

            if (tileBuildingCollider) tileBuildingCollider.enabled = false;
            buildingTile.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            buildingTile.transform.localPosition = new Vector3(Mathf.Round(buildingTile.transform.localPosition.x / multible) * multible, Mathf.Round(buildingTile.transform.localPosition.y / multible) * multible, 0);

            if (autoRotate)
            {
                GameObject[] tilesNextToTileBuilding = FindObjectsNextToObject(buildingTile, "_Attach", 4);

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

            if (rotate && Input.GetKeyDown(KeyCode.R)) buildingTile.transform.localRotation *= Quaternion.Euler(0, 0, 90);

            GameObject closestTileObjectToInstBuildableObjectAttach = FindClosestObjectThatContains(buildingTile, "_Attach");
            GameObject closestTileObjectToInstBuildableObject = FindClosestObjectThatContains(buildingTile, "_Tile");

            if (!destroy && Input.GetButton("Fire1") && closestTileObjectToInstBuildableObjectAttach && closestTileObjectToInstBuildableObject && buildingTile.transform.localPosition != closestTileObjectToInstBuildableObject.transform.localPosition && (buildingTile.transform.localPosition - closestTileObjectToInstBuildableObjectAttach.transform.localPosition).magnitude == multible)
            {
                if (tileBuildingCollider) tileBuildingCollider.enabled = true;
                allObjects.Add(buildingTile);
                buildingTile = null;
            }
            else if (destroy && Input.GetButtonDown("Fire1") && closestTileObjectToInstBuildableObject && buildingTile.transform.localPosition == closestTileObjectToInstBuildableObject.transform.localPosition)
            {
                allObjects.Remove(closestTileObjectToInstBuildableObject);
                Destroy(closestTileObjectToInstBuildableObject);
            }

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
    }               //NOTE: This may cause poor memory find fix if needed

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
        
        if (destroy && Input.GetButtonDown("Fire1")) for (int i = 0; i < allObjects.Count; i++) if (allObjects[i].name.Contains("_Attach")) print(DebuggingIsAttached(GameObject.Find("ShipCore_Attach").GetComponent<PathfindingNode>(), allObjects[i].GetComponent<PathfindingNode>()));
    }

    private void DebugingPhysics()
    {
        
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

    private bool DebuggingIsAttached(PathfindingNode startNode, PathfindingNode endNode)
    {
        bool attached = false;

        List<PathfindingNode> openPathNodes = new List<PathfindingNode>();      //Nodes to be evaluated
        HashSet<PathfindingNode> closedPathNodes = new HashSet<PathfindingNode>();    //Nodes that have been evaluated
        openPathNodes.Add(startNode);

        for (int i = 0; i < allObjects.Count; i++)
        {
            PathfindingNode pathfindingNode;
            if (allObjects[i] && allObjects[i].name.Contains("_Attach"))
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

            GameObject[] objectsNextToNode = FindObjectsNextToObject(currentNode.gameObject, "_Attach", 4);
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

    private bool InListGameObject(GameObject checkFor, List<GameObject> list)
    {
        bool boolToReturn = false;

        for (int i = 0; i < list.Count; i++) if (checkFor == list[i]) boolToReturn = true;

        return boolToReturn;
    }       //Rename this!
}

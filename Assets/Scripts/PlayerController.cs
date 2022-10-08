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

    bool infiniteLoopShutOff;

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

        //for (int i = 0; i < allObjects.Count; i++)
        //{
            //if (allObjects[i].name.Contains("_Attach"))
            //{
                //DebuggingCanReach("ShipCore", "_Attach");
            //}
        //}

        DebuggingCanReach("ShipCore", "_Attach");
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

    private void DebuggingCanReach(string toObject, string walkOn)      //Stop the infinite loop and all that jazz
    {
        List<GameObject> objectsBeenAt = new List<GameObject>();
        GameObject currentObject = GameObject.Find("TestingBlock_Tile_Attach (6)");
        GameObject[] nextTo = FindObjectsNextToObject(currentObject, walkOn, 4);
        GameObject nextObject = null;
        bool newObject = true;
        int failSafe = 0;

        for (int i = 0; i < nextTo.Length; i++)
        {
            newObject = true;

            for (int j = 0; j < objectsBeenAt.Count; j++)
            {
                if (nextTo[i] != objectsBeenAt[j]) newObject = true;
                else newObject = false;
            }

            if (nextTo[i] && nextTo[i].name.Contains(walkOn) && newObject)
            {
                nextObject = nextTo[i];
                objectsBeenAt.Add(currentObject);
                currentObject = nextObject;
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Home))
        {
            print("_______________________");
            for (int i = 0; i < objectsBeenAt.Count; i++) print("ObjectsBeenAt[" + i + "] = " + objectsBeenAt[i]);
            print("CurrentObject = " + currentObject);
        }

        if (nextObject && !infiniteLoopShutOff)
        {
            while (currentObject != null)
            {
                failSafe++;
                nextTo = FindObjectsNextToObject(currentObject, walkOn, 4);

                for (int i = 0; i < nextTo.Length; i++)
                {
                    if (nextTo[i])
                    {
                        if (nextTo[i].name.Contains(walkOn) && InListGameObject(nextTo[i], objectsBeenAt))
                        {
                            nextObject = nextTo[i];
                            objectsBeenAt.Add(currentObject);
                            currentObject = nextObject;
                            break;
                        }
                    }
                }

                if (failSafe == 500)
                {
                    Debug.LogWarning("Infinite loop detected!");
                    infiniteLoopShutOff = true;
                    break;
                }
            }

            if (Input.GetKeyDown(KeyCode.Home))
            {
                print("_______________________");
                for (int i = 0; i < objectsBeenAt.Count; i++) print("ObjectsBeenAt[" + i + "] = " + objectsBeenAt[i]);
                print("CurrentObject = " + currentObject);
            }

            for (int i = 0; i < objectsBeenAt.Count; i++)
            {
                if (i + 1 < objectsBeenAt.Count && objectsBeenAt[i] && objectsBeenAt[i + 1]) Debug.DrawLine(objectsBeenAt[i].transform.position, objectsBeenAt[i + 1].transform.position, Color.red);
            }
        }









        //bool atObj = false;
        //bool stopWhile = false;
        //int failSafe = 0;

        //while (!stopWhile)
        //{
        //GameObject[] nextTo = FindObjectsNextToObject(fromObject, walkOn, 4);
        //GameObject lastObj = null;

        //bool contains;
        //bool contains1;
        //bool FINDNAME;

        //for (int i = 0; i < nextTo.Length; i++)
        //{
        //if (nextTo[i])
        //{
        //contains = nextTo[i].name.Contains(walkOn);
        //contains1 = nextTo[i].name.Contains(toObject);
        //INDNAME = nextTo[i] != lastObj;

        //if (contains && FINDNAME)
        //{
        //if (lastObj) lastObj = fromObject;
        //else lastObj = fromObject;

        //fromObject = nextTo[i];
        //}
        //if (contains1)
        //{
        //atObj = true;
        //stopWhile = true;

        //fromObject = nextTo[i];
        //}
        //if (!contains || !contains1) stopWhile = true;
        //}
        //}

        //failSafe++;
        //if (failSafe == 500)
        //{
        //stopWhile = true;
        //Debug.LogWarning("Infinite loop detected");
        //}
        //if (fromObject && lastObj && !atObj) Debug.DrawLine(fromObject.transform.position, lastObj.transform.position, Color.red);
        //if (atObj) Debug.DrawLine(fromObject.transform.position, lastObj.transform.position, Color.green);
        //}

        //return atObj;
    }

    private bool InListGameObject(GameObject checkFor, List<GameObject> list)
    {
        bool boolToReturn = false;

        for (int i = 0; i < list.Count; i++) if (checkFor == list[i]) boolToReturn = true;

        return boolToReturn;
    }       //Rename this!
}

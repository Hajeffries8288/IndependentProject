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
    Camera mainCamera;

    //Building
    public float multible;
    public GameObject[] buildableObjects;
    GameObject[] instBuildableObjects;
    int buildingIndex;
    bool building;

    //Clicking
    

    //Debuging
    [Header("Debuging")]
    public float objectGrabbedSpeed;
    GameObject objectGrabbed;
    GameObject ship;

    List<GameObject> allObjects;

    public float resorces;

    // Start is called before the first frame update
    void Start()
    {
        //Movement
        
        //Building
        instBuildableObjects = new GameObject[buildableObjects.Length];

        //Clicking


        //Debuging
        mainCamera = Camera.main;
        allObjects = new List<GameObject>(FindObjectsOfType<GameObject>());
        ship = GameObject.Find("Ship");
        mouseScroll = 5;
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
            if (!instBuildableObjects[buildingIndex]) instBuildableObjects[buildingIndex] = Instantiate(buildableObjects[buildingIndex], transform);
            Collider2D instBuildableObjectCollider = instBuildableObjects[buildingIndex].GetComponent<Collider2D>();

            instBuildableObjects[buildingIndex].transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            instBuildableObjects[buildingIndex].transform.localPosition = new Vector3(Mathf.Round(instBuildableObjects[buildingIndex].transform.localPosition.x / multible) * multible, Mathf.Round(instBuildableObjects[buildingIndex].transform.localPosition.y / multible) * multible, 0);
            instBuildableObjectCollider.enabled = false;
            if (Input.GetButtonDown("Fire1"))
            {
                instBuildableObjectCollider.enabled = true;
                instBuildableObjects[buildingIndex] = null;
            }
            if (Input.GetButtonDown("Fire2"))
            {
                Destroy(instBuildableObjects[buildingIndex].gameObject);
                instBuildableObjects[buildingIndex] = null;
                building = false;
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
    }

    private void DebugingPhysics()
    {
        //GrabingStuff
        if (Input.GetButton("Fire1"))
        {
            Ray mousePosition = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit2D = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));

            if (hit2D.collider && !objectGrabbed) objectGrabbed = hit2D.transform.gameObject;

            if (objectGrabbed)
            {
                if (objectGrabbed.name.Contains("_Physics"))
                {
                    Rigidbody2D objectGrabbedRigidbody = objectGrabbed.GetComponent<Rigidbody2D>();

                    objectGrabbedRigidbody.MovePosition(new Vector3(Mathf.Lerp(objectGrabbed.transform.position.x, mousePosition.origin.x, objectGrabbedSpeed * Time.fixedDeltaTime), Mathf.Lerp(objectGrabbed.transform.position.y, mousePosition.origin.y, objectGrabbedSpeed * Time.fixedDeltaTime), 0));
                    objectGrabbedRigidbody.velocity = Vector2.zero;
                }
            }
        }
    }

    //GUI BUTTONS: START
    public void TestingObject()
    {
        buildingIndex = 0;
        building = true;
    }

    public void Slope()
    {
        print("Yet to be implamented");
    }
    //GUI BUTTONS: END

    private GameObject FindClosestObjectThatContains(string contains)
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
                    distances.Add((transform.position - allObjects[i].transform.position).magnitude);
                    objectThatContains.Add(allObjects[i]);
                }
            }
        }
        closestF = Mathf.Min(distances.ToArray());
        for (int i = 0; i < objectThatContains.Count; i++)
        {
            if (objectThatContains[i] != null)
            {
                float distance = (transform.position - objectThatContains[i].transform.position).magnitude;
                if (distance == closestF) closest = objectThatContains[i];
            }
        }

        return closest;
    }

    private GameObject FindObjectInDistanceThatContains(GameObject fromObject, string contains, float distance)
    {
        GameObject objectThatContains = null;

        for (int i = 0; i < allObjects.Count; i++) if (allObjects[i] && allObjects[i].name.Contains(contains) && (fromObject.transform.position - allObjects[i].transform.position).magnitude <= distance) objectThatContains = allObjects[i];

        return objectThatContains;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class CustomAI : MonoBehaviour
{
    public Transform target;
    public float speed = 1;
    public float nextWaypointDistance = 0;
    public float timeUpdatePath = .5f;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Seeker seeker;
    Rigidbody2D rb;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        Vector3 camPos = Camera.main.transform.position;
        Camera.main.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        target = Camera.main.transform;
        Camera.main.transform.position = camPos;

        InvokeRepeating("UpdatePath", 0, timeUpdatePath);
    }


    void UpdatePath()
    {
        Vector3 camPos = Camera.main.transform.position;
        Camera.main.transform.position = Camera.main.ScreenPointToRay(Input.mousePosition).GetPoint(0);
        target = Camera.main.transform;
        Camera.main.transform.position = camPos;

        if (seeker.IsDone()) seeker.StartPath(rb.position, target.position, OnPathComplete);
    }

    private void FixedUpdate()
    {
        if (path == null) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else reachedEndOfPath = false;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance > nextWaypointDistance) currentWaypoint++;
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeemScript : MonoBehaviour
{
    public float velocityOfPickingUpAstroyid;       //Change this name when you have the chance

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        DebugingPhysics();
    }

    private void DebugingPhysics()
    {
        if (!PlayerController.building)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, Mathf.Infinity, 1 << 0, -Mathf.Infinity, Mathf.Infinity);

            if (hit && hit.transform.name.Contains("Astroyids"))
            {
                Rigidbody2D rb = hit.transform.GetComponent<Rigidbody2D>();

                rb.transform.parent = transform;
                rb.velocity = ((transform.position - hit.transform.position) * velocityOfPickingUpAstroyid / hit.transform.localScale.x);
                if ((transform.position - hit.transform.position).magnitude <= 1) Destroy(hit.transform.gameObject); //For testing

                Debug.DrawLine(transform.position, hit.point, Color.red);
            }
        }
    }
}

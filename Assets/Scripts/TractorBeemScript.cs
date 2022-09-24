using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeemScript : MonoBehaviour
{
    public float velocityOfPickingUpAstroyid;       //Change this name when you have the chance
    GameObject ship;

    // Start is called before the first frame update
    void Start()
    {
        ship = GameObject.Find("Ship");
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
        //Figure out how to get this to be the correct rotation because transform.localRotation does not equal the actual rotation;
        var testoWesto = GetComponent<ParticleSystem>().main;
        //testoWesto.startRotationMultiplier = transform.localRotation.z;
        testoWesto.startRotationZ = transform.localRotation.z * Mathf.Rad2Deg * Mathf.Deg2Rad;

        if (!PlayerController.building)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, Mathf.Infinity, 1 << 0, -Mathf.Infinity, Mathf.Infinity);

            if (hit && hit.transform.name.Contains("Astroyids"))
            {
                Rigidbody2D rb = hit.transform.GetComponent<Rigidbody2D>();

                rb.velocity = ((transform.position - hit.transform.position) * velocityOfPickingUpAstroyid / hit.transform.localScale.x);
                rb.angularVelocity = 0;
                GetComponent<Animator>().SetBool("Off/On", true);
                GetComponent<ParticleSystem>().Play();
                if ((transform.position - hit.transform.position).magnitude <= 1) Destroy(hit.transform.gameObject); //For testing

                Debug.DrawLine(transform.position, hit.point, Color.red);
            }
            else
            {
                GetComponent<Animator>().SetBool("Off/On", false);
                GetComponent<ParticleSystem>().Pause();
                GetComponent<ParticleSystem>().Clear();
            }
        }
        else
        {
            GetComponent<ParticleSystem>().Pause();
            GetComponent<ParticleSystem>().Clear();
        }
    }
}

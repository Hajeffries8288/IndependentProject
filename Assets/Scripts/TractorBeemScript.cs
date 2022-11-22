using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeemScript : MonoBehaviour
{
    //TractorBeem
    public float velocityOfPickingUpAstroyid;
    public static int astroyidsCollected; // This is temperary a better and more in depth resorce stuff will be made later

    ObjectSpawner objectSpawner;
    BoxCollider2D boxCollider;

    //Raycast
    public float distanceToPickUpAstroyid;
    RaycastHit2D hit;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        objectSpawner = GameObject.Find("Ship").GetComponent<ObjectSpawner>();
    }

    private void FixedUpdate()
    {
        if (name != "UslessUnattachedObject")
        {
            DebugingPhysics();

            TractorBeem();
        }
    }

    private void DebugingPhysics()
    {
        //Draw a debug line the color of red when the tractor beem hits a pickupable object
        if (GetComponent<BoxCollider2D>().enabled && hit && hit.transform.name.Contains("Astroyids")) Debug.DrawLine(transform.position, hit.point, Color.red);
    }

    private void TractorBeem()
    {
        GameObject closestAstroyid = FindClosestObjectThatContains(gameObject, "Astroyids");
        Animator animator = GetComponent<Animator>();
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();

        if (boxCollider.enabled)
        {
            if (closestAstroyid && (transform.position - closestAstroyid.transform.position).magnitude <= distanceToPickUpAstroyid) hit = Physics2D.Raycast(transform.position, -transform.up, distanceToPickUpAstroyid, 1 << 0, -Mathf.Infinity, Mathf.Infinity);

            if (hit && hit.transform.name.Contains("Astroyids"))
            {
                Rigidbody2D rb = hit.transform.GetComponent<Rigidbody2D>();

                if (rb.velocity.x < objectSpawner.astroyidMaximumVelocity && rb.velocity.y < objectSpawner.astroyidMaximumVelocity) rb.velocity = (transform.position - hit.transform.position) * velocityOfPickingUpAstroyid / hit.transform.localScale.x;
                rb.angularVelocity = 0;
                animator.SetBool("Off/On", true);
                particleSystem.Play();

                if ((transform.position - hit.transform.position).magnitude <= 2)   //This is for testing 
                {
                    Destroy(hit.transform.gameObject);
                    astroyidsCollected++;
                    PlayerController.guiScript.UpdateResorces();
                    PlayerController.allObjects.Remove(hit.transform.gameObject);
                }
            }
            else
            {
                animator.SetBool("Off/On", false);
                particleSystem.Pause();
                particleSystem.Clear();
            }
        }
        else
        {
            particleSystem.Pause();
            particleSystem.Clear();
        }
    }

    private GameObject FindClosestObjectThatContains(GameObject fromObject, string contains)
    {
        GameObject closest = null;
        List<GameObject> objectThatContains = new List<GameObject>(0);
        List<float> distances = new List<float>(0);
        float closestF;

        for (int i = 0; i < PlayerController.allObjects.Count; i++)
        {
            if (PlayerController.allObjects[i] != null)
            {
                if (PlayerController.allObjects[i].name.Contains(contains))
                {
                    distances.Add((fromObject.transform.position - PlayerController.allObjects[i].transform.position).magnitude);
                    objectThatContains.Add(PlayerController.allObjects[i]);
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
}

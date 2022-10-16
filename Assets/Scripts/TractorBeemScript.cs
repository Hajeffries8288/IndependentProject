using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeemScript : MonoBehaviour
{
    //TractorBeem
    public float velocityOfPickingUpAstroyid;

    //Raycast
    public float distanceToPickUpAstroyid;
    RaycastHit2D hit;

    private void FixedUpdate()
    {
        DebugingPhysics();

        TractorBeem();
    }

    private void DebugingPhysics()
    {
        //Draw a debug line the color of red when the tractor beem hits a pickupable object
        if (GetComponent<BoxCollider2D>().enabled && hit && hit.transform.name.Contains("Astroyids")) Debug.DrawLine(transform.position, hit.point, Color.red);
    }

    private void TractorBeem()
    {
        //You have to have this variable because I guess its a bich
        var particleSystemMain = GetComponent<ParticleSystem>().main;
        particleSystemMain.startRotationZ = transform.localRotation.z * Mathf.Deg2Rad; //This isnt the correct rotation figgure this out NOTE: This is for the rotation of the particles so its not the most importent thing

        GameObject closestAstroyid = FindClosestObjectThatContains(gameObject, "Astroyids");
        Animator animator = GetComponent<Animator>();
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();

        if (GetComponent<BoxCollider2D>().enabled)
        {
            if (closestAstroyid && (transform.position - closestAstroyid.transform.position).magnitude <= distanceToPickUpAstroyid) hit = Physics2D.Raycast(transform.position, -transform.up, distanceToPickUpAstroyid, 1 << 0, -Mathf.Infinity, Mathf.Infinity);

            if (hit && hit.transform.name.Contains("Astroyids"))
            {
                Rigidbody2D rb = hit.transform.GetComponent<Rigidbody2D>();

                rb.velocity = (transform.position - hit.transform.position) * velocityOfPickingUpAstroyid / hit.transform.localScale.x;
                rb.angularVelocity = 0;
                animator.SetBool("Off/On", true);
                particleSystem.Play();

                if ((transform.position - hit.transform.position).magnitude <= 1)
                {
                    Destroy(hit.transform.gameObject);
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

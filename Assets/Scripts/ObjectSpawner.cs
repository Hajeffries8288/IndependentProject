using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    //Astroyids
    [Header("Astroyids")]
    [HideInInspector] public static List<GameObject> astroyidsCreated;
    public float chanceForAstroyidSpawn;
    public float distanceToSpawnAstroyids;
    public float distanceFromShipForDespawn;
    public float astroyidMaximumSize;
    public float astroyidMaximumVelocity;
    public GameObject astroid;
    public Sprite[] astroidSprites;

    float timeInbetweenCheckingAstroyid;
    GameObject astroidParent;
    bool createAstroyid;

    //Courutine array for all spawning objects
    bool[] coroutineStart;

    // Start is called before the first frame update
    void Start()
    {
        //Astroyids
        astroidParent = GameObject.Find("AstroyidParent");
        timeInbetweenCheckingAstroyid = Random.Range(0, chanceForAstroyidSpawn);

        //Courutine array for all spawning objects
        astroyidsCreated = new List<GameObject>{};
        coroutineStart = new bool[10]; //Number of diffrent objects being spawned
        for (int i = 0; i < coroutineStart.Length; i++) coroutineStart[i] = true;
    }

    // Update is called once per frame
    void Update()
    {
        Astroyids();

        Debugging();
    }

    private void Debugging()
    {
        
    }

    //Astroyids
    private void Astroyids()            //NOTE: Try to get the script to not call GetComponent during runtime
    {
        if (coroutineStart[0]) StartCoroutine(CheckAstroyids());

        if (createAstroyid)
        {
            float scale = Random.Range(1, astroyidMaximumSize);

            scale = Mathf.Clamp(scale, 1, astroyidMaximumSize);
            GameObject instAstroyid = Instantiate(astroid, transform);
            PlayerController.allObjects.Add(instAstroyid);
            Rigidbody2D instAstroyidRigidbody = instAstroyid.GetComponent<Rigidbody2D>();

            //Astroyids proporties
            instAstroyid.transform.parent = null;
            instAstroyid.transform.parent = astroidParent.transform;
            instAstroyid.GetComponent<SpriteRenderer>().sprite = astroidSprites[Random.Range(0, astroidSprites.Length)];
            instAstroyidRigidbody.velocity = new Vector2(Random.Range(-astroyidMaximumVelocity, astroyidMaximumVelocity), Random.Range(-astroyidMaximumVelocity, astroyidMaximumVelocity));
            instAstroyidRigidbody.AddTorque(Random.Range(-astroyidMaximumVelocity, astroyidMaximumVelocity));
            instAstroyid.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            instAstroyid.transform.localScale = new Vector3(scale, scale, 1);
            astroyidsCreated.Add(instAstroyid);

            //Astroyids position
            while ((transform.position - instAstroyid.transform.position).magnitude < distanceToSpawnAstroyids) instAstroyid.transform.position = new Vector3(transform.position.x + Random.Range(-distanceToSpawnAstroyids - 10, distanceToSpawnAstroyids + 10), transform.position.y + Random.Range(-distanceToSpawnAstroyids - 10, distanceToSpawnAstroyids + 10), 0);

            for (int i = 0; i < astroyidsCreated.Count; i++)
            {
                if (astroyidsCreated[i] != null)
                {
                    if ((transform.position - astroyidsCreated[i].transform.position).magnitude > distanceFromShipForDespawn)
                    {
                        Destroy(astroyidsCreated[i]);
                        astroyidsCreated.RemoveAt(i);
                    }
                }
                else astroyidsCreated.RemoveAt(i);
            }

            createAstroyid = false;
        }
    }

    public IEnumerator CheckAstroyids()
    {
        coroutineStart[0] = false;
        yield return new WaitForSeconds(timeInbetweenCheckingAstroyid);
        coroutineStart[0] = true;
        timeInbetweenCheckingAstroyid = Random.Range(0, chanceForAstroyidSpawn);
        createAstroyid = true;
        StopCoroutine(CheckAstroyids());
    }
}

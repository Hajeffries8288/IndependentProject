using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//                          NOTE: Maybe try to make a better name for this script but for now this should work

public class SpawningThings : MonoBehaviour
{
    //Astroyids
    [Header("Astroyids")]
    [HideInInspector] public static List<GameObject> astroyidsCreated;
    public float distanceToSpawnAstroyids;
    public float astroyidMaximumSize;
    public float astroyidMaximumVelocity;
    public float maximumTimeTellNextCheckForAstroyid;
    public int chanceForAstroyids_Int;
    public GameObject astroid;
    public Sprite[] astroidSprites;
    float timeInbetweenCheckingAstroyid;
    GameObject astroidParent;
    bool createAstroyid;

    //Misc
    public float distanceFromShipForDespawn;
    bool[] coroutineStart;

    // Start is called before the first frame update
    void Start()
    {
        //Astroyids
        astroidParent = GameObject.Find("AstroyidParent");
        timeInbetweenCheckingAstroyid = Random.Range(0, maximumTimeTellNextCheckForAstroyid);

        //Misc
        astroyidsCreated = new List<GameObject>{};
        coroutineStart = new bool[10];          //NOTE: This number is prone to change
        for (int i = 0; i < coroutineStart.Length; i++) coroutineStart[i] = true;
        GetComponent<CircleCollider2D>().radius = distanceFromShipForDespawn;
    }

    // Update is called once per frame
    void Update()
    {
        //Astroyids
        if (coroutineStart[0]) StartCoroutine(CheckAstroyids());
        if (createAstroyid)
        {
            float scale = Random.Range(0.1f, astroyidMaximumSize);
            scale = Mathf.Clamp(scale, 0.1f, astroyidMaximumSize);
            GameObject instAstroyid = Instantiate(astroid, transform);
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

        Debugging();
    }

    private void Debugging()
    {
        
    }

    public IEnumerator CheckAstroyids()
    {
        coroutineStart[0] = false;
        yield return new WaitForSeconds(timeInbetweenCheckingAstroyid);
        coroutineStart[0] = true;
        timeInbetweenCheckingAstroyid = Random.Range(0, maximumTimeTellNextCheckForAstroyid);
        createAstroyid = ((int)Random.Range(0, chanceForAstroyids_Int)) == 0;
        StopCoroutine(CheckAstroyids());
    }
}
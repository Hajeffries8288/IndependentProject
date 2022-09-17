using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstroyidCollectorScript : MonoBehaviour
{
    [HideInInspector] public GameObject[] astroyids;
    public int maxAstroyids;

    private void Start()
    {
        astroyids = new GameObject[maxAstroyids];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Contains("_Collectable"))
        {
            for (int i = 0; i < astroyids.Length; i++)
            {
                if (astroyids[i] == null)
                {
                    if (collision.transform.localScale.x < 2 && collision.transform.localScale.y < 2)
                    {
                        astroyids[i] = collision.transform.gameObject;
                        collision.GetComponent<Rigidbody2D>().simulated = false;
                        collision.GetComponent<Collider2D>().enabled = false;
                        collision.transform.parent = transform;
                        collision.transform.localPosition = new Vector3(Mathf.Round(collision.transform.localPosition.x), -1, 0);
                        break;
                    }
                }
            }
        }
    }
}

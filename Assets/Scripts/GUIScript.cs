using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIScript : MonoBehaviour
{
    //UpdateResorces
    public static Text resorcesText;

    void Start() //Start is called before the first frame update
    {
        //UpdateResorces
        resorcesText = GameObject.Find("Resorces").GetComponent<Text>();
        resorcesText.text = "Resorces: " + TractorBeemScript.astroyidsCollected;
    }

    void Update() //Update is called once per frame
    {
        
    }

    public static void UpdateResorces()
    {
        resorcesText.text = "Resorces: " + TractorBeemScript.astroyidsCollected;
    }
}

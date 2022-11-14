using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIScript : MonoBehaviour
{
    //UpdateResorces
    [Header("Resorces")]
    public static Text resorcesText;

    //DebugInfo
    public GameObject debugInfo;
    public string versionNumber;
    public int buildNumber;

    public static Text version;
    public static Text build;

    public bool showDebugInfo;

    void Start() //Start is called before the first frame update
    {
        //UpdateResorces
        resorcesText = GameObject.Find("Resorces").GetComponent<Text>();
        resorcesText.text = "Resorces: " + TractorBeemScript.astroyidsCollected;

        //DebugInfo
        if (Debug.isDebugBuild)
        {
            debugInfo.SetActive(true);

            version = GameObject.Find("Version").GetComponent<Text>();
            version.text = "Version: " + versionNumber;
            build = GameObject.Find("Build").GetComponent<Text>();
            build.text = "Build: " + buildNumber;
        }
    }

    void Update() //Update is called once per frame
    {
        
    }

    public static void UpdateResorces()
    {
        resorcesText.text = "Resorces: " + TractorBeemScript.astroyidsCollected;
    }

    public void DebugInfo()
    {
        showDebugInfo = showDebugInfo ? false : true;
        debugInfo.SetActive(showDebugInfo);
    }
}

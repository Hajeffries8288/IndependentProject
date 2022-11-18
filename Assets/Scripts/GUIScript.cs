using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Profiling;

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
    public static Text totalUsedMemory;

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
            System.GC.Collect();

            version = GameObject.Find("Version").GetComponent<Text>();
            build = GameObject.Find("Build").GetComponent<Text>();
            totalUsedMemory = GameObject.Find("TotalUsedMemory").GetComponent<Text>();

            version.text = "Version: " + versionNumber;
            build.text = "Build: " + buildNumber;
            totalUsedMemory.text = "Total Used Memory: " + Profiler.GetMonoUsedSizeLong();
        }
    }

    public static void PauseGUI()           //WORK ON THIS !!!!!!!!!!!!!!!
    {

    }

    public static void UpdateResorces()
    {
        resorcesText.text = "Resorces: " + TractorBeemScript.astroyidsCollected;
    }

    public static void UpdateTotalUsedMemory()
    {
        System.GC.Collect();
        totalUsedMemory.text = "Total Used Memory: " + Profiler.GetMonoUsedSizeLong();
    }
}

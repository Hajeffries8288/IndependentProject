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
    [Header("DebugInfo")]
    public GameObject debugInfo;
    public string versionNumber;
    public int buildNumber;

    public Text version;
    public Text build;
    public Text totalUsedMemory;

    public bool showDebugInfo;

    //PauseGUI
    [Header("Pause")]
    public GameObject pauseGUI;

    //BuildingGUI
    [Header("BuildingGUI")]
    public GameObject buildingGUI;

    Button buildingGUIButton;
    bool open = true;

    void Start() //Start is called before the first frame update
    {
        //UpdateResorces
        resorcesText = GameObject.Find("Resorces").GetComponent<Text>();
        resorcesText.text = "Resorces: " + TractorBeemScript.astroyidsCollected;

        //BuildingGUI
        buildingGUIButton = GameObject.Find("BuildingGUIButton").GetComponent<Button>();

        //DebugInfo
        if (Debug.isDebugBuild)
        {
            showDebugInfo = true;
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

    public void Pause()
    {
        pauseGUI.SetActive(pauseGUI.activeSelf ? false : true);
        Time.timeScale = pauseGUI.activeSelf ? 0 : 1;
    }

    public void UpdateResorces()
    {
        resorcesText.text = "Resorces: " + TractorBeemScript.astroyidsCollected;
    }

    public void UpdateTotalUsedMemory()
    {
        System.GC.Collect();
        totalUsedMemory.text = "Total Used Memory: " + Profiler.GetMonoUsedSizeLong();
    }

    public void debugInfoVisible()
    {
        showDebugInfo = showDebugInfo ? false : true;
        debugInfo.SetActive(showDebugInfo);
    }

    //GUI Buttons
    public void ExitGame()
    {
        if (!UnityEditor.EditorApplication.isPlaying) Application.Quit();
        else UnityEditor.EditorApplication.isPlaying = false;
    }

    public void BuildingGUIButton()
    {
        open = open ? false : true;
        buildingGUI.SetActive(open);
        buildingGUIButton.transform.localPosition = open ? Vector3.right * -259.5422f : Vector3.right * -392.88f;
    }
}

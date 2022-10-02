using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Misc_ScriptableObject : ScriptableObject
{
    public List<GameObject> allObjects;
    private void OnEnable()
    {
        allObjects = new List<GameObject>(FindObjectsOfType<GameObject>());
    }

    public List<GameObject> AllObjects
    {
        get {return allObjects;}
    }
}

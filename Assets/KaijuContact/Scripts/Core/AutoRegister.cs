using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRegister : MonoBehaviour
{
    public string key;

    void Awake()
    {
        GlobalVars.globalGameObjects[key] = this.gameObject;
        GameObject g;
        if (!GlobalVars.globalGameObjects.TryGetValue(key, out g))
        {
            Debug.LogWarning("Registration of GameObject with key \"" + key + "\" failed.");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRegister : MonoBehaviour, IRegistrationEntry
{
    public string key;
    public bool hideOnStart = false;

    public void Init()//fuck disabled scripts, portal time
    {
        GlobalVars.globalGameObjects[key] = this.gameObject;
        GameObject g;
        if (!GlobalVars.globalGameObjects.TryGetValue(key, out g))
        {
            Debug.LogWarning("Registration of GameObject with key \"" + key + "\" failed.");
        }
    }

    private void Start()
    {
        if(hideOnStart)
        {
            this.gameObject.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This script is the core of the auto registration system. on startup, it scans the hierarchy and adds anything it can find that is marked for registrtion
 * to the GlobalVars.globalGameObjects dictionary
 * 
 * TO USE:
 * 
 * 1. Add AutoRegister script to the object you want to register
 * 2. set the key to use to access that object (must be unique!)
 * 3. thats it!
 * 
 * Note: objects do NOT have to be active to be registered using this system, they can be disabled in the hierarchy if need be
*/

public class AutoRegisterObjects : MonoBehaviour
{
    //super intensive, called exactly once at startup
    private void Awake()
    {
        var scripts = gameObject.GetComponentsInChildren<IRegistrationEntry>(true);//now you are thinking with portals

        foreach (var script in scripts)
        {
            script.Init();
        }
    }
}

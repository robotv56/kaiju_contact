using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRegisterObjects : MonoBehaviour
{
    //super intensive, called exactly once at startup
    private void Awake()
    {
        var scripts = gameObject.GetComponentsInChildren<IRegistrationEntry>(true);

        foreach (var script in scripts)
            script.Init();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVars;

public class RegistryDebug : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            //globalGameObjects
            Debug.Log("Initiating dump of registry entries...");
            int n = 0;
            foreach(KeyValuePair<string, GameObject> p in globalGameObjects)
            {
                Debug.Log("Entry " + (++n) + ": key: \"" + p.Key + "\", value: \"" + p.Value.name + "\"");
            }

            Debug.Log("Dump complete, " + n + " entries found");
        }
    }
}

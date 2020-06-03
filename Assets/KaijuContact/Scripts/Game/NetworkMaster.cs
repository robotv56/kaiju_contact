using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkMaster : NetworkBehaviour
{
    private NetworkManager manager;

    private void Start()
    {
        manager = GetComponent<NetworkManager>();
    }

    private void Update()
    {
        if (isLocalPlayer)
        {

        }
    }
}

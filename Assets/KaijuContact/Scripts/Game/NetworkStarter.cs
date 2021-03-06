﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static GlobalVars;

public class NetworkStarter : MonoBehaviour
{
    int n = 0;
    
    void Start()
    {
        manager = GetComponent<NetworkManager>();
    }

    void Update()
    {
        if (manager.matchMaker != null && n < 2)
        {
            if (manager.matches == null && n == 0)
            {
                n++;
                manager.matchMaker.ListMatches(0, 20, "", false, 0, 0, manager.OnMatchList);
            }
            else if (manager.matches != null)
            {
                if (manager.matches.Count > 0)
                {
                    for (int i = 0; i < manager.matches.Count; i++)
                    {
                        if (manager.matches[i].currentSize < 8 && manager.matches[i].currentSize > 0)
                        {
                            //Join
                            manager.matchName = manager.matches[i].name;
                            manager.matchMaker.JoinMatch(manager.matches[i].networkId, "", "", "", 0, 0, manager.OnMatchJoined);
                            n++;
                            return;
                        }
                    }
                    //Host
                    manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", "", "", 0, 0, manager.OnMatchCreate);
                    n++;
                }
                else
                {
                    //Host
                    manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", "", "", 0, 0, manager.OnMatchCreate);
                    n++;
                }
            }
        }
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Crest;
using static GlobalVars;

//disable deprecation warnings
#pragma warning disable CS0414
#pragma warning disable CS0618

public class ClientMaster : NetworkBehaviour
{
    [SyncVar] float kaijuHealth;
    [SyncVar] float shipHealth;

    private Text healthUI;

    [SyncVar] bool isKaiju;
    [SyncVar] Weapons shipWeapon = Weapons.RAILGUN;
    [SerializeField] private GameObject[] shipWeapons = { null, null, null };
    [SerializeField] private GameObject[] playerObjects = { null, null };
    [SerializeField] private GameObject[] playerPivots = { null, null };
    [SerializeField] private GameObject[] playerCameras = { null, null };
    private KaijuCore kaijuCore = null;
    private ShipCore shipCore = null;
    private ShipTurret shipTurret = null;
    private PlayerKaijuController kaijuController = null;
    private PlayerShipController shipController = null;

    private float updateTime = 0f;

    // Kaiju & Ship
    [SyncVar] Vector3 playerPosition;
    // Kaiju
    [SyncVar] float playerDesiredRotation;
    [SyncVar] float playerKaijuRotation;
    [SyncVar] Vector3 playerDesiredMovement;
    [SyncVar] Vector3 playerKaijuMovement;
    [SyncVar] float playerSubmerged;
    [SyncVar] int playerSlashCount = 0;
    private int slashCount = 0;
    // Ship
    [SyncVar] float playerRudder;
    [SyncVar] float playerActualRudder;
    [SyncVar] float playerShipRotation;
    [SyncVar] float playerThrottle;
    [SyncVar] float playerShipSpeed;
    [SyncVar] Vector3 playerAimPoint;
    [SyncVar] Vector3 playerTargetVelocity;
    [SyncVar] int playerShotsFired = 0;
    private int shotsFired = 0;
    [SyncVar] int lastShotMask;
    [SyncVar] Vector3 lastShotPosition;
    [SyncVar] Quaternion lastShotRotation;
    [SyncVar] Vector3 lastShotVelocity;

    //used for storing gameobjects locally, helps save memory
    //is VOLATILE, it will often switch to reference different things, only use locally
    private GameObject gameObjectCache;
    private bool hasStarted = false;//used for starting camera
    private GameObject kaijuTracker;

    [SerializeField] private GameObject icebergMasterPrefab;

    private void Start()
    {
        Debug.Log(0);
        CmdResetHealth();
        if (isLocalPlayer)
        {
            if(globalGameObjects.TryGetValue("canvas", out gameObjectCache))
            {
                //TODO ui
                healthUI = gameObjectCache.transform.Find("Health UI").GetComponent<Text>();
            }
            else
            {
                Debug.LogError("Could not find canvas");
            }
            Debug.Log(1);
        }
        if (isLocalPlayer && isServer)
        {
            Debug.Log(2);
            NetworkServer.SpawnWithClientAuthority(Instantiate(icebergMasterPrefab, Vector3.zero, Quaternion.identity), connectionToClient);
            //GameObject.Find("Multiplayer_Manager").GetComponent<NetworkManager>().customConfig = true;
            //GameObject.Find("Multiplayer_Manager").GetComponent<NetworkManager>().connectionConfig.MaxCombinedReliableMessageSize = 248;
            //GameObject.Find("Multiplayer_Manager").GetComponent<NetworkManager>().connectionConfig.MaxCombinedReliableMessageCount = 248;
            //GameObject.Find("Multiplayer_Manager").GetComponent<NetworkManager>().connectionConfig.MaxSentMessageQueueSize = 248;

            //setting cache here
            if(globalGameObjects.TryGetValue("multiplayer_manager", out gameObjectCache))
            {
                gameObjectCache.GetComponent<NetworkManager>().customConfig = true;
                gameObjectCache.GetComponent<NetworkManager>().connectionConfig.MaxCombinedReliableMessageSize = 248;
                gameObjectCache.GetComponent<NetworkManager>().connectionConfig.MaxCombinedReliableMessageCount = 248;
                gameObjectCache.GetComponent<NetworkManager>().connectionConfig.MaxSentMessageQueueSize = 248;
            }
            else
            {
                Debug.LogError("Could not find Multiplayer Manager");
            }

            //NetworkServer.Configure(Network.ConnectionConfig)
        }
        Debug.Log(3);
        kaijuCore = playerObjects[0].GetComponent<KaijuCore>();
        shipCore = playerObjects[1].GetComponent<ShipCore>();
        shipTurret = playerObjects[1].transform.Find("Ship Pivot/Ship Body/Turret Base/Turret Horizontal").GetComponent<ShipTurret>();
        kaijuController = playerObjects[0].GetComponent<PlayerKaijuController>();
        shipController = playerObjects[1].GetComponent<PlayerShipController>();
        Debug.Log(playerObjects[1]);
        Debug.Log(shipController);
        Debug.Log(4);
        isKaiju = false;
        globalGameObjects.TryGetValue("kaiju_tracker", out kaijuTracker);
    }

    private void Update()
    {
        // Set Active Weapon
        if (shipWeapon == Weapons.RAILGUN && (!shipWeapons[0].activeSelf || shipWeapons[1].activeSelf || shipWeapons[2].activeSelf))
        {
            shipWeapons[0].SetActive(true);
            shipWeapons[1].SetActive(false);
            shipWeapons[2].SetActive(false);
        }
        if (shipWeapon == Weapons.LASER && (shipWeapons[0].activeSelf || !shipWeapons[1].activeSelf || shipWeapons[2].activeSelf))
        {
            shipWeapons[0].SetActive(false);
            shipWeapons[1].SetActive(true);
            shipWeapons[2].SetActive(false);
        }
        if (shipWeapon == Weapons.MINIGUN && (shipWeapons[0].activeSelf || shipWeapons[1].activeSelf || !shipWeapons[2].activeSelf))
        {
            shipWeapons[0].SetActive(false);
            shipWeapons[1].SetActive(false);
            shipWeapons[2].SetActive(true);
        }

        // Set Kaiju/Ship
        if (isKaiju && !playerObjects[0].activeSelf)
        {
            playerObjects[0].SetActive(true);
            playerObjects[1].SetActive(false);
            globalGameObjects["kaiju_track_point"] = playerObjects[0].transform.Find("KaijuTrackPoint").gameObject;
        }
        if (!isKaiju && !playerObjects[1].activeSelf)
        {
            playerObjects[0].SetActive(false);
            playerObjects[1].SetActive(true);
        }

        // Set Local Camera
        if (isLocalPlayer && ((isKaiju && (!playerCameras[0].activeSelf || playerCameras[1].activeSelf)) || (!isKaiju && (playerCameras[0].activeSelf || !playerCameras[1].activeSelf))))
        {
            playerCameras[0].SetActive(isKaiju);
            playerCameras[1].SetActive(!isKaiju);

            //setting cache here
            if (globalGameObjects.TryGetValue("ocean", out gameObjectCache))
            {
                if (isKaiju)
                {
                    gameObjectCache.GetComponent<OceanRenderer>().Viewpoint = playerCameras[0].transform;
                }
                else
                {
                    gameObjectCache.GetComponent<OceanRenderer>().Viewpoint = playerCameras[1].transform;
                }
            }
            else Debug.LogError("Could not find the ocean");

            //setting cache here
            if (globalGameObjects.TryGetValue("starting_camera", out gameObjectCache) && gameObjectCache.active)
            {
                gameObjectCache.SetActive(false);
                hasStarted = true;
            }
            else if(!hasStarted)
            {
                Debug.LogError("Could not find starting camera");
            }
        }
        else if (!isLocalPlayer && (playerCameras[0].activeSelf || playerCameras[1].activeSelf))
        {
            playerCameras[0].SetActive(false);
            playerCameras[1].SetActive(false);
        }

        // Send/Receive Player Info
        if (isKaiju && kaijuController.isLocal != isLocalPlayer)
        {
            kaijuController.isLocal = isLocalPlayer;
        }
        if (!isKaiju && shipController.isLocal != isLocalPlayer)
        {
            shipController.isLocal = isLocalPlayer;
        }
        updateTime += Time.deltaTime;

        if (updateTime > 0.2f)
        {
            updateTime = 0f;
            int k = 0;
            if (!isKaiju)
            {
                k = 1;
            }
            if (isLocalPlayer)
            {
                CmdUpdatePlayer(
                    playerObjects[k].transform.position,
                    kaijuCore.GetDesiredRotation(),
                    kaijuCore.GetRotation(),
                    kaijuCore.GetDesiredMovement(),
                    kaijuCore.GetMovement(),
                    kaijuCore.GetSubmerged(),
                    shipCore.GetRudder(),
                    shipCore.GetActualRudder(),
                    shipCore.GetRotation(),
                    shipCore.GetThrottle(),
                    shipCore.GetSpeed(),
                    shipTurret.GetAimPoint(),
                    shipTurret.GetTargetVelocity()
                    );
            }
            else
            {
                playerObjects[k].transform.position = playerPosition;
                kaijuCore.SetDesiredRotation(playerDesiredRotation);
                kaijuCore.HardSetRotation(playerKaijuRotation);
                kaijuCore.SetDesiredMovement(playerDesiredMovement);
                kaijuCore.HardSetMovement(playerKaijuMovement);
                kaijuCore.HardSetSubmerged(playerSubmerged);
                shipCore.SetRudder(playerRudder);
                shipCore.HardSetRudder(playerActualRudder);
                shipCore.HardSetRotation(playerShipRotation);
                shipCore.SetThrottle(playerThrottle);
                shipCore.HardSetSpeed(playerShipSpeed);
                shipTurret.UpdateAim(playerAimPoint, playerTargetVelocity);
                if (playerSlashCount > slashCount)
                {
                    slashCount = playerSlashCount;
                    kaijuCore.HardTriggerSlash();
                }
                if (playerShotsFired > shotsFired)
                {
                    shotsFired = playerShotsFired;
                    shipTurret.HardFireCannon(lastShotMask, lastShotPosition, lastShotRotation, lastShotVelocity);
                }
            }
        }
        //UI control
        if (isLocalPlayer)
        {
            if (isKaiju)
            {
                healthUI.text = "Health: " + kaijuHealth;
                kaijuTracker.SetActive(false);
            }
            else
            {
                healthUI.text = "Health: " + shipHealth;
                //setting cache here
                if(globalGameObjects.TryGetValue("kaiju_track_point", out gameObjectCache))
                {
                    if(!kaijuTracker.active)
                    {
                        kaijuTracker.SetActive(true);
                    }
                    var vect = playerCameras[1].GetComponent<Camera>().WorldToScreenPoint(gameObjectCache.transform.position);
                    var z = vect.z;
                    vect.z = 0;
                    kaijuTracker.transform.position = vect;

                    var r = kaijuTracker.GetComponent<RectTransform>();

                    var distance = Vector3.Distance(gameObjectCache.transform.position, playerCameras[1].transform.position);

                    r.sizeDelta = new Vector2(1000,1000) * (1/distance * 100 * (float)goldenRatio);

                    if(Mathf.Sign(z)<0)
                    {
                        //maybe have an onscreen indicator to show which way to turn
                        kaijuTracker.GetComponent<Image>().enabled = false;
                    }
                    else
                    {
                        kaijuTracker.GetComponent<Image>().enabled = true;
                    }
                }
            }
        }

        /*if (transform.Find("Kaiju").Find("Kaiju Pivot").gameObject.activeSelf != (kaijuHealth > 0f && isKaiju))
        {
            transform.Find("Kaiju").Find("Kaiju Pivot").gameObject.SetActive(kaijuHealth > 0f && isKaiju);
        }
        if (transform.Find("Ship").Find("Ship Pivot").gameObject.activeSelf != (shipHealth > 0f && !isKaiju))
        {
            transform.Find("Ship").Find("Ship Pivot").gameObject.SetActive(kaijuHealth > 0f && !isKaiju);
        }*/
    }

    private void UpdatePlayer(Vector3 pos, float kdr, float kr, Vector3 kdm, Vector3 km, float ks, float srud, float sarud, float sr, float st, float ss, Vector3 sap, Vector3 stv)
    {
        if (isServer)
        {
            RpcUpdatePlayer(pos, kdr, kr, kdm, km, ks, srud, sarud, sr, st, ss, sap, stv);
        }
        else
        {
            CmdUpdatePlayer(pos, kdr, kr, kdm, km, ks, srud, sarud, sr, st, ss, sap, stv);
        }
    }

    [Command(channel = 1)]
    private void CmdUpdatePlayer(Vector3 pos, float kdr, float kr, Vector3 kdm, Vector3 km, float ks, float srud, float sarud, float sr, float st, float ss, Vector3 sap, Vector3 stv)
    {
        playerPosition = pos;
        playerDesiredRotation = kdr;
        playerKaijuRotation = kr;
        playerDesiredMovement = kdm;
        playerKaijuMovement = km;
        playerSubmerged = ks;
        playerRudder = srud;
        playerActualRudder = sarud;
        playerShipRotation = sr;
        playerThrottle = st;
        playerShipSpeed = ss;
        playerAimPoint = sap;
        playerTargetVelocity = stv;
        RpcUpdatePlayer(pos, kdr, kr, kdm, km, ks, srud, sarud, sr, st, ss, sap, stv);
    }

    [ClientRpc(channel = 1)]
    private void RpcUpdatePlayer(Vector3 pos, float kdr, float kr, Vector3 kdm, Vector3 km, float ks, float srud, float sarud, float sr, float st, float ss, Vector3 sap, Vector3 stv)
    {
        playerPosition = pos;
        playerDesiredRotation = kdr;
        playerKaijuRotation = kr;
        playerDesiredMovement = kdm;
        playerKaijuMovement = km;
        playerSubmerged = ks;
        playerRudder = srud;
        playerActualRudder = sarud;
        playerShipRotation = sr;
        playerThrottle = st;
        playerShipSpeed = ss;
        playerAimPoint = sap;
        playerTargetVelocity = stv;
    }

    [Command(channel = 2)]
    public void CmdTriggerCreateIceberg(float x, float z)
    {
        GameObject.Find("IcebergMaster").GetComponent<IcebergMaster>().CreateIceberg(x, z);
    }

    [Command(channel = 2)]
    public void CmdTriggerDamageIceberg(int icebergID, float damage)
    {
        GameObject.Find("IcebergMaster").GetComponent<IcebergMaster>().DamageIceberg(icebergID, damage);
    }

    [Command]
    public void CmdTriggerSlash()
    {
        playerSlashCount++;
        if (isLocalPlayer)
        {
            slashCount++;
        }
    }

    [Command]
    public void CmdHardFireCannon(int mask, Vector3 pos, Quaternion rot, Vector3 vel)
    {
        playerShotsFired++;
        if (isLocalPlayer)
        {
            shotsFired++;
        }
        lastShotMask = mask;
        lastShotPosition = pos;
        lastShotRotation = rot;
        lastShotVelocity = vel;
    }

    [Command]
    public void CmdSetKaiju()
    {
        isKaiju = !isKaiju;
        CmdResetHealth();
    }

    [Command]
    public void CmdSetShipWeapon(Weapons selection)
    {
        shipWeapon = selection;
    }

    public bool GetIsLocalPlayer()
    {
        return isLocalPlayer;
    }

    [Command]
    private void CmdResetHealth()
    {
        kaijuHealth = 250f;
        shipHealth = 50f;
    }

    public void CmdDamageKaiju(float damage)
    {
        kaijuHealth = Mathf.Clamp(kaijuHealth - damage, 0f, kaijuHealth);
    }

    public void CmdDamageShip(float damage)
    {
        shipHealth = Mathf.Clamp(shipHealth - damage, 0f, shipHealth);
    }
}
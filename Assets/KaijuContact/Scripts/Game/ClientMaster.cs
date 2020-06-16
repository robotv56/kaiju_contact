using System.Collections;
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
    private byte playerNumber;
    [SyncVar] string playerName;
    private bool gameMasterActive = false;

    [SyncVar] float kaijuHealth;
    [SyncVar] float shipHealth;

    private Text healthUI;

    [SyncVar] bool playing;
    [SyncVar] bool kaijuOptIn;
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
    //private bool hasStarted = false;//used for starting camera
    private GameObject kaijuTracker;

    [SerializeField] private GameObject gameMasterPrefab;
    [SerializeField] private GameObject icebergMasterPrefab;
    [SerializeField] private GameObject missileLauncher;

    private GameObject icebergMaster;
    private GameObject startingCamera;
    private GameObject ocean;
    private GameObject hud;

    private void Start()
    {
        playing = false;
        if (gameMaster != null)
        {
            playerNumber = gameMaster.AddPlayer(gameObject);
            gameMasterActive = true;
        }
        else if (isLocalPlayer && isServer)
        {
            NetworkServer.SpawnWithClientAuthority(Instantiate(gameMasterPrefab, Vector3.zero, Quaternion.identity), connectionToClient);
        }

        //Debug.Log(0);
        ResetHealth();
        if (isLocalPlayer)
        {
            // Generate a name for the player
            CmdSetPlayerName(namePool[Random.Range(0, namePool.Length)] + "_" + Random.Range(0,99));

            GlobalVars.globalGameObjects.TryGetValue("hud", out hud);
            healthUI = hud.transform.Find("Health UI").GetComponent<Text>();
            
            //Debug.Log(1);
        }
        if (isLocalPlayer && isServer)
        {
            //Debug.Log(2);
            icebergMaster = Instantiate(icebergMasterPrefab, Vector3.zero, Quaternion.identity);
            NetworkServer.SpawnWithClientAuthority(icebergMaster, connectionToClient);
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
        //Debug.Log(3);
        kaijuCore = playerObjects[0].GetComponent<KaijuCore>();
        shipCore = playerObjects[1].GetComponent<ShipCore>();
        shipTurret = playerObjects[1].transform.Find("Ship Pivot/Ship Body/Turret Base/Turret Horizontal").GetComponent<ShipTurret>();
        kaijuController = playerObjects[0].GetComponent<PlayerKaijuController>();
        shipController = playerObjects[1].GetComponent<PlayerShipController>();
        Debug.Log(playerObjects[1]);
        Debug.Log(shipController);
        //Debug.Log(4);
        isKaiju = false;
        globalGameObjects.TryGetValue("kaiju_tracker", out kaijuTracker);

        
        if (isLocalPlayer)
        {
            GlobalVars.globalGameObjects.TryGetValue("starting_camera", out startingCamera);
            GlobalVars.globalGameObjects.TryGetValue("ocean", out ocean);
        }
    }

    private void OnDestroy()
    {
        gameMaster.RemovePlayer(playerNumber);
    }

    private void Update()
    {
        // Add Player To GameMaster
        if (!gameMasterActive && gameMaster != null)
        {
            playerNumber = gameMaster.AddPlayer(gameObject);
            gameMasterActive = true;
        }

        // Health / Set Alive
        if (playing && isKaiju && (kaijuHealth > 0f) != kaijuController.isAlive)
        {
            kaijuController.isAlive = kaijuHealth > 0f;
            if (playing && kaijuCore.dying != (kaijuHealth <= 0f))
            {
                kaijuCore.dying = kaijuHealth <= 0f;
            }
        }
        if (playing && !isKaiju && (shipHealth > 0f) != shipController.isAlive)
        {
            shipController.isAlive = shipHealth > 0f;
            if (playing && shipCore.dying != (shipHealth <= 0f))
            {
                shipCore.dying = shipHealth <= 0f;
            }
        }
        // Set Active Weapon
        if (playing && shipWeapon == Weapons.RAILGUN && (!shipWeapons[0].activeSelf || shipWeapons[1].activeSelf || shipWeapons[2].activeSelf))
        {
            shipWeapons[0].SetActive(true);
            shipWeapons[1].SetActive(false);
            shipWeapons[2].SetActive(false);
        }
        if (playing && shipWeapon == Weapons.LASER && (shipWeapons[0].activeSelf || !shipWeapons[1].activeSelf || shipWeapons[2].activeSelf))
        {
            shipWeapons[0].SetActive(false);
            shipWeapons[1].SetActive(true);
            shipWeapons[2].SetActive(false);
        }
        if (playing && shipWeapon == Weapons.MINIGUN && (shipWeapons[0].activeSelf || shipWeapons[1].activeSelf || !shipWeapons[2].activeSelf))
        {
            shipWeapons[0].SetActive(false);
            shipWeapons[1].SetActive(false);
            shipWeapons[2].SetActive(true);
        }
        if (!playing && (shipWeapons[0].activeSelf || shipWeapons[1].activeSelf || shipWeapons[2].activeSelf))
        {
            shipWeapons[0].SetActive(false);
            shipWeapons[1].SetActive(false);
            shipWeapons[2].SetActive(false);
        }

        // Set Kaiju/Ship
        if (playing && isKaiju && !playerObjects[0].activeSelf)
        {
            playerObjects[0].SetActive(true);
            playerObjects[1].SetActive(false);
            globalGameObjects["kaiju_track_point"] = playerObjects[0].transform.Find("KaijuTrackPoint").gameObject;
        }
        if (playing && !isKaiju && !playerObjects[1].activeSelf)
        {
            playerObjects[0].SetActive(false);
            playerObjects[1].SetActive(true);
        }
        if (!playing && (playerObjects[0].activeSelf || playerObjects[1].activeSelf))
        {
            playerObjects[0].SetActive(false);
            playerObjects[1].SetActive(false);
        }

        // Set Local Camera
        if (playing && isLocalPlayer && ((isKaiju && (!playerCameras[0].activeSelf || playerCameras[1].activeSelf)) || (!isKaiju && (playerCameras[0].activeSelf || !playerCameras[1].activeSelf))))
        {
            playerCameras[0].SetActive(isKaiju);
            playerCameras[1].SetActive(!isKaiju);

            startingCamera.SetActive(false);
            if (isKaiju)
            {
                ocean.GetComponent<OceanRenderer>().Viewpoint = playerCameras[0].transform;
            }
            else
            {
                ocean.GetComponent<OceanRenderer>().Viewpoint = playerCameras[1].transform;
            }
            /*
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
            if (globalGameObjects.TryGetValue("starting_camera", out gameObjectCache) && gameObjectCache.activeSelf)
            {
                gameObjectCache.SetActive(false);
                //hasStarted = true;
            }
            else// if(!hasStarted)
            {
                Debug.LogError("Could not find starting camera");
            }*/
        }
        else if ((!playing || !isLocalPlayer) && (playerCameras[0].activeSelf || playerCameras[1].activeSelf))
        {
            playerCameras[0].SetActive(false);
            playerCameras[1].SetActive(false);
        }

        // Send/Receive Player Info
        if (playing && isKaiju && kaijuController.isLocal != isLocalPlayer)
        {
            kaijuController.isLocal = isLocalPlayer;
        }
        if (playing && !isKaiju && shipController.isLocal != isLocalPlayer)
        {
            shipController.isLocal = isLocalPlayer;
        }
        updateTime += Time.deltaTime;

        if (playing && updateTime > 0.2f)
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
                kaijuTracker.SetActive(true);
                healthUI.text = "Health: " + shipHealth;
                //setting cache here
                if(globalGameObjects.TryGetValue("kaiju_track_point", out gameObjectCache))
                {
                    if(!kaijuTracker.active && !kaijuCore.IsSubmerged())
                    {
                        kaijuTracker.SetActive(true);
                    }
                    
                    if(kaijuCore.IsSubmerged())
                    {
                        Debug.Log("submerged");
                        kaijuTracker.SetActive(false);
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

    public bool GetIsKaiju()
    {
        return isKaiju;
    }

    public string GetName()
    {
        return playerName;
    }

    public bool GetKaijuOptIn()
    {
        return kaijuOptIn;
    }

    public float GetHealth()
    {
        if (isKaiju)
        {
            return kaijuHealth;
        }
        else
        {
            return shipHealth;
        }
    }

    public GameObject GetIcebergMaster()
    {
        if (isServer && isLocalPlayer)
        {
            return icebergMaster;
        }
        else
        {
            return null;
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

    //TODO use register system
    [Command(channel = 2)]
    public void CmdTriggerCreateIceberg(float x, float z)
    {
        globalGameObjects["iceberg_master"].GetComponent<IcebergMaster>().CreateIceberg(x, z);//fuck the cache
    }

    [Command(channel = 2)]
    public void CmdTriggerDamageIceberg(int icebergID, float damage)
    {
        globalGameObjects["iceberg_master"].GetComponent<IcebergMaster>().DamageIceberg(icebergID, damage);
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
    public void CmdSetShipWeapon(Weapons selection)
    {
        shipWeapon = selection;
    }

    public bool GetIsLocalPlayer()
    {
        return isLocalPlayer;
    }
    
    [Command]
    private void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    [Command]
    public void CmdUpdateKaijuOptIn(bool choice)
    {
        kaijuOptIn = choice;
    }

    [Command]
    public void CmdDamageKaiju(float damage)
    {
        kaijuHealth = Mathf.Clamp(kaijuHealth - damage, 0f, kaijuHealth);
        RpcSetKaijuHealth(kaijuHealth);
    }

    [ClientRpc]
    public void RpcSetKaijuHealth(float health)
    {
        kaijuHealth = health;
    }

    [Command]
    public void CmdDamageShip(float damage)
    {
        shipHealth = Mathf.Clamp(shipHealth - damage, 0f, shipHealth);
        RpcSetShipHealth(shipHealth);
    }

    [ClientRpc]
    public void RpcSetShipHealth(float health)
    {
        shipHealth = health;
    }

    private void ResetHealth()
    {
        kaijuHealth = 100f;
        shipHealth = 50f;
    }

    public void TriggerSetKaiju(bool choice, Vector3 spawnPos, float spawnRot)
    {
        SetKaiju(choice, spawnPos, spawnRot);
        RpcSetKaiju(choice, spawnPos, spawnRot);
    }

    private void SetKaiju(bool choice, Vector3 spawnPos, float spawnRot)
    {
        playing = true;
        isKaiju = choice;
        ResetHealth();

        playerPosition = spawnPos;
        if (isKaiju)
        {
            playerObjects[0].transform.position = spawnPos;
            if (isLocalPlayer)
            {
                kaijuController.HardSetCamera();
            }
            playerDesiredRotation = spawnRot;
            kaijuCore.SetDesiredRotation(spawnRot);
            playerKaijuMovement = new Vector3(0f, 0f, -800f);
            kaijuCore.HardSetMovement(new Vector3(0f, 0f, -800f));
            playerKaijuRotation = spawnRot;
            kaijuCore.HardSetRotation(spawnRot);
            playerSubmerged = 7.5f;
            kaijuCore.HardSetSubmerged(7.5f);
        }
        else
        {
            playerObjects[1].transform.position = spawnPos;
            playerShipRotation = spawnRot;
            shipCore.HardSetRotation(spawnRot);
        }

        /*CmdUpdatePlayer(
            spawnPos,
            spawnRot,
            spawnRot,
            playerDesiredMovement,
            playerKaijuMovement,
            playerSubmerged,
            playerRudder,
            playerActualRudder,
            spawnRot,
            playerThrottle,
            playerShipSpeed,
            playerAimPoint,
            playerTargetVelocity
        );*/
    }

    [ClientRpc]
    private void RpcSetKaiju(bool choice, Vector3 spawnPos, float spawnRot)
    {
        SetKaiju(choice, spawnPos, spawnRot);
    }

    public void TriggerEndMatch()
    {
        EndMatch();
        RpcEndMatch();
    }

    private void EndMatch()
    {
        playing = false;
        if (isLocalPlayer)
        {

            startingCamera.SetActive(true);
            ocean.GetComponent<OceanRenderer>().Viewpoint = startingCamera.transform;
        }
    }

    [ClientRpc]
    private void RpcEndMatch()
    {
        EndMatch();
    }

    public void TriggerResetMatch()
    {
        ResetMatch();
        RpcResetMatch();
    }

    private void ResetMatch()
    {
        if (isKaiju)
        {
            kaijuCore.dying = false;
            kaijuController.isAlive = true;
        }
        else
        {
            shipCore.dying = false;
            shipController.isAlive = true;
        }
        isKaiju = false;
        ResetHealth();
    }

    [ClientRpc]
    private void RpcResetMatch()
    {
        ResetMatch();
    }

    [Command(channel = 2)]
    public void CmdSyncMissile(Vector3 cameraPos, Vector3 cameraDir)
    {
        SyncMissile(cameraPos, cameraDir);
        RpcSyncMissile(cameraPos, cameraDir);
    }

    [ClientRpc(channel = 2)]
    private void RpcSyncMissile(Vector3 cameraPos, Vector3 cameraDir)
    {
        SyncMissile(cameraPos, cameraDir);
    }

    private void SyncMissile(Vector3 cameraPos, Vector3 cameraDir)
    {
        missileLauncher.GetComponent<MissileLauncher>().SetAimPoint(cameraPos, cameraDir);
    }

    [Command]
    public void CmdFireMissiles()
    {
        FireMissiles();
        RpcFireMissiles();
    }
    
    [ClientRpc]
    private void RpcFireMissiles()
    {
        FireMissiles();
    }

    private void FireMissiles()
    {
        missileLauncher.GetComponent<MissileLauncher>().Launch();
    }
}

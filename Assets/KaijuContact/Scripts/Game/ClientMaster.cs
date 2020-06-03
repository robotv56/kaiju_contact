using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Crest;

public enum ShipWeapon
{
    GAUSS,
    LASER,
    ROTARY
}

public class ClientMaster : NetworkBehaviour
{
    [SyncVar] bool isKaiju;
    [SyncVar] ShipWeapon shipWeapon = ShipWeapon.GAUSS;
    [SerializeField] private GameObject[] shipWeapons = { null, null, null };
    [SerializeField] private GameObject[] playerObjects = { null, null };
    [SerializeField] private GameObject[] playerPivots = { null, null };
    [SerializeField] private GameObject[] playerCameras = { null, null };
    private KaijuCore kaijuCore = null;
    private ShipCore shipCore = null;
    private ShipTurret shipTurret = null;
    private PlayerKaijuController kaijuController = null;
    private PlayerShipController shipController = null;

    // Kaiju & Ship
    [SyncVar] Vector3 playerPosition;
    // Kaiju
    [SyncVar] float playerDesiredRotation;
    [SyncVar] float playerKaijuRotation;
    [SyncVar] Vector3 playerDesiredMovement;
    [SyncVar] Vector3 playerKaijuMovement;
    [SyncVar] float playerSubmerged;
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

    private void Start()
    {
        kaijuCore = playerObjects[0].GetComponent<KaijuCore>();
        shipCore = playerObjects[1].GetComponent<ShipCore>();
        shipTurret = playerObjects[1].transform.Find("Ship Pivot").Find("Ship Body").Find("Turret Base").Find("Turret Horizontal").GetComponent<ShipTurret>();
        kaijuController = playerObjects[0].GetComponent<PlayerKaijuController>();
        shipController = playerObjects[1].GetComponent<PlayerShipController>();

        isKaiju = false;
    }

    private void Update()
    {
        // Set Active Weapon
        if (shipWeapon == ShipWeapon.GAUSS && (!shipWeapons[0].activeSelf || shipWeapons[1].activeSelf || shipWeapons[2].activeSelf))
        {
            shipWeapons[0].SetActive(true);
            shipWeapons[1].SetActive(false);
            shipWeapons[2].SetActive(false);
        }
        if (shipWeapon == ShipWeapon.LASER && (shipWeapons[0].activeSelf || !shipWeapons[1].activeSelf || shipWeapons[2].activeSelf))
        {
            shipWeapons[0].SetActive(false);
            shipWeapons[1].SetActive(true);
            shipWeapons[2].SetActive(false);
        }
        if (shipWeapon == ShipWeapon.ROTARY && (shipWeapons[0].activeSelf || shipWeapons[1].activeSelf || !shipWeapons[2].activeSelf))
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

            if (isKaiju)
            {
                GameObject.Find("Ocean").GetComponent<OceanRenderer>().Viewpoint = playerCameras[0].transform;
            }
            else
            {
                GameObject.Find("Ocean").GetComponent<OceanRenderer>().Viewpoint = playerCameras[1].transform;
            }

            if (GameObject.Find("Starting Camera"))
            {
                GameObject.Find("Starting Camera").SetActive(false);
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
        if (isLocalPlayer)
        {
            if (isKaiju)
            {
                CmdUpdatePosition(playerObjects[0].transform.position);
                CmdUpdateDesiredRotation(kaijuCore.GetDesiredRotation());
                CmdUpdateKaijuRotation(kaijuCore.GetRotation());
                CmdUpdateDesiredMovement(kaijuCore.GetDesiredMovement());
                CmdUpdateKaijuMovement(kaijuCore.GetMovement());
                CmdUpdateSubmerge(kaijuCore.GetSubmerged());
            }
            else
            {
                CmdUpdatePosition(playerObjects[1].transform.position);
                CmdUpdateRudder(shipCore.GetRudder());
                CmdUpdateActualRudder(shipCore.GetActualRudder());
                CmdUpdateShipRotation(shipCore.GetRotation());
                CmdUpdateThrottle(shipCore.GetThrottle());
                CmdUpdateShipSpeed(shipCore.GetSpeed());
                CmdUpdateAimPoint(shipTurret.GetAimPoint(), shipTurret.GetTargetVelocity());
            }
        }
        else
        {
            if (isKaiju)
            {
                playerObjects[0].transform.position = playerPosition;
                kaijuCore.SetDesiredRotation(playerDesiredRotation);
                kaijuCore.HardSetRotation(playerKaijuRotation);
                kaijuCore.SetDesiredMovement(playerDesiredMovement);
                kaijuCore.HardSetMovement(playerKaijuMovement);
                kaijuCore.HardSetSubmerged(playerSubmerged);
            }
            else
            {
                playerObjects[1].transform.position = playerPosition;
                shipCore.SetRudder(playerRudder);
                shipCore.HardSetRudder(playerActualRudder);
                shipCore.HardSetRotation(playerShipRotation);
                shipCore.SetThrottle(playerThrottle);
                shipCore.HardSetSpeed(playerShipSpeed);
                shipTurret.UpdateAim(playerAimPoint, playerTargetVelocity);

                if (playerShotsFired > shotsFired)
                {
                    shotsFired = playerShotsFired;
                    shipTurret.HardFireCannon(lastShotMask, lastShotPosition, lastShotRotation, lastShotVelocity);
                    Debug.Log("Fired Clientside");
                }
            }
        }
    }

    [Command(channel = 1)]
    private void CmdUpdatePosition(Vector3 pos)
    {
        playerPosition = pos;
    }

    [Command(channel = 1)]
    private void CmdUpdateDesiredRotation(float desiredRotation)
    {
        playerDesiredRotation = desiredRotation;
    }

    [Command(channel = 1)]
    private void CmdUpdateKaijuRotation(float rotation)
    {
        playerKaijuRotation = rotation;
    }

    [Command(channel = 1)]
    private void CmdUpdateDesiredMovement(Vector3 desiredMovement)
    {
        playerDesiredMovement = desiredMovement;
    }

    [Command(channel = 1)]
    private void CmdUpdateKaijuMovement(Vector3 movement)
    {
        playerKaijuMovement = movement;
    }

    [Command(channel = 1)]
    public void CmdUpdateSubmerge(float submerged)
    {
        playerSubmerged = submerged;
    }

    [Command]
    public void CmdTriggerSlash()
    {
        if (!isLocalPlayer)
        {
            kaijuCore.HardTriggerSlash();
        }
    }

    [Command(channel = 1)]
    private void CmdUpdateRudder(float rudder)
    {
        playerRudder = rudder;
    }

    [Command(channel = 1)]
    private void CmdUpdateActualRudder(float actualRudder)
    {
        playerActualRudder = actualRudder;
    }

    [Command(channel = 1)]
    private void CmdUpdateShipRotation(float rotation)
    {
        playerShipRotation = rotation;
    }

    [Command(channel = 1)]
    private void CmdUpdateThrottle(float throttle)
    {
        playerThrottle = throttle;
    }

    [Command(channel = 1)]
    private void CmdUpdateShipSpeed(float speed)
    {
        playerShipSpeed = speed;
    }

    [Command(channel = 1)]
    private void CmdUpdateAimPoint(Vector3 aimPoint, Vector3 targetVelocity)
    {
        playerAimPoint = aimPoint;
        playerTargetVelocity = targetVelocity;
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
    }

    public void SetShipWeapon(ShipWeapon selection)
    {
        shipWeapon = selection;
    }
}

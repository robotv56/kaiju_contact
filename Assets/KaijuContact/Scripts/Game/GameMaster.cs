using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using static GlobalVars;

public class GameMaster : NetworkBehaviour {

    private GameObject[] players = new GameObject[32];
    private bool[] activePlayers = new bool[32];
    private bool[] playerIsInMatch = new bool[32];
    private bool[] serverKaijuOptIn = new bool[32];
    private bool kaijuOptIn = true;
    private string[] playerNames = new string[32];
    private byte localPlayer = 255;
    private byte kaijuOptInCount = 0;
    private byte playerCount = 0;
    [SyncVar] bool gameInProgress = false;
    [SyncVar] GameStates gameState = GameStates.PRE_LOBBY;
    [SyncVar] Winners winner = Winners.UNDETERMINED;
    private float matchTimer;
    private bool starting;
    [SerializeField] private GameObject[] spawns = new GameObject[8];

    //UI refs
    private GameObject startButton;
    private GameObject kaijuOptInUI;
    private GameObject menu;
    private MenuController menuController;

    private void Start()
    {
        gameMaster = this;
        GlobalVars.globalGameObjects.TryGetValue("ready_button", out startButton);
        GlobalVars.globalGameObjects.TryGetValue("kaiju_optin", out kaijuOptInUI);
        GlobalVars.globalGameObjects.TryGetValue("menu", out menu);
        menuController = menu.GetComponent<MenuController>();
        menuController.LobbyActive(true);
    }

    private void Update()
    {
        matchTimer = Mathf.Clamp(matchTimer - Time.deltaTime, 0f, matchTimer);

        // Set names of active players with null names
        for (int i = 0; i < 32; i++)
        {
            if (activePlayers[i] && playerNames[i] == null)
            {
                playerNames[i] = players[i].GetComponent<ClientMaster>().GetName();
            }
        }

        if (gameState == GameStates.PRE_LOBBY)
        {
            if (kaijuOptInUI.GetComponent<Toggle>().isOn != kaijuOptIn)
            {
                kaijuOptIn = kaijuOptInUI.GetComponent<Toggle>().isOn;
            }
        }

        if (isServer)
        {
            if (gameState == GameStates.PRE_LOBBY)
            {
                if (!startButton.activeSelf && playerCount > 1 && !starting)
                {
                    startButton.SetActive(true);
                }
                else if (startButton.activeSelf && (playerCount < 2 || starting))
                {
                    startButton.SetActive(false);
                }
                if (starting && matchTimer == 0f)    
                {
                    StartMatch();
                    starting = false;
                }
            }

            if (gameState == GameStates.IN_PROGRESS)    
            {
                if (winner == Winners.UNDETERMINED)
                {
                    // Inefficient, could be done as a part of death but not fixing this now.
                    if (FindCurrentKaiju() == null || FindCurrentKaiju().GetComponent<ClientMaster>().GetHealth() <= 0f)
                    {
                        //Debug.Log("Kaiju couldn't be found: " + (FindCurrentKaiju() == null) + " Kaiju has no health: " + (FindCurrentKaiju().GetComponent<ClientMaster>().GetHealth() <= 0f));
                        Debug.Log("Arctic Defense Fleet Wins!");
                        winner = Winners.ADF_WINS;
                        matchTimer = 5f;
                    }

                    bool atLeastOneAlive = false;
                    for (int i = 0; i < playerCount; i++)
                    {
                        // Inefficient, could be done as a part of death but not fixing this now.
                        if (activePlayers[i] && playerIsInMatch[i] && !players[i].GetComponent<ClientMaster>().GetIsKaiju() && players[i].GetComponent<ClientMaster>().GetHealth() > 0f)
                        {
                            atLeastOneAlive = true;
                            break;
                        }
                    }
                    if (!atLeastOneAlive)
                    {
                        Debug.Log("Kaiju Wins!");
                        winner = Winners.KAIJU_WINS;
                        matchTimer = 5f;
                    }
                }
                else if (matchTimer == 0f)
                {
                    EndMatch();
                    matchTimer = 10f;
                }
            }

            if (gameState == GameStates.POST_LOBBY)
            {
                if (matchTimer == 0f)
                {
                    ResetMatch();
                }
            }
        }
    }

    public void StartCountdown()
    {
        if (matchTimer == 0f && playerCount > 1 && gameState == GameStates.PRE_LOBBY)
        {
            matchTimer = 5f;
            starting = true;
            startButton.SetActive(false);
        }
    }

    private void StartMatch()
    {
        if (isServer && gameState == GameStates.PRE_LOBBY)
        {
            gameState = GameStates.IN_PROGRESS;
            RpcTransmitGameState(gameState);
            winner = Winners.UNDETERMINED;

            // Disable Pre-Lobby UI
            menuController.PlayHasStarted();
            menuController.LobbyActive(false);

            RpcStartMatch();

            UpdateOptIn();
            Debug.Log(
                "OptIn List: " + 
                serverKaijuOptIn[0] + ", " +
                serverKaijuOptIn[1] + ", " +
                serverKaijuOptIn[2] + ", " +
                serverKaijuOptIn[3] + ", " +
                serverKaijuOptIn[4] + ", " +
                serverKaijuOptIn[5] + ", " +
                serverKaijuOptIn[6] + ", " +
                serverKaijuOptIn[7]
            );
            int kaijuNumber = Random.Range(0, kaijuOptInCount);
            Debug.Log("OptIn Random Selection: " + kaijuNumber + " Out of a possible " + kaijuOptInCount);
            byte n = kaijuOptInCount;
            byte k = 0;
            byte s = 1;
            for (int i = 0; i < n; i++)
            {
                if (activePlayers[i])
                {
                    if ((serverKaijuOptIn[i] || kaijuOptInCount == playerCount))
                    {
                        if (k == kaijuNumber)
                        {
                            // Active player, wanted to be kaiju (or being forced), was randomly selected.
                            Debug.Log("Player " + i + " is now the Kaiju!");
                            players[i].GetComponent<ClientMaster>().TriggerSetKaiju(true, spawns[0].transform.position, spawns[0].transform.rotation.eulerAngles.y);
                        }
                        else
                        {
                            // Active player, wanted to be kaiju (or being forced), was not randomly selected.
                            Debug.Log("Player " + i + " wasn't able to be the Kaiju and is now a ship!");
                            players[i].GetComponent<ClientMaster>().TriggerSetKaiju(false, spawns[s].transform.position, spawns[s].transform.rotation.eulerAngles.y);
                            s++;
                        }
                        k++;
                    }
                    else
                    {
                        // Active player, Did not want to be kaiju.
                        n++;
                        Debug.Log("Player " + i + " didn't want to be the Kaiju and is now a ship!");
                        players[i].GetComponent<ClientMaster>().TriggerSetKaiju(false, spawns[s].transform.position, spawns[s].transform.rotation.eulerAngles.y);
                        s++;
                    }
                    playerIsInMatch[i] = true;
                }
                else
                {
                    // Not an active player.
                    n++;                                                                
                    playerIsInMatch[i] = false;
                }
            }
        }
    }

    private void EndMatch()
    {
        if (isServer && gameState == GameStates.IN_PROGRESS)
        {
            gameState = GameStates.POST_LOBBY;
            RpcTransmitGameState(gameState);

            // Enable Post-Lobby UI
            menuController.WinScreenShow(winner);

            RpcEndMatch();

            for (int i = 0; i < 32; i++)
            {
                if (activePlayers[i] && playerIsInMatch[i])
                {
                    players[i].GetComponent<ClientMaster>().TriggerEndMatch();
                }
            }
        }
    }

    private void ResetMatch()
    {
        if (isServer && gameState == GameStates.POST_LOBBY)
        {
            gameState = GameStates.PRE_LOBBY;
            RpcTransmitGameState(gameState);
            winner = Winners.UNDETERMINED;

            // Disable Post-Lobby UI
            menuController.WinScreenHide();
            // Enable Pre-Lobby UI
            menuController.LobbyActive(true);

            RpcResetMatch();

            for (int i = 0; i < 32; i++)
            {
                if (activePlayers[i] && playerIsInMatch[i])
                {
                    players[i].GetComponent<ClientMaster>().TriggerResetMatch();
                }
            }

            if (players[localPlayer].GetComponent<ClientMaster>().GetIcebergMaster())
            {
                players[localPlayer].GetComponent<ClientMaster>().GetIcebergMaster().GetComponent<IcebergMaster>().ResetMatch();
            }
        }
    }
    
    private void UpdateOptIn()
    {
        if (isServer)
        {
            byte n = playerCount;
            kaijuOptInCount = 0;
            for (int i = 0; i < n; i++)
            {
                serverKaijuOptIn[i] = false;
                if (activePlayers[i])
                {
                    if (players[i].GetComponent<ClientMaster>().GetKaijuOptIn())
                    {
                        kaijuOptInCount++;
                        serverKaijuOptIn[i] = true;
                    }
                }
                else
                {
                    n++;
                }
            }
            if (kaijuOptInCount == 0) // If nobody wants to be kaiju, someone will be forced against their will to play the big awesome kaiju and they will just have to deal with it.
            {
                kaijuOptInCount = playerCount;
            }
        }
    }
    
    public byte AddPlayer(GameObject playerObject)
    {
        if (playerCount < 8)
        {
            for (byte i = 0; i < 32; i++)
            {
                if (!activePlayers[i])
                {
                    playerCount++;
                    activePlayers[i] = true;
                    players[i] = playerObject;
                    if (playerObject.GetComponent<ClientMaster>().isLocalPlayer)
                    {
                        localPlayer = i;
                        KaijuOptIn(true);
                        Debug.Log("GameMaster: Local Player Added: " + i + " There are now " + playerCount + " players.");
                    }
                    else
                    {
                        Debug.Log("GameMaster: Client Player Added: " + i + " There are now " + playerCount + " players.");
                    }
                    return i;
                }
            }
        }
        return 255;
    }

    public void RemovePlayer(byte player)
    {
        if (activePlayers[player])
        {
            playerCount--;
            activePlayers[player] = false;
            players[player] = null;
            playerNames[player] = null;
        }
    }

    public void UpdatePlayerNameList()
    {
        string list = "Players:\n";
        for (int i = 0; i < 8; i++)
        {
            list += playerNames[i];
            if (i == localPlayer)
            {
                list += " (you)";
            }
            list += "\n";
        }
        menuController.SetPlayerList(list);
    }

    public GameObject FindCurrentKaiju()
    {
        for (int i =  0; i < 32; i++)
        {
            if (activePlayers[i] && players[i].GetComponent<ClientMaster>().GetIsKaiju())
            {
                //Debug.Log("Found kaiju player: " + i);
                return players[i];
            }
        }
        Debug.Log("Unable to find kaiju");
        return null;
    }

    public GameObject FindLocalPlayer()
    {
        for (int i = 0; i < 32; i++)
        {
            if (activePlayers[i] && players[i].GetComponent<ClientMaster>().isLocalPlayer)
            {
                return players[i];
            }
        }
        return null;
    }

    public void KaijuOptIn(bool choice)
    {
        if (localPlayer != 255)
        {
            Debug.Log("Changed Kaiju OptIn to: " + choice);
            kaijuOptIn = choice;
            players[localPlayer].GetComponent<ClientMaster>().CmdUpdateKaijuOptIn(choice);
        }
    }

    public bool IsGameInProgress()
    {
        return gameState == GameStates.IN_PROGRESS;
    }

    [ClientRpc]
    private void RpcTransmitGameState(GameStates state)
    {
        gameState = state;
    }

    [ClientRpc]
    private void RpcStartMatch()
    {
        menuController.PlayHasStarted();
        menuController.LobbyActive(false);
    }

    [ClientRpc]
    private void RpcEndMatch()
    {
        menuController.WinScreenShow(winner);
    }

    [ClientRpc]
    private void RpcResetMatch()
    {
        menuController.WinScreenHide();
        menuController.LobbyActive(true);
    }
}

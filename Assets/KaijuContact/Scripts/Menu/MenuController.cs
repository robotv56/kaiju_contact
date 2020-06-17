﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalVars;

public enum MenuState
{
    MAIN,
    HELP,
    LOBBY,
    PLAYING,
    WINSCREEN
}

public class MenuController : MonoBehaviour
{
    MenuState state = MenuState.MAIN;
    MenuState lastState = MenuState.MAIN;
    GameObject mainMenu;
    GameObject background;
    GameObject lobby;
    GameObject playerList;
    GameObject helpScreen;
    GameObject hud;
    GameObject adfLogo;
    GameObject kaijuLogo;
    GameObject winScreen;
    GameObject wintextObject;
    TMPro.TextMeshProUGUI wintext;

    // Start is called before the first frame update
    void Start()
    {
        GlobalVars.globalGameObjects.TryGetValue("main_menu", out mainMenu);
        GlobalVars.globalGameObjects.TryGetValue("background", out background);
        GlobalVars.globalGameObjects.TryGetValue("lobby", out lobby);
        GlobalVars.globalGameObjects.TryGetValue("player_list", out playerList);
        GlobalVars.globalGameObjects.TryGetValue("help_screen", out helpScreen);
        GlobalVars.globalGameObjects.TryGetValue("hud", out hud);
        GlobalVars.globalGameObjects.TryGetValue("win_screen", out winScreen);
        GlobalVars.globalGameObjects.TryGetValue("adf_logo", out adfLogo);
        GlobalVars.globalGameObjects.TryGetValue("kaiju_logo", out kaijuLogo);
        GlobalVars.globalGameObjects.TryGetValue("win_text", out wintextObject);
        wintext = wintextObject.GetComponent<TMPro.TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        // Help Menu Open/Close
        /*if (state != MenuState.HELP && Input.GetKeyDown(KeyCode.F1))
        {
            OnHelpButtonPressed();
        }
        else */if(state == MenuState.HELP && (Input.GetKeyDown(KeyCode.Escape)/* || Input.GetKeyDown(KeyCode.F1)*/))
        {
            state = lastState;
            helpScreen.SetActive(false);
            switch (lastState)
            {
                case MenuState.LOBBY:
                    lobby.SetActive(true);
                    break;
                case MenuState.MAIN:
                    helpScreen.SetActive(true);
                    break;
            }
        }

        if ((state != MenuState.PLAYING) != background.activeSelf)
        {
            background.SetActive(state != MenuState.PLAYING);
        }
    }

    public void OnPlayButtonPressed()//activates lobby gui, NOT network lobby
    {
        state = MenuState.LOBBY;
        mainMenu.SetActive(false);
        manager.StartMatchMaker();
    }

    public void OnHelpButtonPressed()
    {
        lastState = state;
        state = MenuState.HELP;
        helpScreen.SetActive(true);
        switch (lastState)
        {
            case MenuState.MAIN:
                mainMenu.SetActive(false);
                break;
            case MenuState.LOBBY:
                lobby.SetActive(false);
                break;
        }
    }

    public void OnQuitButtonPressed()
    {
        if(Application.isEditor)
        {
            //UnityEditor.EditorApplication.ExitPlaymode();
        }
        else
        {
            Application.Quit();
        }
    }

    public void OnStartButtonPressed()
    {
        gameMaster.StartCountdown();
    }

    public void LobbyActive(bool active)
    {
        if (active && !lobby.activeSelf)
        {
            state = MenuState.LOBBY;
        }
        lobby.SetActive(active);
        if (active && hud.activeSelf)
        {
            hud.SetActive(false);
        }
    }

    public void WinScreenShow(Winners winner)
    {
        state = MenuState.WINSCREEN;
        winScreen.SetActive(true);
        adfLogo.SetActive(winner == Winners.ADF_WINS);
        kaijuLogo.SetActive(winner != Winners.ADF_WINS);
        if (winner == Winners.ADF_WINS)
        {
            wintext.text = "ADF Victory";
        }
        else
        {
            wintext.text = "Kaiju Victory";
        }
        if (hud.activeSelf)
        {
            hud.SetActive(false);
        }
    }

    public void WinScreenHide()
    {
        winScreen.SetActive(false);
        adfLogo.SetActive(false);
        kaijuLogo.SetActive(false);
    }

    public void PlayHasStarted()
    {
        state = MenuState.PLAYING;
        hud.SetActive(true);
    }

    public MenuState GetState()
    {
        return state;
    }

    public void SetPlayerList(string list)
    {
        playerList.GetComponent<TMPro.TextMeshProUGUI>().text = list;
    }
}
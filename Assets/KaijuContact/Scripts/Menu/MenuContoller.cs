using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MenuState
{
    MAIN,
    HELP,
    LOBBY,
    PLAYING
}

public class MenuContoller : MonoBehaviour
{
    MenuState state = MenuState.MAIN;
    GameObject mainMenu;
    GameObject lobby;
    GameObject helpScreen;

    // Start is called before the first frame update
    void Start()
    {
        GlobalVars.globalGameObjects.TryGetValue("main_menu", out mainMenu);
        GlobalVars.globalGameObjects.TryGetValue("lobby", out lobby);
        GlobalVars.globalGameObjects.TryGetValue("help_screen", out helpScreen);
    }

    // Update is called once per frame
    void Update()
    {
        if(state != MenuState.MAIN && Input.GetKeyDown(KeyCode.Escape))
        {
            state = MenuState.MAIN;

            mainMenu.SetActive(true);

            switch (state)
            {
                case MenuState.LOBBY:
                    {
                        lobby.SetActive(false);
                    }break;
                case MenuState.HELP:
                    {
                        helpScreen.SetActive(false);
                    }
                    break;
            }

        }
    }

    public void OnPlayButtonPressed()//activates lobby gui, NOT network lobby
    {
        state = MenuState.LOBBY;
        mainMenu.SetActive(false);
        lobby.SetActive(true);
    }

    public void OnHelpButtonPressed()
    {
        state = MenuState.HELP;
        mainMenu.SetActive(false);
        helpScreen.SetActive(true);
    }

    public void OnQuitButtonPressed()
    {
        if(Application.isEditor)
        {
            UnityEditor.EditorApplication.ExitPlaymode();
        }
        else
        {
            Application.Quit();
        }
    }

    public void OnStartButtonPressed()
    {
        //networking stuff
    }

    public void OpenLobby()
    {
        //enable start button
    }

    public void CloseLobby()
    {
        //disable start button
    }
}

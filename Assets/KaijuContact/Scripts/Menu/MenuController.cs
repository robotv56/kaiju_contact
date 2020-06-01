using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine.UI;

public enum MenuState
{
    MAIN,
    JOIN,
    HOST,
    CONFIG
}

public class MenuController : MonoBehaviour
{
    public TimelineAsset[] animations;
    //0 = open main menu
    //1 = close main menu
    //2 = open join menu
    //3 = close join menu

    public PlayableDirector[] directors;
    //0 =  main menu
    //1 = join menu

    public MenuState state = MenuState.MAIN;//state menu is in

    public Image[] playerReadyStatusIndicators;

    private MenuState oldState;//store old state for 

    private bool isDirty = false;
    private bool isWorking = false;

    private void Awake()
    {
        GlobalVars.menuController = this;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && state != MenuState.MAIN)//hack to make an "event" for key presses. unity pls
        {
            OnEscapeKeyPressed();
        }

        if(isDirty && !isWorking)
        {
            isDirty = false;
            StartCoroutine("coroutine");
        }
        else if(isDirty)
        {
            isDirty = false;
            Debug.Log("Attempted to move while animating");
        }


        //Gray=Not connected
        //Red=Notready
        //Green=Ready
        if(state == MenuState.JOIN || state == MenuState.HOST)
        {
            for(int i = 0; i != 6; i++)
            {
                if(GlobalVars.players[i])
                {
                    playerReadyStatusIndicators[i].color = Color.green;
                }
                else
                {
                    playerReadyStatusIndicators[i].color = Color.red;
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            for(int i = 0; i != 6; i++)
            {
                GlobalVars.players[i] = true;
            }
        }
    }

    //coroutine
    IEnumerator coroutine()
    {
        isWorking = true;

        //animate old window closing
        switch (oldState)
        {
            //TODO
            case MenuState.MAIN:
                {
                    directors[0].Play(animations[1]);//play main menu close animation
                    yield return new WaitForSeconds(1.1f);
                    directors[0].gameObject.SetActive(false);
                }
                break;

            case MenuState.JOIN:
                {
                    directors[1].Play(animations[3]);//play join menu close animation
                    yield return new WaitForSeconds(1.1f);
                    directors[1].gameObject.SetActive(false);
                }
                break;
        }
        
        //animate new window opening
        switch (state)
        {
            //TODO
            case MenuState.MAIN:
                {
                    directors[0].gameObject.SetActive(true);
                    directors[0].Play(animations[0]);//play main menu open animation
                }
                break;

            case MenuState.JOIN:
                {
                    directors[1].gameObject.SetActive(true);
                    directors[1].Play(animations[2]);//play join menu open animation
                }
                break;
        }
        
        isWorking = false;
        yield return null;
    }

    //events for main menu
    public void OnJoinButtonPressed()
    {
        oldState = state;
        state = MenuState.JOIN;
        isDirty = true;
    }

    public void OnHostButtonPressed()
    {
        oldState = state;
        state = MenuState.HOST;
        isDirty = true;
    }

    public void OnEscapeKeyPressed()
    {
        oldState = state;
        state = MenuState.MAIN;
        isDirty = true;
    }

    //TODO write lobby manager

}

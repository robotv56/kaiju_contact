using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

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

    public MenuState state = MenuState.MAIN;

    private MenuState oldState;

    private bool isDirty = false;
    private bool isWorking = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscapeKeyPressed();
        }

        if(isDirty && !isWorking)
        {
            isDirty = false;
            StartCoroutine("");
        }
        else if(isDirty)
        {
            isDirty = false;
            Debug.Log("Attempted to move while animating");
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
        }
        
        //animate new window opening
        switch (state)
        {
            //TODO
        }
        
        isWorking = false;
        yield return null;
    }

    //events
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
}

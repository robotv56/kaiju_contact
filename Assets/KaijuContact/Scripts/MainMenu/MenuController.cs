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

    public PlayableDirector[] directors;
    //0 =  main menu

    
}

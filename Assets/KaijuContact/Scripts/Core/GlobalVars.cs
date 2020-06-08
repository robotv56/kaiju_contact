using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public static class GlobalVars
{
    public enum PlayerStates
    {
        NOT_READY,
        CONNECTING,
        READY
    }

    public static bool[] players = new bool[6];
    
    public static string joinIP;

    public static bool uiActive = true;//may be used for updating UI and HUD code

    public enum Weapons
    {
        RAILGUN,
        MINIGUN,
        LASER
    }

    public static Weapons LocalPlayerWeapon = Weapons.RAILGUN;

    public static MenuController menuController;//main menu controller

    //populated by component isntances of AutoRegister.cs
    public static Dictionary<string, GameObject> globalGameObjects = new Dictionary<string, GameObject>();

    public static double goldenRatio = 1.61803398874989484820458683436;
}

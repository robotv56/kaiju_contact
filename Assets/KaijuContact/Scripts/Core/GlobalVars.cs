using UnityEngine;
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
}

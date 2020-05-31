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

    public static bool uiActive = true;//used for updating UI and HUD code
}

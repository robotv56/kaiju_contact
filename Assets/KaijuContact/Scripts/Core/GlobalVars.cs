using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
public static class GlobalVars
{
    public enum PlayerInput
    {
        KEYBOARD,
        XBOX_P1,
        XBOX_P2,
        XBOX_P3,
        XBOX_P4
    }

    public enum GameStates
    {
        PRE_LOBBY,
        IN_PROGRESS,
        POST_LOBBY
    }

    public enum Winners
    {
        KAIJU_WINS,
        ADF_WINS,
        UNDETERMINED
    }

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

    public static GameMaster gameMaster; // The master of games!

    public static NetworkManager manager; // The UNET default network manager we are using

    // Mystery Science Theater 3000, The many names of David Ryder.
    public static string[] namePool = 
    {
        "Slab_Bulkhead",
        "Bridge_Largemeats",
        "Punt_Speedchunk",
        "Butch_Deadlift",
        "Bold_Bigflank",
        "Splint_Chesthair",
        "Flint_Ironstag",
        "Bolt_Vanderhuge",
        "Thick_McRunfast",
        "Blast_Hardcheese",
        "Buff_Drinklots",
        "Trunk_Slamchest",
        "Fist_Rockbone",
        "Stump_Beefnob",
        "Smash_Lampjaw",
        "Punch_Rockgroin",
        "Buck_Plankchest",
        "Stump_Chunkmen",
        "Dirk_Hardpec",
        "Rip_Steakface",
        "Slate_Slabrock",
        "Crude_Bonemeal",
        "Ripped_Slagcheek",
        "Punch_Sideiron",
        "Gristle_McThornbody",
        "Slate_Fistcrunch",
        "Buff_Hardpack",
        "Bob_Johnson",
        "Blast_Thickneck",
        "Crunch_Buttsteak",
        "Slam_Squatthrust",
        "Mump_Beefbroth",
        "Touch_Rustrod",
        "Reef_Blastbody",
        "Big_McLargehuge",
        "Smoke_Manmuscle",
        "Eat_Punchbeef",
        "Hack_Blowfist",
        "Roll_Fizzlebeef"
    };
}

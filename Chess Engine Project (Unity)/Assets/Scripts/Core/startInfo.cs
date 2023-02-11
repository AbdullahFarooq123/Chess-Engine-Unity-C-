using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "START INFO")]
public class startInfo : MonoBehaviour{
    public static bool whiteIsBot {
        get;
        set;
    }
    public bool wIsBot;
    public static bool blackIsBot {
        get;
        set;
    }
    public bool bIsBot;
    public static int whiteDifficulty {
        get;
        set;
    }
    public int wDifficulty;
    public static int blackDifficulty {
        get;
        set;
    }
    public int bDifficulty;
    public static bool isUnlimitedTime {
        get;
        set;
    }
    public bool UnlimitedTime;
    public static int startSeconds {
        get;
        set;
    }
    public int sSeconds;
    public static int bonusSeconds {
        get;
        set;
    }
    public int bSeconds;

}

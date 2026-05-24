using UnityEngine;

public class FlagManager : MonoBehaviour
{
    public string[] flagsToResetOnStart = new string[] {
        "nameCard",
        "plague",
        "poster"
    };
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static FlagManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start()
    {
        // Reset specified flags on start
        foreach (string flag in flagsToResetOnStart)
        {
            SetFlag(flag, false);
        }
    }

    public bool GetFlag(string flag)
    {
        return PlayerPrefs.GetInt(flag, 0) == 1;
    }

    public void SetFlag(string flag, bool value)
    {
        PlayerPrefs.SetInt(flag, value ? 1 : 0);
        PlayerPrefs.Save();
    }
}

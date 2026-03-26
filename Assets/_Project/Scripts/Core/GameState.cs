using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance { get; private set; }

    public bool HasKey { get; private set; }
    public bool HasGroundKey{get; private set;}
    public bool HasScrewdriver { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetHasKey(bool value)
    {
        HasKey = value;
    }

    public void SetHasScrewdriver(bool value)
    {
        HasScrewdriver = value;
    }

    public void ResetRun()
    {
        HasKey = false;
        HasScrewdriver = false;
    }
}

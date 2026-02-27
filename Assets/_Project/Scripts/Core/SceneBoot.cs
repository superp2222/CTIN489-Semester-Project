using UnityEngine;

public class SceneBoot : MonoBehaviour
{
    public bool resetKeyOnLoad = true;

    void Start()
    {
        if (resetKeyOnLoad && GameState.Instance != null)
        {
            GameState.Instance.ResetRun();
        }
    }
}

using UnityEngine;

public class CutsceneControl : MonoBehaviour
{
    [Header("Disable these during cutscene")]
    public MonoBehaviour[] disableOnStart;

    [Header("Enable these after cutscene")]
    public MonoBehaviour[] enableOnEnd;

    [Header("Cameras")]
    public GameObject introCam;
    public GameObject cameraRig;

    void Start()
    {
        // disable gameplay controls at cutscene start
        if (disableOnStart != null)
            foreach (var c in disableOnStart)
                if (c != null) c.enabled = false;

        
    }

    public void EnableGameplay()
    {
        // enable gameplay scripts
        if (enableOnEnd != null)
            foreach (var c in enableOnEnd)
                if (c != null) c.enabled = true;

        // switch cameras
        if (introCam != null) introCam.SetActive(false);
        if (cameraRig != null) cameraRig.SetActive(true);
    }
}

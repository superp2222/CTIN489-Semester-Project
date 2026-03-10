using UnityEngine;

public class Lightswitch : MonoBehaviour
{
    public enum Mode
    {
        TurnOn,
        TurnOff
    }

    [Header("Lights Container")]
    [Tooltip("Parent object containing all lights (e.g., your 'Lights' GameObject).")]
    public GameObject lightsParent;

    [Header("Settings")]
    public Mode mode = Mode.TurnOn;

    [Tooltip("Intensity used when lights are on.")]
    public float onIntensity = 500f;

    private Light[] lights;

    void Awake()
    {
        if (lightsParent != null)
            lights = lightsParent.GetComponentsInChildren<Light>(true);
    }

    public void Activate()
    {
        if (lights == null || lights.Length == 0) return;

        float targetIntensity = (mode == Mode.TurnOn) ? onIntensity : 0f;

        foreach (Light l in lights)
        {
            if (l != null)
                l.intensity = targetIntensity;
        }
    }
}
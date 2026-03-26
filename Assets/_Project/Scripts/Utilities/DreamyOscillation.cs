using UnityEngine;

/// <summary>
/// Gently moves the object up and down over time,
/// creating a slow, floaty "dreamy" oscillation.
/// </summary>
public class DreamyOscillation : MonoBehaviour
{
    [Header("Motion")]
    [Tooltip("Maximum vertical offset from the starting position.")]
    public float amplitude = 0.25f;

    [Tooltip("Time (in seconds) to complete one full up-and-down cycle.")]
    public float period = 3.5f;

    [Tooltip("Randomize phase so multiple objects don't move in perfect sync.")]
    public bool randomizePhase = true;

    private Vector3 _startLocalPos;
    private float _phaseOffset;

    void Awake()
    {
        _startLocalPos = transform.localPosition;

        if (randomizePhase)
            _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void OnEnable()
    {
        // Reset baseline when re-enabled, so it feels stable.
        _startLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (period <= 0.0001f)
            return;

        float t = (Time.time + _phaseOffset) * (Mathf.PI * 2f / period);
        float offsetY = Mathf.Sin(t) * amplitude;

        Vector3 pos = _startLocalPos;
        pos.y += offsetY;
        transform.localPosition = pos;
    }
}


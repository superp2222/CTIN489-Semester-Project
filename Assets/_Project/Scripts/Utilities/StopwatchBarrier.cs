using TelemetryManager;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Soft barrier that logs the elapsed local stopwatch time when an allowed user crosses it.
/// After logging, it resets the shared local stopwatch and destroys itself.
/// </summary>
[RequireComponent(typeof(Collider))]
public class StopwatchBarrier : MonoBehaviour
{
    [Header("Allowed Users")]
    [Tooltip("If this list is empty, anyone can trigger it. Otherwise only listed root objects may trigger.")]
    [SerializeField] private GameObject[] allowedUsers;

    [Header("Barrier ID")]
    [Tooltip("Optional ID for this barrier; defaults to the GameObject name.")]
    [SerializeField] private string barrierId;

    private static SampleMetric<string> s_barrierIdMetric;
    private static SampleMetric<float> s_lapDurationMetric;
    private static SampleMetric<float> s_globalTimeMetric;
    private static bool s_metricsInitialized;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        EnsureMetrics();
    }

    private static void EnsureMetrics()
    {
        if (s_metricsInitialized)
            return;

        s_barrierIdMetric = TelemetryManager.TelemetryManager.CreateSampleMetric<string>("StopwatchBarrierId");
        s_lapDurationMetric = TelemetryManager.TelemetryManager.CreateSampleMetric<float>("StopwatchBarrierLapSeconds");
        s_globalTimeMetric = TelemetryManager.TelemetryManager.CreateSampleMetric<float>("StopwatchBarrierGlobalSeconds");

        s_metricsInitialized = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        EnsureMetrics();

        GameObject root = other.transform.root.gameObject;
        if (!IsAllowed(root))
            return;

        string sceneName = SceneTimeTelemetry.IsTimingInitialized
            ? SceneTimeTelemetry.CurrentSceneName
            : SceneManager.GetActiveScene().name;

        float lapDuration = SceneTimeTelemetry.IsTimingInitialized
            ? SceneTimeTelemetry.GetStopwatchElapsed()
            : 0f;

        float globalElapsed = SceneTimeTelemetry.IsTimingInitialized
            ? SceneTimeTelemetry.GetGlobalElapsed()
            : 0f;

        string idToLog = string.IsNullOrWhiteSpace(barrierId) ? gameObject.name : barrierId;
        string combinedId = string.IsNullOrWhiteSpace(sceneName)
            ? idToLog
            : $"{sceneName}:{idToLog}";

        s_barrierIdMetric?.AddSample(combinedId);
        s_lapDurationMetric?.AddSample(lapDuration);
        s_globalTimeMetric?.AddSample(globalElapsed);

        SceneTimeTelemetry.ResetStopwatch();

        Destroy(gameObject);
    }

    private bool IsAllowed(GameObject root)
    {
        if (allowedUsers == null || allowedUsers.Length == 0)
            return true;

        foreach (GameObject allowed in allowedUsers)
        {
            if (allowed != null && root == allowed)
                return true;
        }

        return false;
    }
}
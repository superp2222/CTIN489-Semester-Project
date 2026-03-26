using TelemetryManager;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks two timers for the active scene:
/// 1) Global scene timer: from scene start until scene unload / quit.
/// 2) Local stopwatch timer: from scene start or last barrier reset.
///
/// The global timer is logged once per scene.
/// The local stopwatch is read/reset by StopwatchBarrier instances.
/// </summary>
public class SceneTimeTelemetry : MonoBehaviour
{
    private static SceneTimeTelemetry _instance;

    // Shared scene timing so other systems can access it.
    public static float GlobalSceneStartTime { get; private set; }
    public static float StopwatchStartTime { get; private set; }
    public static string CurrentSceneName { get; private set; }
    public static bool IsTimingInitialized { get; private set; }

    public static float GetGlobalElapsed()
    {
        return IsTimingInitialized ? Mathf.Max(0f, Time.time - GlobalSceneStartTime) : 0f;
    }

    public static float GetStopwatchElapsed()
    {
        return IsTimingInitialized ? Mathf.Max(0f, Time.time - StopwatchStartTime) : 0f;
    }

    public static void ResetStopwatch()
    {
        if (IsTimingInitialized)
            StopwatchStartTime = Time.time;
    }

    private SampleMetric<string> _sceneNameMetric;
    private SampleMetric<float> _sceneDurationMetric;

    private string _currentSceneName;
    private float _sceneEnterTime;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        _sceneNameMetric = TelemetryManager.TelemetryManager.CreateSampleMetric<string>("SceneName");
        _sceneDurationMetric = TelemetryManager.TelemetryManager.CreateSampleMetric<float>("SceneDurationSeconds");

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        Application.quitting += OnApplicationQuitting;

        InitializeTimingForScene(SceneManager.GetActiveScene());
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        // Log the scene we are leaving.
        LogCurrentSceneDuration();

        // Start fresh timers for the new scene.
        InitializeTimingForScene(newScene);
    }

    private void OnApplicationQuitting()
    {
        // Log final scene before TelemetryManager writes out metrics.
        LogCurrentSceneDuration();
    }

    private void OnDestroy()
    {
        if (_instance != this)
            return;

        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        Application.quitting -= OnApplicationQuitting;

        if (_instance == this)
            _instance = null;
    }

    private void InitializeTimingForScene(Scene scene)
    {
        _currentSceneName = scene.name;
        _sceneEnterTime = Time.time;

        GlobalSceneStartTime = _sceneEnterTime;
        StopwatchStartTime = _sceneEnterTime;
        CurrentSceneName = _currentSceneName;
        IsTimingInitialized = true;
    }

    private void LogCurrentSceneDuration()
    {
        if (!IsTimingInitialized || string.IsNullOrEmpty(_currentSceneName))
            return;

        float duration = Mathf.Max(0f, Time.time - _sceneEnterTime);

        _sceneNameMetric?.AddSample(_currentSceneName);
        _sceneDurationMetric?.AddSample(duration);
    }
}
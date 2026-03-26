using UnityEngine;

namespace TelemetryManager
{
    public class TelemetryTester : MonoBehaviour
    {
        private SampleMetric<float> _floatMetric;
        private SampleMetric<Vector3 > _vector3Metric;
        private CounterMetric _counterMetric;
        public void Start()
        {
            _floatMetric = TelemetryManager.CreateSampleMetric<float>("random floats generated");
            _vector3Metric = TelemetryManager.CreateSampleMetric<Vector3>("vector3s generated");
            _counterMetric = TelemetryManager.CreateCounterMetric("spacebar press count");
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("Recording telemetry for spacebar");
                _floatMetric.AddSample(Random.Range(0f,1f));
                _vector3Metric.AddSample(Vector3.up);
                _counterMetric.Accumulate(1);
                
            }
        }
    }
}

    using System;
    using System.Collections.Generic;
    using System.IO;
    using UnityEngine;
    
    
#if UNITY_EDITOR
    using UnityEditor;
#endif

    namespace TelemetryManager
    {
        public class TelemetryManager : MonoBehaviour
        {
            public string pathToCsvFile => @Application.persistentDataPath;
            public string filename => pathToCsvFile + "/" +  DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff")+ "_data.csv";
            private static readonly List<Metric> Metrics = new();
            public static SampleMetric<T> CreateSampleMetric<T>(string name)
            {
                var metric = new SampleMetric<T>() { Name = name };
                Metrics.Add(metric);
                return metric;
            }

            public static CounterMetric CreateCounterMetric(string name)
            {
                var metric = new CounterMetric() { Name = name };
                Metrics.Add(metric);
                return metric;
            }

            public void OnApplicationQuit()
            {
                if (pathToCsvFile == string.Empty) Debug.LogError("Path to CSV file is empty");
                Debug.Log($"Writing telemetry to CSV file {filename}");

                StreamWriter writer = new StreamWriter(filename);
                foreach (var metric in Metrics)
                {
                    foreach (var s in metric.GetOutput())
                    {
                        writer.WriteLine(s);
                    }
                }
                writer.Flush();
                writer.Close();
              
            }
            
#if UNITY_EDITOR
            [ContextMenu("Open Telemetry Folder")]
            private void OpenTelemetryFolder()
            {
                string path = Application.persistentDataPath;
                if (Directory.Exists(path))
                    EditorUtility.RevealInFinder(path);
                else
                    Debug.LogWarning($"Telemetry folder does not exist: {path}");
            }
#endif
            
        }
    }

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    namespace TelemetryManager
    {
        public class SampleMetric<T> : Metric
        {
            private readonly List<Sample> _samples = new();
            private static readonly HashSet<Type> VectorTypes = new() 
            { 
                typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Vector2Int), typeof(Vector3Int) 
            };
            public void AddSample(T value)
            {
                _samples.Add(new Sample(value));
            }

            public override List<string> GetOutput()
            {
                List<string> values = new() { Name};
                foreach (var s in _samples)
                    values.Add(s.ToString());
                return values;
            }
            
            static float[] GetComponents(T value)
            {
                return value switch
                {
                    Vector2 v => new[] { v.x, v.y },
                    Vector3 v => new[] { v.x, v.y, v.z },
                    Vector4 v => new[] { v.x, v.y, v.z, v.w },
                    Vector2Int v => new[] { (float)v.x, (float)v.y },
                    Vector3Int v => new[] { (float)v.x, (float)v.y, (float)v.z },
                    _ => throw new ArgumentException($"Unsupported vector type: {typeof(T)}")
                };
            }

            class Sample
            {
                public readonly T Value;
                public readonly float Time;
                public override string ToString()
                {
                    if (VectorTypes.Contains(typeof(T)))
                    {
                        float[] components = GetComponents(Value);
                        return $"{Time}, {string.Join(", ", components)}";
                    }
                    
                    return Time + ", " + Value;
                }

                public Sample(T value)
                {
                    Value = value;
                    Time = UnityEngine.Time.realtimeSinceStartup;
                }
            }
        }
        
       
    }
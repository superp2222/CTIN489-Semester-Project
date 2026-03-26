using System.Collections.Generic;

namespace TelemetryManager
{
    public abstract class Metric
    {
        public string Name;
        public abstract List<string> GetOutput();

    }
}
using System.Collections.Generic;

namespace TelemetryManager
{
    public class CounterMetric : Metric
    {
        private int counter;

        public void Accumulate(int amount)
        {
            counter += amount;
        }


        public override List<string> GetOutput()
        {
            return new List<string>() { Name, counter.ToString() };
        }
    }
}
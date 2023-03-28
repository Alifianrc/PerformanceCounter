using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace PerformanceReader
{
    public class PerformanceReader
    {
        private PerformanceCounter m_cpuCounter;
        private PerformanceCounter m_ramCounter;

        private Thread m_counterThread;
        private const int CountDelay = 1000;
        private bool m_isRunning;

        private double m_averageCpu;
        private double m_averageRam;

        public const string FileName = "Analytics.json";

        string m_sectionId = "id3";

        public PerformanceReader()
        {
            m_cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            m_ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            m_isRunning = false;
        }

        public void Start()
        {
            m_counterThread = new Thread(() => Counting());
            m_counterThread.Start();

            m_isRunning = true;

            // Count cpu usage
            m_averageCpu = m_cpuCounter.NextValue();
            m_averageRam = m_ramCounter.NextValue();
        }

        private void Counting()
        {
            while (m_isRunning)
            {
                // Get sample of cpu and ram usage every 1 second
                Thread.Sleep(CountDelay);
                Console.Clear();

                // Count cpu usage
                var resultCpu = m_cpuCounter.NextValue();
                Console.WriteLine("CPU : " + resultCpu.ToString("f2") + "%");
                m_averageCpu = (m_averageCpu + resultCpu) / 2;

                // Count ram usage
                var resultRam = m_ramCounter.NextValue();
                Console.WriteLine("RAM : " + resultRam.ToString("f2") + "%");
                m_averageRam = (m_averageRam + resultRam) / 2;
            }
        }

        public void Stop()
        {
            m_isRunning = false;
            Thread.Sleep(CountDelay);
            SaveToFile();
        }

        private void SaveToFile()
        {
            JObject savedData = null;

            if (File.Exists(FileName))
            {
                var loadedData = File.ReadAllText(FileName);
                try
                {
                    savedData = JObject.Parse(loadedData);
                }
                catch(Exception e)
                {
                    savedData = new JObject();
                }
            }
            else
            {
                var fs = File.Create(FileName);
                fs.Close();

                savedData = new JObject();
            }

            var currentTime = DateTimeOffset.Now;

            var jData = new JObject(
                new JProperty("Average CPU %", m_averageCpu.ToString("f2")),
                new JProperty("Average RAM %", m_averageRam.ToString("f2")),
                new JProperty("Date", currentTime.ToUnixTimeSeconds())
            );

            var jProp = new JProperty(m_sectionId + "#" + currentTime.ToUnixTimeSeconds().ToString(), jData);

            savedData.Add(jProp);

            File.WriteAllText(FileName, savedData.ToString());
        }
    }
}

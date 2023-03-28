using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Management;

namespace PerformanceReader
{
    public class PerformanceReader
    {
        private ManagementObjectSearcher m_cpuSearcher;
        private ManagementObjectSearcher m_ramSearcher;

        private Thread m_counterThread;
        private const int CountDelay = 1000;

        private double m_averageCpu;
        private double m_averageRam;

        public PerformanceReader()
        {
            m_cpuSearcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
            m_ramSearcher = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
        }

        public void Start()
        {
            m_counterThread = new Thread(() => Counting());
            m_counterThread.Start();

            // Count cpu usage
            int cpuCount = 0;
            double totalPercent = 0;
            foreach (ManagementObject obj in m_cpuSearcher.Get())
            {
                cpuCount++;
                totalPercent += double.Parse((obj["PercentProcessorTime"]).ToString());
            }

            m_averageCpu = (totalPercent / cpuCount);

            // Count ram usage
            var memoryValues = m_ramSearcher.Get().Cast<ManagementObject>().Select(mo => new {
                FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
            }).FirstOrDefault();

            if (memoryValues != null)
            {
                m_averageRam = ((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100;
            }
        }

        private void Counting()
        {
            while (true)
            {
                // Get sample of cpu and ram usage every 1 second
                Thread.Sleep(CountDelay);
                Console.Clear();

                // Count cpu usage
                int cpuCount = 0;
                double totalPercent = 0;
                foreach (ManagementObject obj in m_cpuSearcher.Get())
                {
                    cpuCount++;
                    totalPercent += double.Parse((obj["PercentProcessorTime"]).ToString());
                }

                var resultCpu = (totalPercent / cpuCount);
                Console.WriteLine("CPU : " + resultCpu.ToString("f2") + "%");
                m_averageCpu = (m_averageCpu + resultCpu) / 2;

                // Count ram usage
                var memoryValues = m_ramSearcher.Get().Cast<ManagementObject>().Select(mo => new {
                    FreePhysicalMemory = Double.Parse(mo["FreePhysicalMemory"].ToString()),
                    TotalVisibleMemorySize = Double.Parse(mo["TotalVisibleMemorySize"].ToString())
                }).FirstOrDefault();

                if (memoryValues != null)
                {
                    var resultRam  = (((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / memoryValues.TotalVisibleMemorySize) * 100);
                    Console.WriteLine("RAM : " + resultRam.ToString("f2") + "%");
                    m_averageRam = (m_averageRam + resultRam) / 2;
                }
            }
        }

        public void Stop()
        {
            m_counterThread.Abort();

            // Save to file
        }

        private void SaveToFile()
        {

        }
    }
}

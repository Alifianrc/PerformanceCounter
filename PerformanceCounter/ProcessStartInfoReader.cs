﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceReader
{
    public class ProcessStartInfoReader
    {
        ProcessStartInfo m_startInfo;

        public ProcessStartInfoReader()
        {
            m_startInfo = new ProcessStartInfo();
            m_startInfo.FileName = "sar";
            m_startInfo.Arguments = "-u 1 1"; // retrieve CPU usage for 1 second, once
            m_startInfo.RedirectStandardOutput = true;
            m_startInfo.UseShellExecute = false;
        }

        public void Start()
        {
            using (Process process = new Process())
            {
                process.StartInfo = m_startInfo;
                process.Start();

                // read the output of the sar command
                string output = process.StandardOutput.ReadToEnd();

                // extract the CPU usage from the output
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                string[] fields = lines[3].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                float cpuUsage = float.Parse(fields[2]);

                Console.WriteLine("CPU Usage: {0}%", cpuUsage);
            }

            m_startInfo.FileName = "sar";
            m_startInfo.Arguments = "-r 1 1"; // retrieve memory usage for 1 second, once

            using (Process process = new Process())
            {
                process.StartInfo = m_startInfo;
                process.Start();

                // read the output of the sar command
                string output = process.StandardOutput.ReadToEnd();

                // extract the memory usage from the output
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                string[] fields = lines[3].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                float memUsage = float.Parse(fields[4]);

                Console.WriteLine("Memory Usage: {0}%", memUsage);
            }
        }

        public void Stop()
        {

        }
    }
}

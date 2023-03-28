using System;

namespace PerformanceReader // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var alpha = new PerformanceReader();
            alpha.Start();
        }
    }
}
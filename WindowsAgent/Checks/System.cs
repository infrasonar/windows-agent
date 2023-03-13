using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WindowsAgent.Checks
{

    internal class System : Check
    {
        private const int DefaultCheckInterval = 300;  // Interval in seconds
        private const string _name = "system";        

        public override string Name() { return _name; }

        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            CheckResult data = new CheckResult();
            CheckType time = new CheckType("time");

            time.Set("uptime", Stopwatch.GetTimestamp() / Stopwatch.Frequency);
            data.Add(time);

            return data;
        }
    }
}

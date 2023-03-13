using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WindowsAgent.Checks
{
    using CheckType = Dictionary<string, object>;
    using CheckResult = Dictionary<string, Dictionary<string, object>>;

    internal class System : Check
    {
        private const string _name = "system";        

        public override string Name() { return _name; }

        public override bool CanRun() { return true; }

        public override CheckResult Run()
        {
            CheckResult data = new CheckResult();
            CheckType time = new CheckType();

            time["time"] = Stopwatch.GetTimestamp() / Stopwatch.Frequency;


            return data["time"] = ;
        }
    }
}

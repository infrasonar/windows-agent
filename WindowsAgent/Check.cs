using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent
{
    using CheckResult = Dictionary<string, Dictionary<string, object>>;

    internal abstract class Check
    {
        public abstract string Name();
        public abstract bool CanRun();
        public abstract CheckResult Run();
    }

    internal class Result
    {

    }
}

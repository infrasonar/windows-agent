using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;

namespace WindowsAgent
{
    internal abstract class Check
    {
        private Timer _timer;

        private const int DefaultCheckInterval = 300;  // Interval in seconds

        public Check()
        {
            int interval = Config.GetCheckInterval(this.Name(), DefaultCheckInterval);

            _timer = new Timer(intervalInMinutes);
            string name = this.Name();
        }

        public abstract string Name();
        public abstract bool CanRun();
        public abstract CheckResult Run();
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent
{
    public enum EventId
    {
        TokenFound = 2000,
        TokenNotFound = 2001,
    }

    internal class Logger
    {
        public static void Write(string message, EventLogEntryType type, EventId eventId)
        {
            using (EventLog eventLog = new EventLog("Application"))
            {
                eventLog.Source = "InfraSonarAgent";
                eventLog.WriteEntry(message, type, (int)eventId);
            }
        }
    }
}

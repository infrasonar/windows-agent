using System.Diagnostics;

namespace WindowsAgent
{
    public enum EventId
    {
        TokenFound = 2000,
        TokenNotFound = 2001,
        InvalidInterval = 2002,
        CustomInterval = 2003,
        CheckDisabled = 2004,
        UploadFailed = 2005,
        WriteIntervalFailed = 2006,
        InitRegistry = 2007,
        AssetIdRegistry = 2008,
        AssetNameRegistry = 2009,
        AssetNameNotFound = 2010,
        AssignCollectorFailed = 2011,
        AssignKindFailed = 2011,
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

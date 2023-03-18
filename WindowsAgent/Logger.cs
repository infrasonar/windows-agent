using System.Diagnostics;

namespace WindowsAgent
{
    public enum EventId
    {
        None = 0,
        TokenNotFound = 2001,
        InvalidInterval = 2002,
        UploadFailed = 2005,
        WriteIntervalFailed = 2006,
        InitRegistry = 2007,
        AssetIdRegistry = 2008,
        AssetNameRegistry = 2009,
        AssetNameNotFound = 2010,
        AssignCollectorFailed = 2011,
        AssignKindFailed = 2011,
        CheckError = 2013,
        CreateAsset = 2016,
        AnnounceAsset = 2017,
    }

    internal class Logger
    {
        public static void Write(string message, EventLogEntryType type, EventId eventId)
        {
            using (var eventLog = new EventLog("Application"))
            {
                eventLog.Source = "InfraSonarAgent";
                eventLog.WriteEntry(message, type, (int)eventId);
            }
        }
    }
}

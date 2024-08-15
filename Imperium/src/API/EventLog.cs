using Imperium.Core.EventLogging;

namespace Imperium.API;

public static class EventLog
{
    /// <summary>
    /// Logs a message to the event log.
    /// </summary>
    /// <param name="message"></param>
    public static void Log(EventLogMessage message)
    {
        APIHelpers.AssertImperiumReady();

        Imperium.EventLog.AddLog(message);
    }
}
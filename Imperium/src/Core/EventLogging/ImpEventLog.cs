using System.Collections.Generic;
using Imperium.Util;
using Imperium.Util.Binding;

namespace Imperium.Core.EventLogging;

public class ImpEventLog
{
    public readonly ImpBinding<List<EventLogMessage>> Log = new([]);

    internal readonly EntityEventLogger EntityEvents;
    internal readonly PlayerEventLogger PlayerEvents;
    internal readonly GameEventLogger GameEvents;

    private EventLogMessage latestLog;

    internal ImpEventLog()
    {
        EntityEvents = new EntityEventLogger(this);
        PlayerEvents = new PlayerEventLogger(this);
        GameEvents = new GameEventLogger(this);
    }

    public void AddLog(EventLogMessage log)
    {
        if (latestLog.ObjectName == log.ObjectName && latestLog.Message == log.Message)
        {
            latestLog.Count++;
            Log.Refresh();
            return;
        }

        log.Time = Formatting.FormatDayTime(Imperium.TimeOfDay.currentDayTime);
        log.Day = Imperium.StartOfRound.gameStats.daysSpent;

        latestLog = log;
        Log.Value.Add(log);
        Log.Refresh();
    }
}
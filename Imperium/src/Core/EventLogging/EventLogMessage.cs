namespace Imperium.Core.EventLogging;

public struct EventLogMessage
{
    public string Time { get; set; }
    public string ObjectName { get; init; }
    public string Message { get; init; }
    public EventLogType Type { get; init; }

    public string DetailsTitle { get; init; }
    public EventLogDetail[] Details { get; init; }

    internal int Count { get; set; }
    internal int Day { get; set; }
}

public readonly struct EventLogDetail
{
    public string Title { get; init; }
    public string Text { get; init; }
}

public enum EventLogType
{
    Entity,
    Player,
    Game,
    Custom
}
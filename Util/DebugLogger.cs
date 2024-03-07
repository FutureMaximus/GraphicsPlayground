using GraphicsPlayground.Util;

namespace GraphicsPlayground.Util;

public static class DebugLogger
{
    /// <summary> The log messages containing colored strings. </summary>
    public static readonly Dictionary<int, List<ColorHelper.ColoredString>> LogMessages = new();
    /// <summary> Maximum amount of messages before the oldest message is removed. </summary>
    public static readonly int MaximumMessages = 100;

    public static void Log(string message)
    {
        if (LogMessages.Count >= MaximumMessages)
        {
            LogMessages.Remove(LogMessages.Keys.First());
        }
        List<ColorHelper.ColoredString> coloredMessages = ColorHelper.ConvertTextToColoredStrings(message);
        int count = LogMessages.Count;
        if (LogMessages.ContainsKey(count))
        {
            count += 1;
        }
        LogMessages.Add(count, coloredMessages);
    }
}


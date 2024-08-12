namespace Imperium.Util;

public static class RichText
{
    public static string Strikethrough(string value) => $"<s>{value}</s>";
    public static string Underlined(string value) => $"<u>{value}</u>";
    public static string Bold(string value) => $"<b>{value}</b>";
    public static string Italic(string value) => $"<i>{value}</i>";
    public static string Size(string value, int size) => $"<size={size}>{value}</size>";
}
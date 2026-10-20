namespace Rune.Controls;

/// <summary>A recent-document card for the homepage list.</summary>
public sealed class RecentCard
{
    public string Path { get; }
    public string DisplayName { get; }
    public string LastOpenedText { get; }

    public RecentCard(string path, string displayName, DateTime lastOpenedUtc)
    {
        Path = path;
        DisplayName = displayName;
        LastOpenedText = lastOpenedUtc == default
            ? "Recently opened"
            : $"Last opened {lastOpenedUtc.ToLocalTime():yyyy/M/d HH:mm}";
    }

}

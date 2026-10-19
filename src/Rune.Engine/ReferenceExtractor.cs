using System.Text.RegularExpressions;

namespace Rune.Engine;

public sealed record ReferencePreview(string Text, string? Uri, string? Doi);

/// <summary>Extracts one bibliography entry around a PDF destination.</summary>
public static partial class ReferenceExtractor
{
    private const int MaxEntryLength = 1400;

    public static ReferencePreview? Extract(PageText page, double? targetX, double? targetY)
    {
        if (string.IsNullOrWhiteSpace(page.Text))
        {
            return null;
        }

        int anchor = FindAnchor(page, targetX, targetY);
        string text = page.Text;
        int start = FindEntryStart(text, anchor);
        int end = FindEntryEnd(text, start);
        if (end <= start)
        {
            return null;
        }

        string entry = Cleanup(text[start..Math.Min(end, start + MaxEntryLength)]);
        if (entry.Length < 8)
        {
            return null;
        }

        string? doi = DoiRegex().Match(entry) is { Success: true } doiMatch
            ? doiMatch.Groups[1].Value.TrimEnd('.', ',', ';', ')', ']')
            : null;
        string? uri = UrlRegex().Match(entry) is { Success: true } urlMatch
            ? urlMatch.Value.TrimEnd('.', ',', ';', ')', ']')
            : doi is null ? null : $"https://doi.org/{doi}";
        return new ReferencePreview(entry, uri, doi);
    }

    private static int FindAnchor(PageText page, double? targetX, double? targetY)
    {
        if (targetY is null)
        {
            return 0;
        }

        int best = 0;
        double bestScore = double.MaxValue;
        for (int i = 0; i < page.CharBoxes.Count; i++)
        {
            var box = page.CharBoxes[i];
            if (box.Width <= 0 || box.Height <= 0) continue;
            double dy = targetY.Value < box.Y ? box.Y - targetY.Value
                : targetY.Value > box.Y + box.Height ? targetY.Value - box.Y - box.Height : 0;
            double dx = targetX is null ? 0 : targetX.Value < box.X ? box.X - targetX.Value
                : targetX.Value > box.X + box.Width ? targetX.Value - box.X - box.Width : 0;
            double score = dy * 4 + dx;
            if (score < bestScore)
            {
                bestScore = score;
                best = i;
            }
        }
        return best;
    }

    private static int FindEntryStart(string text, int anchor)
    {
        int floor = Math.Max(0, anchor - 700);
        int result = -1;
        for (Match match = EntryMarkerRegex().Match(text, floor);
             match.Success && match.Index <= anchor;
             match = match.NextMatch())
        {
            result = match.Index;
        }
        if (result >= 0) return result;

        int paragraph = text.LastIndexOf("\n\n", Math.Clamp(anchor, 0, text.Length - 1), StringComparison.Ordinal);
        return paragraph >= 0 ? paragraph + 2 : Math.Max(0, text.LastIndexOf('\n', Math.Clamp(anchor, 0, text.Length - 1)) + 1);
    }

    private static int FindEntryEnd(string text, int start)
    {
        int searchAt = Math.Min(text.Length, start + 1);
        Match next = EntryMarkerRegex().Match(text, searchAt);
        int markerEnd = next.Success ? next.Index : text.Length;
        int paragraph = text.IndexOf("\n\n", searchAt, StringComparison.Ordinal);
        int paragraphEnd = paragraph >= 0 ? paragraph : text.Length;
        return Math.Min(markerEnd, paragraphEnd);
    }

    private static string Cleanup(string value)
    {
        value = HyphenatedBreakRegex().Replace(value, string.Empty);
        value = WhitespaceRegex().Replace(value, " ");
        return value.Trim();
    }

    [GeneratedRegex(@"(?m)^\s*(?:\[\s*\d{1,4}\s*\]|\d{1,4}[.)])\s+")]
    private static partial Regex EntryMarkerRegex();

    [GeneratedRegex(@"-\s*\r?\n\s*")]
    private static partial Regex HyphenatedBreakRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"https?://[^\s<>]+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    [GeneratedRegex(@"(?:https?://(?:dx\.)?doi\.org/|doi\s*:\s*)?(10\.\d{4,9}/[-._;()/:A-Z0-9]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DoiRegex();
}

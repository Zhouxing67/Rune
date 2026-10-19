using System.Text.RegularExpressions;

namespace Rune.Engine;

public sealed record ReferencePreview(string Text, string? Uri, string? Doi);

/// <summary>
/// Extracts one bibliography entry around an internal-link target. Geometry is
/// authoritative: the target maps to a physical line, then markers, vertical
/// gaps and hanging indents delimit the entry.
/// </summary>
public static partial class ReferenceExtractor
{
    private const int MaxEntryLines = 18;

    public static ReferencePreview? Extract(PageText page, double? targetX, double? targetY)
    {
        // A page-only destination cannot identify one reference reliably.
        if (targetY is null || page.Lines.Count == 0) return null;

        int anchor = FindAnchorLine(page.Lines, targetX, targetY.Value);
        int start = FindEntryStart(page.Lines, anchor);
        int end = FindEntryEnd(page.Lines, start);
        if (end < start) return null;

        string entry = Cleanup(string.Join("\n", page.Lines.Skip(start).Take(end - start + 1).Select(line => line.Text)));
        if (entry.Length < 8) return null;

        string? doi = DoiRegex().Match(entry) is { Success: true } doiMatch
            ? doiMatch.Groups[1].Value.TrimEnd('.', ',', ';', ')', ']')
            : null;
        string? uri = UrlRegex().Match(entry) is { Success: true } urlMatch
            ? urlMatch.Value.TrimEnd('.', ',', ';', ')', ']')
            : doi is null ? null : $"https://doi.org/{doi}";
        return new ReferencePreview(entry, uri, doi);
    }

    private static int FindAnchorLine(IReadOnlyList<PageTextLine> lines, double? x, double y)
    {
        int best = 0;
        double bestScore = double.MaxValue;
        for (int i = 0; i < lines.Count; i++)
        {
            var box = lines[i].Bounds;
            double dy = y < box.Y ? box.Y - y : y > box.Y + box.Height ? y - box.Y - box.Height : 0;
            double dx = x is null ? 0 : x.Value < box.X ? box.X - x.Value
                : x.Value > box.X + box.Width ? x.Value - box.X - box.Width : 0;
            double score = dy * 12 + dx; // Y selects the row; X disambiguates columns.
            if (score < bestScore)
            {
                bestScore = score;
                best = i;
            }
        }
        return best;
    }

    private static int FindEntryStart(IReadOnlyList<PageTextLine> lines, int anchor)
    {
        if (IsEntryStart(lines[anchor].Text) || LooksLikeAuthorYear(lines[anchor].Text)) return anchor;

        double currentX = lines[anchor].Bounds.X;
        double medianHeight = MedianHeight(lines, anchor);
        for (int i = anchor - 1; i >= Math.Max(0, anchor - MaxEntryLines); i--)
        {
            if (IsEntryStart(lines[i].Text) || LooksLikeAuthorYear(lines[i].Text)) return i;
            if (IsLargeGap(lines[i], lines[i + 1], medianHeight)) return i + 1;
            if (lines[i].Bounds.X + IndentThreshold(lines[i]) < currentX) return i;
            currentX = Math.Min(currentX, lines[i].Bounds.X);
        }
        return anchor;
    }

    private static int FindEntryEnd(IReadOnlyList<PageTextLine> lines, int start)
    {
        double medianHeight = MedianHeight(lines, start);
        int limit = Math.Min(lines.Count - 1, start + MaxEntryLines - 1);
        for (int i = start + 1; i <= limit; i++)
        {
            if (IsEntryStart(lines[i].Text) || LooksLikeAuthorYear(lines[i].Text) ||
                IsLargeGap(lines[i - 1], lines[i], medianHeight)) return i - 1;
        }
        return limit;
    }

    private static bool IsLargeGap(PageTextLine before, PageTextLine after, double medianHeight)
    {
        double gap = after.Bounds.Y - (before.Bounds.Y + before.Bounds.Height);
        // Normal PDF line leading is commonly close to one glyph height. Only
        // treat substantially larger whitespace as a paragraph separator.
        return gap > Math.Max(6, medianHeight * 1.35);
    }

    private static double MedianHeight(IReadOnlyList<PageTextLine> lines, int around)
    {
        var heights = lines.Skip(Math.Max(0, around - 4)).Take(9)
            .Select(line => line.Bounds.Height).Where(height => height > 0).Order().ToArray();
        return heights.Length == 0 ? 10 : heights[heights.Length / 2];
    }

    private static double IndentThreshold(PageTextLine line) => Math.Max(5, line.Bounds.Height * 0.55);
    private static bool IsEntryStart(string text) => EntryMarkerRegex().IsMatch(text);
    private static bool LooksLikeAuthorYear(string text) => AuthorYearRegex().IsMatch(text);

    private static string Cleanup(string value)
    {
        value = HyphenatedBreakRegex().Replace(value, string.Empty);
        value = WhitespaceRegex().Replace(value, " ");
        return value.Trim();
    }

    [GeneratedRegex(@"^\s*(?:\[\s*\d{1,4}\s*\]|\d{1,4}[.)])\s+")]
    private static partial Regex EntryMarkerRegex();

    [GeneratedRegex(@"^[A-ZÀ-ÖØ-Þ][^\n]{0,180}?(?:\(\s*)?(?:18|19|20)\d{2}[a-z]?\)?[.,]")]
    private static partial Regex AuthorYearRegex();

    [GeneratedRegex(@"-\s*\r?\n\s*")]
    private static partial Regex HyphenatedBreakRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"https?://[^\s<>]+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    [GeneratedRegex(@"(?:https?://(?:dx\.)?doi\.org/|doi\s*:\s*)?(10\.\d{4,9}/[-._;()/:A-Z0-9]+)", RegexOptions.IgnoreCase)]
    private static partial Regex DoiRegex();
}

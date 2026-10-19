using Rune.Engine;

namespace Rune.Tests;

public class ReferenceExtractorTests
{
    [Fact]
    public void Extract_UsesDestinationToChooseReferenceAndFindsDoi()
    {
        const string text = "[1] First Author. First paper.\n[2] Second Author. Useful paper. doi:10.1234/example.42\n[3] Third Author. Third paper.";
        var boxes = BoxesForLines(text);
        var page = new PageText(0, text, boxes);

        var preview = ReferenceExtractor.Extract(page, targetX: 10, targetY: 30);

        Assert.NotNull(preview);
        Assert.StartsWith("[2] Second Author", preview.Text);
        Assert.DoesNotContain("[3]", preview.Text);
        Assert.Equal("10.1234/example.42", preview.Doi);
        Assert.Equal("https://doi.org/10.1234/example.42", preview.Uri);
    }

    [Fact]
    public void Extract_JoinsWrappedAndHyphenatedLines()
    {
        const string text = "1. Ada Lovelace. A refer-\nence split across lines.\n\nAppendix";
        var page = new PageText(0, text, BoxesForLines(text));

        var preview = ReferenceExtractor.Extract(page, 0, 10);

        Assert.NotNull(preview);
        Assert.Equal("1. Ada Lovelace. A reference split across lines.", preview.Text);
    }

    [Fact]
    public void Extract_HangingIndentStopsBeforeNextApaReference()
    {
        const string text = "Airports Council International, 2017. Passenger traffic 2017\n" +
                            "FINAL. https://example.test/traffic\n" +
                            "Authority, S.A., Corporate Social Responsibility Report 2016.\n" +
                            "Retrieved Jan 1, 2018.";
        var page = new PageText(0, text, BoxesForLines(text, [0, 18, 0, 18]));

        var preview = ReferenceExtractor.Extract(page, 0, 30); // continuation line of entry one

        Assert.NotNull(preview);
        Assert.StartsWith("Airports Council International", preview.Text);
        Assert.Contains("FINAL", preview.Text);
        Assert.DoesNotContain("Authority", preview.Text);
    }

    [Fact]
    public void Extract_ApaEntriesWithoutIndentStopsAtNextAuthorYearLine()
    {
        const string text = "Airports Council International, 2017. Passenger traffic.\n" +
                            "Retrieved Jan 1, 2018.\n" +
                            "Authority, S.A., 2016. Corporate Social Responsibility Report.\n" +
                            "Shanghai: Authority.";
        var page = new PageText(0, text, BoxesForLines(text));

        var preview = ReferenceExtractor.Extract(page, 4, 10);

        Assert.NotNull(preview);
        Assert.Contains("Passenger traffic", preview.Text);
        Assert.DoesNotContain("Authority, S.A.", preview.Text);
    }

    [Fact]
    public void Extract_WithoutPreciseTargetDoesNotGuess()
    {
        const string text = "[1] First reference.\n[2] Second reference.";
        var page = new PageText(0, text, BoxesForLines(text));

        Assert.Null(ReferenceExtractor.Extract(page, null, null));
    }

    private static TextRect[] BoxesForLines(string text, double[]? indents = null)
    {
        var boxes = new TextRect[text.Length];
        int line = 0;
        double x = indents?[0] ?? 0, y = 10;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                boxes[i] = default;
                line++;
                x = indents is not null && line < indents.Length ? indents[line] : 0;
                y += 20;
            }
            else
            {
                boxes[i] = new TextRect(x, y, 6, 10);
                x += 6;
            }
        }
        return boxes;
    }
}

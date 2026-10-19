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

    private static TextRect[] BoxesForLines(string text)
    {
        var boxes = new TextRect[text.Length];
        double x = 0, y = 10;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                boxes[i] = default;
                x = 0;
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

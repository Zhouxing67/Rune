using Rune.Engine;

namespace Rune.Controls;

public sealed record SearchResultItem(SearchHit Hit, string Snippet)
{
    public string PageLabel => $"Page {Hit.PageIndex + 1}";
}

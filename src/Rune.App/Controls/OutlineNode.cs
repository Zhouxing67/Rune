using Rune.Engine;

namespace Rune.Controls;

/// <summary>Bindable table-of-contents node for the outline TreeView.</summary>
public sealed class OutlineNode : System.ComponentModel.INotifyPropertyChanged
{
    public string Title { get; }
    public int PageIndex { get; }
    public IList<OutlineNode> Children { get; }
    public string PageLabel => PageIndex >= 0 ? (PageIndex + 1).ToString() : string.Empty;

    private bool _isCurrent;
    public bool IsCurrent
    {
        get => _isCurrent;
        set
        {
            if (_isCurrent == value) return;
            _isCurrent = value;
            PropertyChanged?.Invoke(this, new(nameof(IsCurrent)));
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    public OutlineNode(OutlineItem item)
    {
        Title = string.IsNullOrWhiteSpace(item.Title) ? "(untitled)" : item.Title;
        PageIndex = item.PageIndex;
        Children = [.. item.Children.Select(c => new OutlineNode(c))];
    }
}

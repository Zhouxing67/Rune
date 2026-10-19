using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Rune.Engine;
using Windows.UI;

namespace Rune.Controls;

public sealed partial class SearchOverview : UserControl
{
    private IReadOnlyList<SearchHit> _hits = [];
    private IReadOnlyList<(float Width, float Height)> _pageSizes = [];
    private SearchHit? _active;
    private List<(double Y, SearchHit Hit)> _markers = [];

    public event EventHandler<SearchHit>? HitInvoked;

    public SearchOverview()
    {
        InitializeComponent();
        SizeChanged += (_, _) => Rebuild();
    }

    public void SetResults(IReadOnlyList<SearchHit> hits, IReadOnlyList<(float Width, float Height)> pageSizes, SearchHit? active)
    {
        _hits = hits;
        _pageSizes = pageSizes;
        _active = active;
        Visibility = hits.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        Rebuild();
    }

    public void SetActive(SearchHit? active)
    {
        _active = active;
        Rebuild();
    }

    private void Rebuild()
    {
        MarkerCanvas.Children.Clear();
        _markers = [];
        if (_hits.Count == 0 || _pageSizes.Count == 0 || ActualHeight <= 0) return;

        foreach (var group in _hits.GroupBy(NormalizedPosition).OrderBy(g => g.Key))
        {
            // Merge marks that would occupy the same two-pixel band. The click
            // still resolves to the nearest concrete hit rather than a bucket.
            double y = Math.Clamp(group.Key * Math.Max(1, ActualHeight - 4), 0, Math.Max(0, ActualHeight - 4));
            var representative = group.First();
            bool active = group.Any(h => ReferenceEquals(h, _active) || h == _active);
            var mark = new Rectangle
            {
                Width = active ? 14 : 10,
                Height = active ? 4 : 2,
                RadiusX = 1,
                RadiusY = 1,
                Fill = new SolidColorBrush(active ? Color.FromArgb(255, 255, 153, 0) : Color.FromArgb(255, 255, 213, 0)),
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            Canvas.SetLeft(mark, active ? 2 : 6);
            Canvas.SetTop(mark, y);
            MarkerCanvas.Children.Add(mark);
            foreach (var hit in group) _markers.Add((y, hit));
        }
    }

    private double NormalizedPosition(SearchHit hit)
    {
        int page = Math.Clamp(hit.PageIndex, 0, _pageSizes.Count - 1);
        double within = hit.Rects.Count == 0 || _pageSizes[page].Height <= 0
            ? 0
            : Math.Clamp(hit.Rects[0].Y / _pageSizes[page].Height, 0, 1);
        // Round to a stable visual band to cap the number of rectangles in very
        // large result sets while preserving every hit in the click map.
        return Math.Round(((page + within) / _pageSizes.Count) * Math.Max(1, ActualHeight / 2))
            / Math.Max(1, ActualHeight / 2);
    }

    private void MarkerCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (_markers.Count == 0) return;
        double y = e.GetCurrentPoint(MarkerCanvas).Position.Y;
        SearchHit hit = _markers.MinBy(m => Math.Abs(m.Y - y)).Hit;
        HitInvoked?.Invoke(this, hit);
        e.Handled = true;
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Rune.Controls;

public enum SelectionToolbarAction { Highlight, Underline, Strikeout, Copy, Note }

public sealed class SelectionToolbarActionEventArgs : EventArgs
{
    public required SelectionToolbarAction Action { get; init; }
    public Color? Color { get; init; }
    public byte Alpha { get; init; } = 255;
}

public sealed partial class SelectionToolbar : UserControl
{
    private static readonly Color[] Palette =
    {
        Color.FromArgb(255, 255, 210, 0), Color.FromArgb(255, 255, 166, 0),
        Color.FromArgb(255, 80, 190, 120), Color.FromArgb(255, 38, 139, 210),
        Color.FromArgb(255, 191, 90, 242), Color.FromArgb(255, 220, 55, 70),
    };

    private Color _highlightColor = Palette[0];
    private Color _underlineColor = Palette[3];
    private Color _strikeoutColor = Palette[5];

    public event EventHandler<SelectionToolbarActionEventArgs>? ActionRequested;

    public SelectionToolbar()
    {
        InitializeComponent();
        UpdateIndicators();
    }

    private void Highlight_Click(object sender, RoutedEventArgs e) => RaiseMarkup(SelectionToolbarAction.Highlight, _highlightColor, 102);
    private void Underline_Click(object sender, RoutedEventArgs e) => RaiseMarkup(SelectionToolbarAction.Underline, _underlineColor, 255);
    private void Strikeout_Click(object sender, RoutedEventArgs e) => RaiseMarkup(SelectionToolbarAction.Strikeout, _strikeoutColor, 255);
    private void Copy_Click(object sender, RoutedEventArgs e) => Raise(SelectionToolbarAction.Copy);
    private void Note_Click(object sender, RoutedEventArgs e) => Raise(SelectionToolbarAction.Note);
    private void HighlightColour_Click(object sender, RoutedEventArgs e) => ShowPalette(HighlightColourButton, SelectionToolbarAction.Highlight);
    private void UnderlineColour_Click(object sender, RoutedEventArgs e) => ShowPalette(UnderlineColourButton, SelectionToolbarAction.Underline);
    private void StrikeoutColour_Click(object sender, RoutedEventArgs e) => ShowPalette(StrikeoutColourButton, SelectionToolbarAction.Strikeout);

    private void ShowPalette(FrameworkElement target, SelectionToolbarAction action)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
        foreach (Color color in Palette)
        {
            var swatch = new Button
            {
                Width = 28, Height = 28, Padding = new Thickness(0),
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(5), Tag = color,
                Content = new Border
                {
                    Width = 18, Height = 18, CornerRadius = new CornerRadius(9),
                    Background = new SolidColorBrush(color),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(55, 80, 80, 80)),
                    BorderThickness = new Thickness(1),
                },
            };
            ToolTipService.SetToolTip(swatch, ColourName(color));
            swatch.Click += (_, _) =>
            {
                SetColour(action, (Color)swatch.Tag);
                target.ContextFlyout?.Hide();
            };
            row.Children.Add(swatch);
        }

        var flyout = new Flyout
        {
            Placement = FlyoutPlacementMode.BottomEdgeAlignedLeft,
            Content = new Border { Padding = new Thickness(4), Child = row },
        };
        target.ContextFlyout = flyout;
        flyout.ShowAt(target);
    }

    private void SetColour(SelectionToolbarAction action, Color color)
    {
        switch (action)
        {
            case SelectionToolbarAction.Highlight: _highlightColor = color; break;
            case SelectionToolbarAction.Underline: _underlineColor = color; break;
            case SelectionToolbarAction.Strikeout: _strikeoutColor = color; break;
        }
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        HighlightColourIndicator.Background = new SolidColorBrush(_highlightColor);
        UnderlineColourIndicator.Background = new SolidColorBrush(_underlineColor);
        StrikeoutColourIndicator.Background = new SolidColorBrush(_strikeoutColor);
    }

    private static string ColourName(Color color) => color switch
    {
        { R: 255, G: 210 } => "Yellow", { R: 255, G: 166 } => "Orange",
        { G: 190 } => "Green", { B: 210 } => "Blue", { B: 242 } => "Purple", _ => "Red",
    };

    private void RaiseMarkup(SelectionToolbarAction action, Color color, byte alpha) =>
        ActionRequested?.Invoke(this, new SelectionToolbarActionEventArgs { Action = action, Color = color, Alpha = alpha });
    private void Raise(SelectionToolbarAction action) =>
        ActionRequested?.Invoke(this, new SelectionToolbarActionEventArgs { Action = action });
}

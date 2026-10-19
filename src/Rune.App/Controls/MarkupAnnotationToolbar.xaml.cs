using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Rune.Controls;

public sealed class MarkupAnnotationActionEventArgs : EventArgs
{
    public Color? Color { get; init; }
    public bool Delete { get; init; }
}

public sealed partial class MarkupAnnotationToolbar : UserControl
{
    private static readonly Color[] Palette =
    {
        Color.FromArgb(255, 255, 210, 0), Color.FromArgb(255, 255, 166, 0),
        Color.FromArgb(255, 80, 190, 120), Color.FromArgb(255, 38, 139, 210),
        Color.FromArgb(255, 191, 90, 242), Color.FromArgb(255, 220, 55, 70),
    };

    public event EventHandler<MarkupAnnotationActionEventArgs>? ActionRequested;

    public MarkupAnnotationToolbar()
    {
        InitializeComponent();
        int index = 0;
        foreach (Color color in Palette)
        {
            var button = new Button
            {
                Width = 30, Height = 30, Padding = new Thickness(0),
                Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
                BorderThickness = new Thickness(0), CornerRadius = new CornerRadius(4),
                Content = new Border
                {
                    Width = 17, Height = 17, CornerRadius = new CornerRadius(8.5),
                    Background = new SolidColorBrush(color), BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(55, 80, 80, 80)),
                },
            };
            button.Click += (_, _) => ActionRequested?.Invoke(this,
                new MarkupAnnotationActionEventArgs { Color = color });
            ColourRow.Children.Insert(index++, button);
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e) =>
        ActionRequested?.Invoke(this, new MarkupAnnotationActionEventArgs { Delete = true });
}

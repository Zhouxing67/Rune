using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Rune.Engine;

namespace Rune.Controls;

public sealed partial class CitationPreviewCard : UserControl
{
    private string? _uri;

    public event EventHandler? GoToRequested;
    public event EventHandler<string>? OpenLinkRequested;
    public event EventHandler? CloseRequested;

    public CitationPreviewCard() => InitializeComponent();

    public void ShowReference(ReferencePreview preview, int pageIndex)
    {
        ReferenceText.Text = preview.Text;
        PageLabel.Text = $"Page {pageIndex + 1}";
        _uri = preview.Uri;
        OpenLinkButton.Visibility = string.IsNullOrWhiteSpace(_uri)
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private void GoTo_Click(object sender, RoutedEventArgs e) => GoToRequested?.Invoke(this, EventArgs.Empty);
    private void OpenLink_Click(object sender, RoutedEventArgs e)
    {
        if (_uri is { Length: > 0 } uri) OpenLinkRequested?.Invoke(this, uri);
    }
    private void Close_Click(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);
}

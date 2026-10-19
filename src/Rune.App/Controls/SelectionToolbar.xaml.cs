using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Rune.Engine;

namespace Rune.Controls;

public enum SelectionToolbarAction { Highlight, Underline, Strikeout, Copy, Note }

public sealed partial class SelectionToolbar : UserControl
{
    public event EventHandler<SelectionToolbarAction>? ActionRequested;

    public SelectionToolbar() => InitializeComponent();

    private void Highlight_Click(object sender, RoutedEventArgs e) => Raise(SelectionToolbarAction.Highlight);
    private void Underline_Click(object sender, RoutedEventArgs e) => Raise(SelectionToolbarAction.Underline);
    private void Strikeout_Click(object sender, RoutedEventArgs e) => Raise(SelectionToolbarAction.Strikeout);
    private void Copy_Click(object sender, RoutedEventArgs e) => Raise(SelectionToolbarAction.Copy);
    private void Note_Click(object sender, RoutedEventArgs e) => Raise(SelectionToolbarAction.Note);
    private void Raise(SelectionToolbarAction action) => ActionRequested?.Invoke(this, action);
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace Rune.Controls;

public sealed partial class NoteCard : UserControl
{
    public event EventHandler<string>? SaveRequested;
    public event EventHandler? DeleteRequested;
    public event EventHandler? CloseRequested;

    public NoteCard() => InitializeComponent();

    public void ShowText(string text)
    {
        ReadText.Text = text;
        EditText.Text = text;
        SetEditing(false);
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        SetEditing(true);
        EditText.Focus(FocusState.Programmatic);
        EditText.Select(EditText.Text.Length, 0);
    }

    private void Save_Click(object sender, RoutedEventArgs e) => SaveRequested?.Invoke(this, EditText.Text);
    private void Delete_Click(object sender, RoutedEventArgs e) => DeleteRequested?.Invoke(this, EventArgs.Empty);
    private void Close_Click(object sender, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);
    private void Root_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Escape) return;
        CloseRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }

    private void SetEditing(bool editing)
    {
        ReadText.Visibility = editing ? Visibility.Collapsed : Visibility.Visible;
        EditText.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;
        EditButton.Visibility = editing ? Visibility.Collapsed : Visibility.Visible;
        SaveButton.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;
    }
}

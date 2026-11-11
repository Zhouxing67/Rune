using Microsoft.UI.Xaml.Controls;

namespace Rune.Services;

/// <summary>
/// Serializes every <see cref="ContentDialog"/> in the app.
///
/// WinUI allows only ONE ContentDialog per window at a time — a second
/// <c>ShowAsync()</c> throws (<c>"Only a single ContentDialog can be open at
/// any time."</c>). Before this gate existed that throw could escape an
/// <c>async void</c> handler and kill the process, which is exactly what
/// happened when a background update check collided with a user-initiated one.
///
/// Each XamlRoot has its own gate; different windows may show dialogs independently.
/// </summary>
public static class DialogHost
{
    private static readonly HashSet<Microsoft.UI.Xaml.XamlRoot> OpenRoots = [];

    /// <summary>
    /// Shows a dialog, or returns <see cref="ContentDialogResult.None"/> if one
    /// is already open. <c>None</c> is what the Close button returns, so every
    /// existing call site already treats it as the safe/cancel path.
    /// </summary>
    public static async Task<ContentDialogResult> ShowAsync(ContentDialog dialog)
    {
        if (dialog.XamlRoot is not { } root || !OpenRoots.Add(root))
        {
            ErrorLog.Default.Write("DialogHost", $"Suppressed a second dialog: {dialog.Title}");
            return ContentDialogResult.None;
        }

        try
        {
            return await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            // A dialog failing to display must never take the app down.
            ErrorLog.Default.Write("DialogHost", ex);
            return ContentDialogResult.None;
        }
        finally
        {
            OpenRoots.Remove(root);
        }
    }
}

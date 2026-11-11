using Microsoft.UI.Xaml;
using Rune.Services;

namespace Rune;

public partial class App : Application
{
    public static MainWindow? MainWindow => Workspace.LastActive;

    public App()
    {
        InitializeComponent();

        // Safety net. An `async void` handler (every WinUI event handler is one)
        // has no Task to park an exception in, so the runtime rethrows it on the
        // dispatcher and the process dies unless Handled is set here. Log it and
        // keep running — a reader losing an hour of annotations to a transient
        // failure is far worse than a stale error message.
        UnhandledException += (_, e) =>
        {
            ErrorLog.Default.Write("UnhandledException", e.Exception);
            e.Handled = true;
            ReportToUser(e.Exception.Message);
        };

        // The other half: `_ = SomeAsync()` parks a failure in a Task nobody
        // awaits, which is silently swallowed. This surfaces those too.
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            ErrorLog.Default.Write("UnobservedTaskException", e.Exception);
            e.SetObserved();
        };
    }

    /// <summary>
    /// Surfaces a background failure in the main window's InfoBar. Never a
    /// dialog: the very bug this net was added for was "a second ContentDialog
    /// was shown", so opening one from the handler could re-trigger it.
    /// </summary>
    private static void ReportToUser(string message)
    {
        try
        {
            (MainWindow as MainWindow)?.ReportBackgroundError(message);
        }
        catch
        {
            // Window may be closing or not built yet — the log already has it.
        }
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Workspace.Initialize();
        Program.StartDispatching(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
    }
}

using System.Collections.Concurrent;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

namespace Rune;

public static class Program
{
    private static readonly ConcurrentQueue<AppActivationArguments> Pending = new();
    private static DispatcherQueue? _dispatcher;
    private static bool _draining;

    [STAThread]
    public static void Main(string[] args)
    {
        WinRT.ComWrappersSupport.InitializeComWrappers();
        var current = AppInstance.GetCurrent();
        // Subscribe before registering: another process can redirect immediately.
        current.Activated += (_, activation) => Enqueue(activation);
        var initialActivation = current.GetActivatedEventArgs();
        var instance = AppInstance.FindOrRegisterForKey("Rune.Main");
        if (!instance.IsCurrent)
        {
            Task.Run(async () => await instance.RedirectActivationToAsync(initialActivation))
                .GetAwaiter().GetResult();
            return;
        }
        Pending.Enqueue(initialActivation);
        Application.Start(parameters =>
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherQueueSynchronizationContext(
                DispatcherQueue.GetForCurrentThread()));
            _ = new App();
        });
    }

    private static void Enqueue(AppActivationArguments args)
    {
        Pending.Enqueue(args);
        _dispatcher?.TryEnqueue(Drain);
    }

    internal static void StartDispatching(DispatcherQueue dispatcher)
    {
        _dispatcher = dispatcher;
        dispatcher.TryEnqueue(Drain);
    }

    private static async void Drain()
    {
        if (_draining) return;
        _draining = true;
        try
        {
            while (Pending.TryDequeue(out var args))
            {
                try { await Workspace.HandleActivationAsync(args); }
                catch (Exception ex) { App.MainWindow?.ReportBackgroundError(ex.Message); }
            }
        }
        finally { _draining = false; }
    }
}

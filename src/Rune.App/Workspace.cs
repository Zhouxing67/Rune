using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using Microsoft.Windows.AppLifecycle;
using Rune.Services;
using Windows.ApplicationModel.Activation;
using System.Runtime.InteropServices;
using Rune.Engine;

namespace Rune;

/// <summary>Application state and window routing. Access only on the UI thread.</summary>
internal static class Workspace
{
    private static readonly List<MainWindow> Windows = [];
    private static readonly AppStateStore Store = new(Environment.GetEnvironmentVariable("RUNE_STATE_DIR") is { Length: > 0 } directory ? directory : null);
    internal static AppState State { get; } = Store.Load();
    internal static MainWindow? LastActive => Windows.LastOrDefault();
    internal static event Action? Changed;

    private static bool _initializing;

    internal static void Initialize()
    {
        _initializing = true;
        var sessions = State.Settings.RestoreSession
            ? (State.Session.Windows.Count > 0 ? State.Session.Windows.ToList() : [State.Session])
            : [];
        foreach (var session in sessions) CreateWindow(session);
        if (Windows.Count == 0) CreateWindow();
        _initializing = false;
    }

    internal static MainWindow CreateWindow(SessionState? session = null)
    {
        var window = new MainWindow(session);
        Windows.Add(window);
        window.Activated += (_, e) =>
        {
            if (e.WindowActivationState == WindowActivationState.Deactivated) return;
            Windows.Remove(window);
            Windows.Add(window);
        };
        string icon = Path.Combine(AppContext.BaseDirectory, "Assets", "RunePdf.ico");
        if (File.Exists(icon)) window.AppWindow.SetIcon(icon);
        window.Activate();
        return window;
    }

    private static readonly HashSet<string> Writing = new(StringComparer.OrdinalIgnoreCase);
    internal static bool IsWriting(string path) => Writing.Any(p => DocumentIdentity.Same(p, path));
    internal static IDisposable ReserveOutput(string path)
    {
        if (FindOwner(path) is not null || IsWriting(path))
            throw new IOException("This PDF is already open or being saved. Choose another file.");
        path = Path.GetFullPath(path);
        Writing.Add(path);
        return new OutputLease(path);
    }
    private sealed class OutputLease(string path) : IDisposable
    {
        public void Dispose() => Writing.Remove(path);
    }

    internal static void Save()
    {
        if (!_initializing && Windows.Count > 0)
            State.Session = new SessionState { Windows = Windows.Select(w => w.GetSession()).ToList() };
        Store.Save(State);
        Changed?.Invoke();
    }

    internal static void Closing(MainWindow window)
    {
        // Explicitly closed windows disappear from the next session. The last
        // window is retained so exiting and reopening restores its documents.
        var remaining = Windows.Where(w => w != window).ToList();
        State.Session = new SessionState
        {
            Windows = (remaining.Count > 0 ? remaining : [window]).Select(w => w.GetSession()).ToList(),
        };
        Windows.Remove(window);
        Save();
    }

    internal static MainWindow? FindOwner(string path) => Windows.FirstOrDefault(w => w.ContainsDocument(path));

    internal static void Activate(MainWindow window)
    {
        if (window.AppWindow.Presenter is OverlappedPresenter { State: OverlappedPresenterState.Minimized } presenter)
            presenter.Restore();
        window.Activate();
    }

    internal static Window OwnerOf(FrameworkElement element) =>
        Windows.First(w => ReferenceEquals(w.Content.XamlRoot, element.XamlRoot));

    internal static async Task HandleActivationAsync(AppActivationArguments activation)
    {
        var target = LastActive ?? CreateWindow();
        Activate(target);
        if (activation.Data is IFileActivatedEventArgs files)
        {
            foreach (var file in files.Files) await target.LoadDocumentAsync(file.Path);
        }
        else if (activation.Data is ILaunchActivatedEventArgs launch)
        {
            var command = LaunchCommand.Parse(SplitCommandLine(launch.Arguments), Environment.ProcessPath);
            foreach (var path in command.Files)
                if (File.Exists(path))
                    await target.LoadDocumentAsync(path, command.Page, command.Zoom);
        }
    }

    private static string[] SplitCommandLine(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine)) return [];
        // Prefix a dummy program name so every real token uses Windows argument
        // quoting rules, whether activation includes the executable or not.
        IntPtr argv = CommandLineToArgvW("Rune " + commandLine, out int count);
        if (argv == IntPtr.Zero) throw new System.ComponentModel.Win32Exception();
        try
        {
            return Enumerable.Range(1, count - 1)
                .Select(i => Marshal.PtrToStringUni(Marshal.ReadIntPtr(argv, i * IntPtr.Size))!)
                .ToArray();
        }
        finally { LocalFree(argv); }
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CommandLineToArgvW(string commandLine, out int count);

    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr memory);
}

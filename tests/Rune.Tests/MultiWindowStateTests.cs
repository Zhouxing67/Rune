using Rune.Services;

namespace Rune.Tests;

public class MultiWindowStateTests
{
    [Fact]
    public void UpdatingPositionDoesNotReorderRecentDocuments()
    {
        var state = new AppState();
        state.Remember("a.pdf", "A", 0, 1, 0, 0);
        state.Remember("b.pdf", "B", 0, 1, 0, 0);
        var opened = state.FindRecent("a.pdf")!.LastOpenedUtc;
        state.Remember("a.pdf", "A", 8, 2, 90, .5, markOpened: false);
        Assert.Equal("b.pdf", state.Recents[0].Path);
        Assert.Equal(opened, state.FindRecent("a.pdf")!.LastOpenedUtc);
        Assert.Equal(8, state.FindRecent("a.pdf")!.PageIndex);
    }

    [Fact]
    public void MultipleWindowSessionsRoundTrip()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        try
        {
            var store = new AppStateStore(directory);
            var state = new AppState();
            state.Session.Windows.Add(new SessionState { OpenPaths = ["a.pdf"], ActivePath = "a.pdf" });
            state.Session.Windows.Add(new SessionState { OpenPaths = ["b.pdf", "c.pdf"], ActiveIndex = 1, ActivePath = "c.pdf" });
            store.Save(state);
            var restored = store.Load();
            Assert.Equal(2, restored.Session.Windows.Count);
            Assert.Equal("c.pdf", restored.Session.Windows[1].ActivePath);
            Assert.Equal(2, restored.Session.Windows[1].OpenPaths.Count);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Fact]
    public void FileIdentityNormalizesRelativeSegmentsAndCase()
    {
        var root = Path.GetTempPath();
        Assert.True(DocumentIdentity.Same(Path.Combine(root, "folder", "..", "A.pdf"), Path.Combine(root, "a.pdf")));
        Assert.False(DocumentIdentity.Same(Path.Combine(root, "a.pdf"), Path.Combine(root, "b.pdf")));
    }
}

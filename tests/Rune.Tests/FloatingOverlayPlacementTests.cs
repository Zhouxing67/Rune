using Rune.Engine;

namespace Rune.Tests;

public sealed class FloatingOverlayPlacementTests
{
    [Fact]
    public void Place_PrefersRequestedSideWithoutCoveringAnchor()
    {
        var anchor = new DipRect(100, 100, 40, 20);

        var position = FloatingOverlayPlacement.Place(anchor, 80, 30, 400, 300);

        Assert.Equal(80, position.X);
        Assert.Equal(62, position.Y);
    }

    [Fact]
    public void Place_FallsBelowWhenTopEdgeHasNoRoom()
    {
        var anchor = new DipRect(100, 10, 40, 20);

        var position = FloatingOverlayPlacement.Place(anchor, 80, 30, 400, 300);

        Assert.Equal(38, position.Y);
    }

    [Fact]
    public void Place_MovesOutsideWholeSelectionWhenAnotherLineBlocksIt()
    {
        var anchor = new DipRect(100, 100, 40, 20);
        DipRect[] selection = [anchor, new DipRect(80, 62, 100, 25)];

        var position = FloatingOverlayPlacement.Place(
            anchor, 80, 30, 400, 300, FloatingOverlaySide.Above, selection);

        Assert.Equal(24, position.Y);
    }

    [Fact]
    public void Place_ClampsToHorizontalViewportEdge()
    {
        var anchor = new DipRect(2, 100, 10, 20);

        var position = FloatingOverlayPlacement.Place(anchor, 80, 30, 400, 300);

        Assert.Equal(8, position.X);
    }
}

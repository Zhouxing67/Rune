namespace Rune.Engine;

public enum FloatingOverlaySide
{
    Above,
    Below,
}

/// <summary>Viewport position for a floating control anchored to document content.</summary>
public readonly record struct FloatingOverlayPosition(double X, double Y);

/// <summary>
/// Places a floating control near an anchor while keeping it inside the viewport
/// and, whenever possible, clear of the content it describes.
/// </summary>
public static class FloatingOverlayPlacement
{
    public static FloatingOverlayPosition Place(
        DipRect anchor,
        double overlayWidth,
        double overlayHeight,
        double viewportWidth,
        double viewportHeight,
        FloatingOverlaySide preferredSide = FloatingOverlaySide.Above,
        IReadOnlyList<DipRect>? avoid = null,
        double edge = 8,
        double gap = 8)
    {
        overlayWidth = Math.Max(0, overlayWidth);
        overlayHeight = Math.Max(0, overlayHeight);
        double maxX = Math.Max(edge, viewportWidth - overlayWidth - edge);
        double maxY = Math.Max(edge, viewportHeight - overlayHeight - edge);
        double x = Math.Clamp(anchor.X + (anchor.Width - overlayWidth) / 2, edge, maxX);

        var obstacles = avoid is { Count: > 0 } ? avoid : [anchor];
        DipRect bounds = BoundsOf(obstacles);
        FloatingOverlaySide alternate = preferredSide == FloatingOverlaySide.Above
            ? FloatingOverlaySide.Below
            : FloatingOverlaySide.Above;

        // First stay close to the pointer/link. If another line of a multi-line
        // selection is in the way, move outside the whole selection instead.
        var candidates = new[]
        {
            Candidate(anchor, preferredSide),
            Candidate(bounds, preferredSide),
            Candidate(anchor, alternate),
            Candidate(bounds, alternate),
        };

        foreach (double rawY in candidates)
        {
            var candidate = new DipRect(x, rawY, overlayWidth, overlayHeight);
            if (rawY >= edge && rawY <= maxY && !obstacles.Any(candidate.Intersects))
            {
                return new FloatingOverlayPosition(x, rawY);
            }
        }

        // Tiny windows may not have a completely clear side. Pick the candidate
        // that covers the least protected content, then clamp it to the viewport.
        return candidates
            .Select((rawY, index) =>
            {
                double y = Math.Clamp(rawY, edge, maxY);
                var rect = new DipRect(x, y, overlayWidth, overlayHeight);
                double overlap = obstacles.Sum(obstacle => IntersectionArea(rect, obstacle));
                return (Position: new FloatingOverlayPosition(x, y), Overlap: overlap, Index: index);
            })
            .OrderBy(item => item.Overlap)
            .ThenBy(item => item.Index)
            .First().Position;

        double Candidate(DipRect target, FloatingOverlaySide side) => side == FloatingOverlaySide.Above
            ? target.Y - overlayHeight - gap
            : target.Bottom + gap;
    }

    private static DipRect BoundsOf(IReadOnlyList<DipRect> rects)
    {
        double left = rects.Min(rect => rect.X);
        double top = rects.Min(rect => rect.Y);
        double right = rects.Max(rect => rect.Right);
        double bottom = rects.Max(rect => rect.Bottom);
        return new DipRect(left, top, right - left, bottom - top);
    }

    private static double IntersectionArea(DipRect left, DipRect right)
    {
        double width = Math.Max(0, Math.Min(left.Right, right.Right) - Math.Max(left.X, right.X));
        double height = Math.Max(0, Math.Min(left.Bottom, right.Bottom) - Math.Max(left.Y, right.Y));
        return width * height;
    }
}

using System.Globalization;

namespace Rune.Engine;

public sealed record LaunchCommand(IReadOnlyList<string> Files, int? Page, double? Zoom)
{
    public static LaunchCommand Parse(IEnumerable<string> arguments, string? executablePath)
    {
        var args = arguments.ToArray();
        var files = new List<string>();
        int? page = null;
        double? zoom = null;
        bool literal = false;
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (!literal && arg == "--") { literal = true; continue; }
            if (!literal && (arg == "--page" || arg == "--zoom"))
            {
                if (i + 1 >= args.Length) continue;
                string value = args[++i];
                if (arg == "--page" && int.TryParse(value, out int p) && p > 0) page = p;
                if (arg == "--zoom" && double.TryParse(value, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out double z) && double.IsFinite(z) && z > 0) zoom = z;
                continue;
            }
            if (!literal && arg.StartsWith("--", StringComparison.Ordinal)) continue;
            // AppLifecycle launch activation can contain the executable itself.
            if (executablePath is not null &&
                string.Equals(arg, executablePath, StringComparison.OrdinalIgnoreCase)) continue;
            if (string.Equals(Path.GetExtension(arg), ".exe", StringComparison.OrdinalIgnoreCase)) continue;
            if (!string.IsNullOrWhiteSpace(arg)) files.Add(arg);
        }
        return new LaunchCommand(files, page, zoom);
    }
}

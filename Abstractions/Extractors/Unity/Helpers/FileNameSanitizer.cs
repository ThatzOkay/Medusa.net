using System.Text.RegularExpressions;

namespace Abstractions.Extractors.Unity.Helpers;

/// <summary>Strips characters that aren't safe in a filename and collapses whitespace to underscores.</summary>
public static partial class FileNameSanitizer
{
    [GeneratedRegex("""[<>:"/\\|?*]""")]
    private static partial Regex InvalidCharsRegex();

    public static string Sanitize(string name)
    {
        var saneName = InvalidCharsRegex().Replace(name, "_");
        saneName = saneName.Replace(' ', '_');
        saneName = saneName.Trim('_').Trim();

        return string.IsNullOrEmpty(saneName) ? "Untitled" : saneName;
    }
}

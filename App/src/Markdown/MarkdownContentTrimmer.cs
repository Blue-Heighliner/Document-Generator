namespace BlueHeighliner.DocumentGenerator.Markdown;

/// <summary>Trims markdown source text down to the content between its outer horizontal bars.</summary>
internal interface IMarkdownContentTrimmer
{
    /// <summary>
    /// When <paramref name="excludeSurroundingHorizontalBars"/> is true and <paramref name="markdown"/>
    /// contains two or more horizontal bar lines (<c>---</c>/<c>***</c>/<c>___</c>), returns only the
    /// content strictly between the first and last bar. Otherwise returns <paramref name="markdown"/>
    /// unchanged.
    /// </summary>
    /// <param name="markdown">The raw markdown source text.</param>
    /// <param name="excludeSurroundingHorizontalBars">Whether trimming is enabled.</param>
    /// <returns>The (possibly trimmed) markdown source text.</returns>
    string Trim(string markdown, bool excludeSurroundingHorizontalBars);
}

/// <inheritdoc cref="IMarkdownContentTrimmer" />
internal sealed class MarkdownContentTrimmer : IMarkdownContentTrimmer
{
    /// <inheritdoc />
    public string Trim(string markdown, bool excludeSurroundingHorizontalBars)
    {
        if (!excludeSurroundingHorizontalBars)
        {
            return markdown;
        }

        string[] lines = markdown.Replace("\r\n", "\n").Split('\n');
        List<int> barLineIndices = [];
        for (int index = 0; index < lines.Length; index++)
        {
            if (IsHorizontalBar(lines[index]))
            {
                barLineIndices.Add(index);
            }
        }

        if (barLineIndices.Count < 2)
        {
            return markdown;
        }

        int first = barLineIndices[0];
        int last = barLineIndices[^1];
        return string.Join('\n', lines[(first + 1)..last]);
    }

    private bool IsHorizontalBar(string line)
    {
        string trimmed = line.Trim();
        if (trimmed.Length < 3)
        {
            return false;
        }

        char marker = trimmed[0];
        return marker is '-' or '*' or '_'
            && trimmed.All(character => character == marker || char.IsWhiteSpace(character))
            && trimmed.Count(character => character == marker) >= 3;
    }
}

namespace BlueHeighliner.DocumentGenerator.Numbering;

/// <summary>
/// Tracks per-level counters across a document and produces the numbering label for each heading
/// as it's encountered, in document order.
/// </summary>
internal interface IHeadingNumberer
{
    /// <summary>
    /// Advances this level's counter (resetting any deeper levels' counters, since they belong to
    /// the section this heading just started) and returns its formatted numbering label.
    /// </summary>
    /// <param name="level">The heading level (1-based) being numbered.</param>
    /// <param name="configuration">The document configuration carrying the per-level heading styling.</param>
    /// <returns>
    /// The formatted numbering label (prefixed with ancestor levels' numbers when configured), or
    /// <see langword="null" /> when numbering is disabled for <paramref name="level"/>.
    /// </returns>
    string? Number(int level, DocumentConfiguration configuration);
}

/// <inheritdoc cref="IHeadingNumberer" />
internal sealed class HeadingNumberer : IHeadingNumberer
{
    private readonly Dictionary<int, int> counters = new();

    /// <inheritdoc />
    public string? Number(int level, DocumentConfiguration configuration)
    {
        foreach (int deeperLevel in counters.Keys.Where(existingLevel => existingLevel > level).ToList())
        {
            counters.Remove(deeperLevel);
        }

        HeadingConfiguration heading = configuration.GetHeading(level);
        if (heading.Numbering == HeadingNumberingStyle.None)
        {
            return null;
        }

        counters[level] = counters.GetValueOrDefault(level) + 1;

        if (!heading.NumberingPrefix)
        {
            return heading.Numbering.Format(counters[level]);
        }

        IEnumerable<string> segments = counters.Keys
            .Where(ancestorLevel => ancestorLevel <= level)
            .OrderBy(ancestorLevel => ancestorLevel)
            .Where(ancestorLevel => configuration.GetHeading(ancestorLevel).Numbering != HeadingNumberingStyle.None)
            .Select(ancestorLevel => configuration.GetHeading(ancestorLevel).Numbering.Format(counters[ancestorLevel]));

        return string.Join('.', segments);
    }
}

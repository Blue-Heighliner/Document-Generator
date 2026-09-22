namespace BlueHeighliner.DocumentGenerator.Configuration;

/// <summary>
/// The content shown in a page header or footer, as up to three independently-stacked
/// left/center/right positions plus a page number. Lines at the same index across positions land
/// on the same row.
/// </summary>
internal sealed record PageSectionConfiguration
{
    /// <summary>Lines stacked at the left edge. Defaults to none.</summary>
    public IReadOnlyList<PageContentLine> Left { get; init; } = [];

    /// <summary>Lines stacked at the center. Defaults to none.</summary>
    public IReadOnlyList<PageContentLine> Center { get; init; } = [];

    /// <summary>Lines stacked at the right edge. Defaults to none.</summary>
    public IReadOnlyList<PageContentLine> Right { get; init; } = [];

    /// <summary>
    /// Whether the current page number is shown, and where it's aligned. When not
    /// <see cref="PageNumbersMode.None"/>, it's always the first row in the stack - the topmost row
    /// in a header, or the row closest to the page's bottom edge in a footer. Defaults to
    /// <see cref="PageNumbersMode.None"/>.
    /// </summary>
    public PageNumbersMode PageNumbers { get; init; } = PageNumbersMode.None;
}

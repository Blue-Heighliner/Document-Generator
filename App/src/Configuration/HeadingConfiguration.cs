namespace BlueHeighliner.DocumentGenerator.Configuration;

/// <summary>The styling and numbering rules applied to a single markdown heading level.</summary>
internal sealed record HeadingConfiguration
{
    /// <summary>The heading font size, in points. Defaults to 12.</summary>
    public double Size { get; init; } = 12;

    /// <summary>Whether the heading text is bold. Defaults to <see langword="false" />.</summary>
    public bool Bold { get; init; }

    /// <summary>Whether the heading text is italic. Defaults to <see langword="false" />.</summary>
    public bool Italic { get; init; }

    /// <summary>
    /// The heading font color, as a hex RGB string (e.g. <c>"1F2937"</c> or <c>"#1F2937"</c>).
    /// Defaults to black (<c>"#000000"</c>).
    /// </summary>
    public string Color { get; init; } = "#000000";

    /// <summary>
    /// The numbering style applied to this level, or <see cref="HeadingNumberingStyle.None"/> for
    /// none. Defaults to <see cref="HeadingNumberingStyle.None"/>.
    /// </summary>
    public HeadingNumberingStyle Numbering { get; init; } = HeadingNumberingStyle.None;

    /// <summary>
    /// When true, this level's number is prefixed with its ancestor levels' numbers (e.g.
    /// <c>1.2.1</c>); when false, this level is numbered independently (e.g. just <c>1</c>). Defaults
    /// to <see langword="false" />.
    /// </summary>
    public bool NumberingPrefix { get; init; }

    /// <summary>The paragraph spacing before the heading, in points. Defaults to 0.</summary>
    public double SpaceBefore { get; init; }

    /// <summary>The paragraph spacing after the heading, in points. Defaults to 0.</summary>
    public double SpaceAfter { get; init; }

    /// <summary>
    /// Whether this level is included in the table of contents. Defaults to <see langword="false" />.
    /// </summary>
    public bool TableOfContents { get; init; }

    /// <summary>
    /// When true, a page break is inserted immediately after this level's content ends (i.e. right
    /// before the next heading at this level or shallower). Defaults to <see langword="false" />.
    /// </summary>
    public bool PageBreakAfter { get; init; }
}

namespace BlueHeighliner.DocumentGenerator.Configuration;

/// <summary>
/// The root of the document configuration file: styling for headings, header/footer content, and
/// document-wide layout options (cover page, table of contents, page breaks, source trimming).
/// </summary>
internal sealed record DocumentConfiguration
{
    /// <summary>
    /// The cover page title. In JSON, a plain array of strings is shorthand for just its
    /// <c>lines</c>. Defaults to no lines, size 36, bold.
    /// </summary>
    [JsonConverter(typeof(TitleConfigurationConverter))]
    public CoverTextConfiguration Title { get; init; } = new() { Bold = true };

    /// <summary>
    /// The cover page subtitle. In JSON, a plain array of strings is shorthand for just its
    /// <c>lines</c>. Defaults to no lines, size 20.
    /// </summary>
    [JsonConverter(typeof(SubtitleConfigurationConverter))]
    public CoverTextConfiguration Subtitle { get; init; } = new() { Size = 20 };

    /// <summary>
    /// When true, only the markdown content between the first and last horizontal bar
    /// (<c>---</c>/<c>***</c>/<c>___</c>) in the source document is rendered; the bars themselves
    /// and any text outside them are excluded. Defaults to <see langword="false" />.
    /// </summary>
    public bool Exclusion { get; init; }

    /// <summary>
    /// The level 1 heading's styling and numbering. Defaults to unnumbered, size 24, bold, shown in
    /// the table of contents.
    /// </summary>
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public HeadingConfiguration Heading1 { get; init; } = new() { Size = 24, Bold = true, SpaceBefore = 24, SpaceAfter = 12, TableOfContents = true };

    /// <summary>
    /// The level 2 heading's styling and numbering. Defaults to unnumbered, size 20, bold, shown in
    /// the table of contents.
    /// </summary>
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public HeadingConfiguration Heading2 { get; init; } = new() { Size = 20, Bold = true, SpaceBefore = 18, SpaceAfter = 9, TableOfContents = true };

    /// <summary>The level 3 heading's styling and numbering. Defaults to unnumbered, size 16, bold.</summary>
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public HeadingConfiguration Heading3 { get; init; } = new() { Size = 16, Bold = true, SpaceBefore = 14, SpaceAfter = 7 };

    /// <summary>The level 4 heading's styling and numbering. Defaults to unnumbered, size 14, bold.</summary>
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public HeadingConfiguration Heading4 { get; init; } = new() { Size = 14, Bold = true, SpaceBefore = 12, SpaceAfter = 6 };

    /// <summary>The level 5 heading's styling and numbering. Defaults to unnumbered, size 12, bold.</summary>
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public HeadingConfiguration Heading5 { get; init; } = new() { Size = 12, Bold = true, SpaceBefore = 10, SpaceAfter = 5 };

    /// <summary>The level 6 heading's styling and numbering. Defaults to unnumbered, size 11, bold.</summary>
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public HeadingConfiguration Heading6 { get; init; } = new() { Size = 11, Bold = true, SpaceBefore = 8, SpaceAfter = 4 };

    /// <summary>
    /// The page header shown on every page and (optionally, differently) the first page. Defaults to
    /// an empty header.
    /// </summary>
    public PageSectionConfiguration Header { get; init; } = new();

    /// <summary>
    /// The page footer shown on every page and (optionally, differently) the first page. Defaults to
    /// an empty footer.
    /// </summary>
    public PageSectionConfiguration Footer { get; init; } = new();
}

/// <summary>Level-indexed access to <see cref="DocumentConfiguration"/>'s per-level heading properties.</summary>
internal static class DocumentConfigurationExtensions
{
    extension(DocumentConfiguration configuration)
    {
        /// <summary>Gets the heading configuration for the given level.</summary>
        /// <param name="level">The heading level (1-6).</param>
        /// <returns>The corresponding <c>Heading1</c>-<c>Heading6</c> property.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="level"/> is not between 1 and 6.</exception>
        public HeadingConfiguration GetHeading(int level) => level switch
        {
            1 => configuration.Heading1,
            2 => configuration.Heading2,
            3 => configuration.Heading3,
            4 => configuration.Heading4,
            5 => configuration.Heading5,
            6 => configuration.Heading6,
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Heading level must be between 1 and 6."),
        };
    }
}

namespace BlueHeighliner.DocumentGenerator.Tests;

/// <summary>Shared <see cref="DocumentConfiguration"/> and <see cref="HeadingConfiguration"/> builders for tests.</summary>
internal static class TestConfigurations
{
    /// <summary>Builds a minimal, valid document configuration with the given overrides.</summary>
    public static DocumentConfiguration BuildDocument(
        IReadOnlyList<string>? title = null,
        IReadOnlyList<string>? subtitle = null,
        bool exclusion = false,
        HeadingConfiguration? heading1 = null,
        HeadingConfiguration? heading2 = null,
        HeadingConfiguration? heading3 = null,
        HeadingConfiguration? heading4 = null,
        HeadingConfiguration? heading5 = null,
        HeadingConfiguration? heading6 = null,
        PageSectionConfiguration? header = null,
        PageSectionConfiguration? footer = null) => new()
        {
            Title = new CoverTextConfiguration { Lines = title ?? [], Bold = true },
            Subtitle = new CoverTextConfiguration { Lines = subtitle ?? [], Size = 20 },
            Exclusion = exclusion,
            Heading1 = heading1 ?? new HeadingConfiguration(),
            Heading2 = heading2 ?? new HeadingConfiguration(),
            Heading3 = heading3 ?? new HeadingConfiguration(),
            Heading4 = heading4 ?? new HeadingConfiguration(),
            Heading5 = heading5 ?? new HeadingConfiguration(),
            Heading6 = heading6 ?? new HeadingConfiguration(),
            Header = header ?? BuildSection(),
            Footer = footer ?? BuildSection(),
        };

    /// <summary>Builds a header/footer content section with the given overrides.</summary>
    public static PageSectionConfiguration BuildSection(
        IReadOnlyList<PageContentLine>? left = null,
        IReadOnlyList<PageContentLine>? center = null,
        IReadOnlyList<PageContentLine>? right = null,
        PageNumbersMode pageNumbers = PageNumbersMode.None) => new()
        {
            Left = left ?? [],
            Center = center ?? [],
            Right = right ?? [],
            PageNumbers = pageNumbers,
        };

    /// <summary>Builds mode-"all" lines from plain strings - a convenience for tests that don't care about mode.</summary>
    public static IReadOnlyList<PageContentLine> BuildLines(params string[] lines) => [.. lines.Select(line => new PageContentLine { Line = line })];

    /// <summary>Builds a heading style configuration with the given overrides.</summary>
    public static HeadingConfiguration BuildHeading(
        double size = 12,
        bool bold = false,
        bool italic = false,
        string color = "#000000",
        HeadingNumberingStyle numbering = HeadingNumberingStyle.None,
        bool numberingPrefix = false,
        double spaceBefore = 0,
        double spaceAfter = 0,
        bool tableOfContents = false,
        bool pageBreakAfter = false) => new()
        {
            Size = size,
            Bold = bold,
            Italic = italic,
            Color = color,
            Numbering = numbering,
            NumberingPrefix = numberingPrefix,
            SpaceBefore = spaceBefore,
            SpaceAfter = spaceAfter,
            TableOfContents = tableOfContents,
            PageBreakAfter = pageBreakAfter,
        };
}

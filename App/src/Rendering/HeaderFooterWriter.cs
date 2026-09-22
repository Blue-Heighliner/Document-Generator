namespace BlueHeighliner.DocumentGenerator.Rendering;

/// <summary>The relationship ids of a header or footer's "default" and "first page" parts.</summary>
internal sealed record HeaderFooterPartIds
{
    /// <summary>The relationship id of the part shown on every page other than a section's first.</summary>
    public required string DefaultId { get; init; }

    /// <summary>The relationship id of the part shown on a section's first page.</summary>
    public required string FirstId { get; init; }
}

/// <summary>Builds header and footer parts from <see cref="PageSectionConfiguration"/>.</summary>
internal interface IHeaderFooterWriter
{
    /// <summary>Creates the default and first-page header parts for <paramref name="configuration"/>.</summary>
    /// <param name="mainPart">The document's main part to add the header parts to.</param>
    /// <param name="configuration">The header content configuration.</param>
    /// <param name="printableWidth">The page's printable width (page width minus left and right margins), in points, used to position center/right tab stops.</param>
    /// <returns>The relationship ids of the created parts.</returns>
    HeaderFooterPartIds WriteHeader(MainDocumentPart mainPart, PageSectionConfiguration configuration, double printableWidth);

    /// <summary>Creates the default and first-page footer parts for <paramref name="configuration"/>.</summary>
    /// <param name="mainPart">The document's main part to add the footer parts to.</param>
    /// <param name="configuration">The footer content configuration.</param>
    /// <param name="printableWidth">The page's printable width (page width minus left and right margins), in points, used to position center/right tab stops.</param>
    /// <returns>The relationship ids of the created parts.</returns>
    HeaderFooterPartIds WriteFooter(MainDocumentPart mainPart, PageSectionConfiguration configuration, double printableWidth);
}

/// <inheritdoc cref="IHeaderFooterWriter" />
internal sealed class HeaderFooterWriter : IHeaderFooterWriter
{
    /// <inheritdoc />
    public HeaderFooterPartIds WriteHeader(MainDocumentPart mainPart, PageSectionConfiguration configuration, double printableWidth)
    {
        HeaderPart defaultPart = mainPart.AddNewPart<HeaderPart>();
        defaultPart.Header = new Header(BuildParagraphs(configuration, PageContentMode.Body, reverseRows: false, printableWidth));
        defaultPart.Header.Save();

        HeaderPart firstPart = mainPart.AddNewPart<HeaderPart>();
        firstPart.Header = new Header(BuildParagraphs(configuration, PageContentMode.Cover, reverseRows: false, printableWidth));
        firstPart.Header.Save();

        return new HeaderFooterPartIds { DefaultId = mainPart.GetIdOfPart(defaultPart), FirstId = mainPart.GetIdOfPart(firstPart) };
    }

    /// <inheritdoc />
    public HeaderFooterPartIds WriteFooter(MainDocumentPart mainPart, PageSectionConfiguration configuration, double printableWidth)
    {
        FooterPart defaultPart = mainPart.AddNewPart<FooterPart>();
        defaultPart.Footer = new Footer(BuildParagraphs(configuration, PageContentMode.Body, reverseRows: true, printableWidth));
        defaultPart.Footer.Save();

        FooterPart firstPart = mainPart.AddNewPart<FooterPart>();
        firstPart.Footer = new Footer(BuildParagraphs(configuration, PageContentMode.Cover, reverseRows: true, printableWidth));
        firstPart.Footer.Save();

        return new HeaderFooterPartIds { DefaultId = mainPart.GetIdOfPart(defaultPart), FirstId = mainPart.GetIdOfPart(firstPart) };
    }

    /// <summary>
    /// Builds one row paragraph per line position (left/center/right lines at the same index share a
    /// row), with the page number - if enabled - prepended as row 0. Then - for a footer, whose
    /// content stacks upward from the page's bottom edge rather than downward from its top - reverses
    /// the whole row order so row 0 (the page number, when present, otherwise the first-specified
    /// line of each position) ends up closest to the page edge.
    /// </summary>
    private List<Paragraph> BuildParagraphs(PageSectionConfiguration configuration, PageContentMode variant, bool reverseRows, double printableWidth)
    {
        List<string> left = Filter(configuration.Left, variant);
        List<string> center = Filter(configuration.Center, variant);
        List<string> right = Filter(configuration.Right, variant);

        int rowCount = new[] { left.Count, center.Count, right.Count }.Max();
        List<Paragraph> rows = [];
        for (int index = 0; index < rowCount; index++)
        {
            rows.Add(BuildRow(
                index < left.Count ? left[index] : null,
                index < center.Count ? center[index] : null,
                index < right.Count ? right[index] : null,
                printableWidth));
        }

        if (configuration.PageNumbers != PageNumbersMode.None)
        {
            rows.Insert(0, BuildPageNumberParagraph(configuration.PageNumbers));
        }

        if (reverseRows)
        {
            rows.Reverse();
        }

        return rows.Count > 0 ? rows : [new Paragraph()];
    }

    private List<string> Filter(IReadOnlyList<PageContentLine> lines, PageContentMode variant) =>
        [.. lines.Where(line => line.Mode == PageContentMode.All || line.Mode == variant).Select(line => line.Line)];

    private Paragraph BuildRow(string? left, string? center, string? right, double printableWidth)
    {
        int presentCount = new[] { left, center, right }.Count(value => value is not null);
        if (presentCount <= 1)
        {
            (string text, JustificationValues justification) = left is not null
                ? (left, JustificationValues.Left)
                : center is not null
                    ? (center, JustificationValues.Center)
                    : (right ?? string.Empty, JustificationValues.Right);

            return new Paragraph(
                new ParagraphProperties(new Justification { Val = justification }),
                new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
        }

        int centerPosition = (int)Math.Round(printableWidth * 20 / 2);
        int rightPosition = (int)Math.Round(printableWidth * 20);

        Paragraph paragraph = new(new ParagraphProperties(new Tabs(
            new TabStop { Val = TabStopValues.Center, Position = centerPosition },
            new TabStop { Val = TabStopValues.Right, Position = rightPosition })));
        paragraph.Append(
            new Run(new Text(left ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve }),
            new Run(new TabChar()),
            new Run(new Text(center ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve }),
            new Run(new TabChar()),
            new Run(new Text(right ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve }));
        return paragraph;
    }

    private Paragraph BuildPageNumberParagraph(PageNumbersMode pageNumbers) => new(
        new ParagraphProperties(new Justification { Val = ToJustification(pageNumbers) }),
        new Run(new Text("Page ") { Space = SpaceProcessingModeValues.Preserve }),
        new Run(new FieldChar { FieldCharType = FieldCharValues.Begin }),
        new Run(new FieldCode(" PAGE ") { Space = SpaceProcessingModeValues.Preserve }),
        new Run(new FieldChar { FieldCharType = FieldCharValues.Separate }),
        new Run(new Text("1")),
        new Run(new FieldChar { FieldCharType = FieldCharValues.End }));

    private JustificationValues ToJustification(PageNumbersMode pageNumbers) => pageNumbers switch
    {
        PageNumbersMode.Left => JustificationValues.Left,
        PageNumbersMode.Center => JustificationValues.Center,
        PageNumbersMode.Right => JustificationValues.Right,
        _ => throw new ArgumentOutOfRangeException(nameof(pageNumbers), pageNumbers, "Unknown page numbers mode."),
    };
}

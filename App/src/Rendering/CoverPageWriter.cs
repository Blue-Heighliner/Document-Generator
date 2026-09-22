namespace BlueHeighliner.DocumentGenerator.Rendering;

/// <summary>Writes the cover page's title and subtitle content to the document body.</summary>
internal interface ICoverPageWriter
{
    /// <summary>Appends the cover page's title and subtitle paragraphs to <paramref name="body"/>.</summary>
    /// <param name="body">The document body to append to.</param>
    /// <param name="configuration">The document configuration carrying the title and subtitle text.</param>
    /// <param name="availableHeight">
    /// The cover page's text area height, in points (page height minus top and bottom margins), used
    /// to vertically center the title/subtitle block within it.
    /// </param>
    void Write(Body body, DocumentConfiguration configuration, double availableHeight);
}

/// <inheritdoc cref="ICoverPageWriter" />
internal sealed class CoverPageWriter : ICoverPageWriter
{
    private readonly double lineHeightMultiplier = 1.2;
    private readonly double titleSubtitleGap = 20;

    /// <summary>
    /// Vertically centers the title/subtitle block by computing exact line heights and a leading
    /// space-before offset from known font sizes and the page's text area height, rather than relying
    /// on the OOXML section-level vertical alignment property (<c>w:vAlign</c>) - real Word honors it,
    /// but LibreOffice's DOCX import does not, even for the simplest single-section reproduction.
    /// Explicit paragraph spacing renders identically in both.
    /// </summary>
    /// <inheritdoc />
    public void Write(Body body, DocumentConfiguration configuration, double availableHeight)
    {
        CoverTextConfiguration title = configuration.Title;
        CoverTextConfiguration subtitle = configuration.Subtitle;
        double titleLineHeight = title.Size * lineHeightMultiplier;
        double subtitleLineHeight = subtitle.Size * lineHeightMultiplier;
        bool hasGap = title.Lines.Count > 0 && subtitle.Lines.Count > 0;

        double contentHeight = (title.Lines.Count * titleLineHeight)
            + (hasGap ? titleSubtitleGap : 0)
            + (subtitle.Lines.Count * subtitleLineHeight);
        double topSpacing = Math.Max(0, (availableHeight - contentHeight) / 2);

        bool isFirstLine = true;
        foreach (string line in title.Lines)
        {
            body.AppendChild(BuildLine(line, title, titleLineHeight, spaceBefore: isFirstLine ? topSpacing : 0));
            isFirstLine = false;
        }

        for (int index = 0; index < subtitle.Lines.Count; index++)
        {
            double spaceBefore = index == 0 ? (hasGap ? titleSubtitleGap : (isFirstLine ? topSpacing : 0)) : 0;
            body.AppendChild(BuildLine(subtitle.Lines[index], subtitle, subtitleLineHeight, spaceBefore));
            isFirstLine = false;
        }
    }

    private Paragraph BuildLine(string text, CoverTextConfiguration style, double lineHeight, double spaceBefore)
    {
        RunProperties runProperties = new(new FontSize { Val = (style.Size * 2).ToString(CultureInfo.InvariantCulture) });
        runProperties.Color = new Color { Val = style.Color.TrimStart('#') };
        if (style.Bold)
        {
            runProperties.Bold = new Bold();
        }

        if (style.Italic)
        {
            runProperties.Italic = new Italic();
        }

        return new Paragraph(
            new ParagraphProperties(
                new SpacingBetweenLines
                {
                    Before = ((long)Math.Round(spaceBefore * 20)).ToString(CultureInfo.InvariantCulture),
                    Line = ((long)Math.Round(lineHeight * 20)).ToString(CultureInfo.InvariantCulture),
                    LineRule = LineSpacingRuleValues.Exact,
                },
                new Justification { Val = JustificationValues.Center }),
            new Run(runProperties, new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    }
}

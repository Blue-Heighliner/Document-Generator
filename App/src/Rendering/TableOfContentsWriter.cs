namespace BlueHeighliner.DocumentGenerator.Rendering;

/// <summary>Writes a "Table of Contents" heading and an updatable TOC field to the document body.</summary>
internal interface ITableOfContentsWriter
{
    /// <summary>Appends the table of contents page content to <paramref name="body"/>.</summary>
    /// <param name="body">The document body to append to.</param>
    /// <param name="levels">The heading levels to include in the generated table of contents.</param>
    void Write(Body body, IReadOnlyList<int> levels);
}

/// <inheritdoc cref="ITableOfContentsWriter" />
internal sealed class TableOfContentsWriter : ITableOfContentsWriter
{
    /// <inheritdoc />
    public void Write(Body body, IReadOnlyList<int> levels)
    {
        body.AppendChild(new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            new Run(new RunProperties(new Bold(), new FontSize { Val = "32" }), new Text("Table of Contents"))));

        body.AppendChild(new Paragraph());

        string instruction = $" TOC \\o \"{BuildLevelRanges(levels)}\" \\h \\z \\u ";
        body.AppendChild(new Paragraph(
            new Run(new FieldChar { FieldCharType = FieldCharValues.Begin }),
            new Run(new FieldCode(instruction) { Space = SpaceProcessingModeValues.Preserve }),
            new Run(new FieldChar { FieldCharType = FieldCharValues.Separate }),
            new Run(new Text("Right-click and choose \"Update Field\" to generate the table of contents.")),
            new Run(new FieldChar { FieldCharType = FieldCharValues.End })));
    }

    private string BuildLevelRanges(IReadOnlyList<int> levels)
    {
        List<int> sorted = levels.Distinct().OrderBy(level => level).ToList();
        List<string> ranges = [];

        int rangeStart = sorted[0];
        int rangeEnd = sorted[0];
        for (int index = 1; index <= sorted.Count; index++)
        {
            if (index < sorted.Count && sorted[index] == rangeEnd + 1)
            {
                rangeEnd = sorted[index];
                continue;
            }

            ranges.Add($"{rangeStart}-{rangeEnd}");
            if (index < sorted.Count)
            {
                rangeStart = sorted[index];
                rangeEnd = sorted[index];
            }
        }

        return string.Join(';', ranges);
    }
}

namespace BlueHeighliner.DocumentGenerator.Rendering;

/// <summary>Converts a markdown inline tree (bold/italic/code spans, line breaks, links) into OpenXML runs.</summary>
internal interface IInlineRunWriter
{
    /// <summary>Converts <paramref name="inline"/>'s children into a flat sequence of OpenXML runs.</summary>
    /// <param name="inline">The container of inline elements to convert, or <see langword="null" /> for none.</param>
    /// <returns>The converted runs, in document order.</returns>
    IReadOnlyList<OpenXmlElement> Write(ContainerInline? inline);
}

/// <inheritdoc cref="IInlineRunWriter" />
internal sealed class InlineRunWriter : IInlineRunWriter
{
    /// <inheritdoc />
    public IReadOnlyList<OpenXmlElement> Write(ContainerInline? inline)
    {
        List<OpenXmlElement> elements = [];
        if (inline is not null)
        {
            AppendChildren(elements, inline, bold: false, italic: false);
        }

        return elements;
    }

    private void AppendChildren(List<OpenXmlElement> elements, ContainerInline container, bool bold, bool italic)
    {
        foreach (Inline child in container)
        {
            switch (child)
            {
                case LiteralInline literal:
                    elements.Add(BuildRun(literal.Content.ToString(), bold, italic, monospace: false));
                    break;
                case CodeInline code:
                    elements.Add(BuildRun(code.Content, bold, italic, monospace: true));
                    break;
                case LineBreakInline:
                    elements.Add(new Run(new Break()));
                    break;
                case EmphasisInline emphasis:
                    AppendChildren(elements, emphasis, bold || emphasis.DelimiterCount >= 2, italic || emphasis.DelimiterCount % 2 == 1);
                    break;
                case ContainerInline containerChild:
                    AppendChildren(elements, containerChild, bold, italic);
                    break;
            }
        }
    }

    private Run BuildRun(string text, bool bold, bool italic, bool monospace)
    {
        RunProperties properties = new();
        if (bold)
        {
            properties.Bold = new Bold();
        }

        if (italic)
        {
            properties.Italic = new Italic();
        }

        if (monospace)
        {
            properties.RunFonts = new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas", ComplexScript = "Consolas" };
            properties.Shading = new Shading { Val = ShadingPatternValues.Clear, Fill = "D9D9D9" };
        }

        Run run = new(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        if (properties.HasChildren)
        {
            run.PrependChild(properties);
        }

        return run;
    }
}

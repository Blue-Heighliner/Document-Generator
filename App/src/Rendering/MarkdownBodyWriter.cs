namespace BlueHeighliner.DocumentGenerator.Rendering;

/// <summary>Converts a parsed markdown document into the document body's content.</summary>
internal interface IMarkdownBodyWriter
{
    /// <summary>Converts <paramref name="markdown"/> and appends the result to <paramref name="mainPart"/>'s body.</summary>
    /// <param name="mainPart">The main document part content is appended to and mermaid images are embedded into.</param>
    /// <param name="configuration">The document configuration driving heading styles, numbering, and page breaks.</param>
    /// <param name="markdown">The (already trimmed) markdown source text.</param>
    /// <param name="cancellation">A token used to cancel any mermaid diagram rendering.</param>
    Task Write(MainDocumentPart mainPart, DocumentConfiguration configuration, string markdown, CancellationToken cancellation);
}

/// <inheritdoc cref="IMarkdownBodyWriter" />
internal sealed class MarkdownBodyWriter(
    IHeadingNumberer headingNumberer,
    IInlineRunWriter inlineRunWriter,
    ITableConverter tableConverter,
    IMermaidImageRenderer mermaidImageRenderer,
    IDrawingElementBuilder drawingElementBuilder) : IMarkdownBodyWriter
{
    private readonly long maxImageWidthEmu = 5486400;

    /// <inheritdoc />
    public async Task Write(MainDocumentPart mainPart, DocumentConfiguration configuration, string markdown, CancellationToken cancellation)
    {
        Body body = mainPart.Document!.Body!;
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
        MarkdownDocument document = Markdig.Markdown.Parse(markdown, pipeline);

        List<int> openHeadingLevels = [];
        int mermaidImageCount = 0;

        foreach (Block block in document)
        {
            if (block is HeadingBlock heading)
            {
                CloseHeadingSections(body, configuration, openHeadingLevels, heading.Level);
                openHeadingLevels.Add(heading.Level);
                WriteHeading(body, configuration, heading);
            }
            else if (block is FencedCodeBlock fenced && string.Equals(fenced.Info, "mermaid", StringComparison.OrdinalIgnoreCase))
            {
                mermaidImageCount++;
                await WriteMermaidDiagram(mainPart, body, fenced, mermaidImageCount, cancellation);
            }
            else if (block is CodeBlock code)
            {
                WriteCodeBlock(body, code);
            }
            else if (block is Markdig.Extensions.Tables.Table table)
            {
                body.AppendChild(tableConverter.Convert(table, inlineRunWriter));
                body.AppendChild(new Paragraph());
            }
            else if (block is ParagraphBlock paragraph)
            {
                Paragraph wordParagraph = new();
                wordParagraph.Append(inlineRunWriter.Write(paragraph.Inline));
                body.AppendChild(wordParagraph);
            }
            else if (block is ThematicBreakBlock)
            {
                body.AppendChild(new Paragraph(new ParagraphProperties(
                    new ParagraphBorders(new BottomBorder { Val = BorderValues.Single, Size = 6, Space = 1, Color = "808080" }))));
            }
            else if (block is ListBlock list)
            {
                WriteList(body, list, depth: 0);
            }
        }

        CloseHeadingSections(body, configuration, openHeadingLevels, level: 0);
    }

    private void CloseHeadingSections(Body body, DocumentConfiguration configuration, List<int> openLevels, int level)
    {
        bool needsPageBreak = false;
        for (int index = openLevels.Count - 1; index >= 0; index--)
        {
            if (level != 0 && openLevels[index] < level)
            {
                break;
            }

            if (configuration.GetHeading(openLevels[index]).PageBreakAfter)
            {
                needsPageBreak = true;
            }

            openLevels.RemoveAt(index);
        }

        if (needsPageBreak && level != 0)
        {
            body.AppendChild(new Paragraph(new Run(new Break { Type = BreakValues.Page })));
        }
    }

    private void WriteHeading(Body body, DocumentConfiguration configuration, HeadingBlock heading)
    {
        HeadingConfiguration style = configuration.GetHeading(heading.Level);
        List<OpenXmlElement> runs = [];

        string? number = headingNumberer.Number(heading.Level, configuration);
        if (number is not null)
        {
            runs.Add(new Run(new Text($"{number}  ") { Space = SpaceProcessingModeValues.Preserve }));
        }

        runs.AddRange(inlineRunWriter.Write(heading.Inline));

        foreach (Run run in runs.OfType<Run>())
        {
            RunProperties properties = run.RunProperties ??= new RunProperties();
            properties.FontSize = new FontSize { Val = (style.Size * 2).ToString(CultureInfo.InvariantCulture) };
            properties.Color = new Color { Val = style.Color.TrimStart('#') };
            if (style.Bold)
            {
                properties.Bold = new Bold();
            }

            if (style.Italic)
            {
                properties.Italic = new Italic();
            }
        }

        Paragraph paragraph = new(new ParagraphProperties(
            new SpacingBetweenLines
            {
                Before = (style.SpaceBefore * 20).ToString(CultureInfo.InvariantCulture),
                After = (style.SpaceAfter * 20).ToString(CultureInfo.InvariantCulture),
            },
            new OutlineLevel { Val = heading.Level - 1 }));
        paragraph.Append(runs);
        body.AppendChild(paragraph);
    }

    private void WriteCodeBlock(Body body, CodeBlock code)
    {
        string[] lines = code.Lines.ToString().Replace("\r\n", "\n").Split('\n');
        List<OpenXmlElement> runs = [];
        for (int index = 0; index < lines.Length; index++)
        {
            if (index > 0)
            {
                runs.Add(new Run(new Break()));
            }

            runs.Add(new Run(
                new RunProperties(new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas", ComplexScript = "Consolas" }),
                new Text(lines[index]) { Space = SpaceProcessingModeValues.Preserve }));
        }

        Paragraph paragraph = new(new ParagraphProperties(new Shading { Val = ShadingPatternValues.Clear, Fill = "D9D9D9" }));
        paragraph.Append(runs);
        body.AppendChild(paragraph);
    }

    private async Task WriteMermaidDiagram(MainDocumentPart mainPart, Body body, FencedCodeBlock fenced, int imageNumber, CancellationToken cancellation)
    {
        string diagram = fenced.Lines.ToString();
        MermaidImage image = await mermaidImageRenderer.Render(diagram, cancellation);

        ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Png);
        using (MemoryStream imageStream = new(image.Data.ToArray()))
        {
            imagePart.FeedData(imageStream);
        }

        string relationshipId = mainPart.GetIdOfPart(imagePart);
        string name = $"Diagram {imageNumber}";
        Drawing drawing = drawingElementBuilder.BuildPicture(relationshipId, name, image.Width, image.Height, maxImageWidthEmu);

        body.AppendChild(new Paragraph(
            new ParagraphProperties(new Justification { Val = JustificationValues.Center }),
            new Run(drawing)));
    }

    private void WriteList(Body body, ListBlock list, int depth)
    {
        int number = 1;
        foreach (ListItemBlock item in list)
        {
            string prefix = list.IsOrdered ? $"{number}. " : "•  ";
            number++;

            foreach (Block block in item)
            {
                if (block is ParagraphBlock paragraph)
                {
                    Paragraph wordParagraph = new(new ParagraphProperties(
                        new Indentation { Left = (720 * (depth + 1)).ToString(CultureInfo.InvariantCulture) }));
                    wordParagraph.AppendChild(new Run(new Text(prefix) { Space = SpaceProcessingModeValues.Preserve }));
                    wordParagraph.Append(inlineRunWriter.Write(paragraph.Inline));
                    body.AppendChild(wordParagraph);
                }
                else if (block is ListBlock nested)
                {
                    WriteList(body, nested, depth + 1);
                }
            }
        }
    }
}

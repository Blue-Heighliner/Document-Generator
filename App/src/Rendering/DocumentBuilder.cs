namespace BlueHeighliner.DocumentGenerator.Rendering;

/// <summary>Generates a Word document from a configuration and markdown source, end to end.</summary>
internal interface IDocumentBuilder
{
    /// <summary>Generates the document and writes it to <paramref name="output"/>.</summary>
    /// <param name="configuration">The document configuration.</param>
    /// <param name="markdown">The (already trimmed) markdown source text.</param>
    /// <param name="output">The stream the generated <c>.docx</c> is written to.</param>
    /// <param name="cancellation">A token used to cancel generation.</param>
    Task Generate(DocumentConfiguration configuration, string markdown, Stream output, CancellationToken cancellation);
}

/// <inheritdoc cref="IDocumentBuilder" />
internal sealed class DocumentBuilder(
    ICoverPageWriter coverPageWriter,
    ITableOfContentsWriter tableOfContentsWriter,
    IHeaderFooterWriter headerFooterWriter,
    IMarkdownBodyWriter bodyWriter) : IDocumentBuilder
{
    private readonly uint pageWidth = 12240;
    private readonly uint pageHeight = 15840;
    private readonly uint pageMargin = 1440;
    private readonly uint headerFooterDistance = 720;

    /// <inheritdoc />
    public async Task Generate(DocumentConfiguration configuration, string markdown, Stream output, CancellationToken cancellation)
    {
        using WordprocessingDocument document = WordprocessingDocument.Create(output, WordprocessingDocumentType.Document);
        MainDocumentPart mainPart = document.AddMainDocumentPart();
        mainPart.Document = new Document(new Body());
        Body body = mainPart.Document!.Body!;

        DocumentSettingsPart settingsPart = mainPart.AddNewPart<DocumentSettingsPart>();
        settingsPart.Settings = new Settings(new HideSpellingErrors(), new HideGrammaticalErrors(), new UpdateFieldsOnOpen { Val = true });
        settingsPart.Settings.Save();

        double printableWidth = (pageWidth - (2 * pageMargin)) / 20.0;
        HeaderFooterPartIds headerIds = headerFooterWriter.WriteHeader(mainPart, configuration.Header, printableWidth);
        HeaderFooterPartIds footerIds = headerFooterWriter.WriteFooter(mainPart, configuration.Footer, printableWidth);

        double coverTextAreaHeight = (pageHeight - (2 * pageMargin)) / 20.0;
        coverPageWriter.Write(body, configuration, coverTextAreaHeight);
        body.AppendChild(new Paragraph(new ParagraphProperties(BuildSectionProperties(headerIds, footerIds, titlePage: true))));

        List<int> tableOfContentsLevels = [.. Enumerable.Range(1, 6).Where(level => configuration.GetHeading(level).TableOfContents)];
        if (tableOfContentsLevels.Count > 0)
        {
            tableOfContentsWriter.Write(body, tableOfContentsLevels);
            body.AppendChild(new Paragraph(new ParagraphProperties(BuildSectionProperties(headerIds, footerIds, titlePage: false))));
        }

        await bodyWriter.Write(mainPart, configuration, markdown, cancellation);
        body.AppendChild(BuildSectionProperties(headerIds, footerIds, titlePage: false));

        mainPart.Document!.Save();
    }

    private SectionProperties BuildSectionProperties(HeaderFooterPartIds headerIds, HeaderFooterPartIds footerIds, bool titlePage)
    {
        SectionProperties sectionProperties = new(
            new HeaderReference { Type = HeaderFooterValues.Default, Id = headerIds.DefaultId },
            new FooterReference { Type = HeaderFooterValues.Default, Id = footerIds.DefaultId });

        if (titlePage)
        {
            sectionProperties.Append(
                new HeaderReference { Type = HeaderFooterValues.First, Id = headerIds.FirstId },
                new FooterReference { Type = HeaderFooterValues.First, Id = footerIds.FirstId });
        }

        sectionProperties.Append(new PageSize { Width = pageWidth, Height = pageHeight });
        sectionProperties.Append(new PageMargin
        {
            Top = (int)pageMargin,
            Bottom = (int)pageMargin,
            Left = pageMargin,
            Right = pageMargin,
            Header = headerFooterDistance,
            Footer = headerFooterDistance,
            Gutter = 0,
        });

        if (titlePage)
        {
            sectionProperties.Append(new TitlePage());
        }

        return sectionProperties;
    }
}

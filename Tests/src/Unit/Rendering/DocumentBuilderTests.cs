namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Rendering;

public sealed class DocumentBuilderTests
{
    private readonly DocumentBuilder builder;

    public DocumentBuilderTests()
    {
        Mock<IMermaidImageRenderer> mermaidImageRenderer = new();
        mermaidImageRenderer
            .Setup(renderer => renderer.Render(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MermaidImage { Data = TestImages.BuildMinimalPng(), Width = 10, Height = 10 });

        InlineRunWriter inlineRunWriter = new();
        MarkdownBodyWriter bodyWriter = new(new HeadingNumberer(), inlineRunWriter, new TableConverter(), mermaidImageRenderer.Object, new DrawingElementBuilder());
        builder = new DocumentBuilder(new CoverPageWriter(), new TableOfContentsWriter(), new HeaderFooterWriter(), bodyWriter);
    }

    [Fact]
    public async Task Generate_ProducesSchemaValidDocument()
    {
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(
            title: ["Title"],
            subtitle: ["Subtitle"],
            heading1: TestConfigurations.BuildHeading(numbering: HeadingNumberingStyle.Decimal, tableOfContents: true, pageBreakAfter: true),
            heading2: TestConfigurations.BuildHeading(numbering: HeadingNumberingStyle.Decimal, numberingPrefix: true, tableOfContents: true),
            header: TestConfigurations.BuildSection(left: TestConfigurations.BuildLines("Header"), pageNumbers: PageNumbersMode.Center),
            footer: TestConfigurations.BuildSection(
                left: [new PageContentLine { Line = "First footer", Mode = PageContentMode.Cover }],
                pageNumbers: PageNumbersMode.Right));

        string markdown = "# One\n\n## Sub\n\ntext\n\n```mermaid\ngraph TD\nA-->B\n```\n\n| A | B |\n| --- | --- |\n| 1 | 2 |\n\n# Two";

        using MemoryStream output = new();
        await builder.Generate(configuration, markdown, output, CancellationToken.None);
        output.Position = 0;

        using WordprocessingDocument document = WordprocessingDocument.Open(output, false);
        OpenXmlValidator validator = new(FileFormatVersions.Office2019);
        List<ValidationErrorInfo> errors = [.. validator.Validate(document)];

        Assert.Empty(errors);
    }

    [Fact]
    public async Task Generate_NoHeadingInTableOfContents_OmitsTocPage()
    {
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(title: ["Title"]);

        using MemoryStream output = new();
        await builder.Generate(configuration, "# Heading", output, CancellationToken.None);
        output.Position = 0;

        using WordprocessingDocument document = WordprocessingDocument.Open(output, false);
        Assert.DoesNotContain(document.MainDocumentPart!.Document!.Body!.Descendants<Text>(), text => text.Text == "Table of Contents");
    }
}

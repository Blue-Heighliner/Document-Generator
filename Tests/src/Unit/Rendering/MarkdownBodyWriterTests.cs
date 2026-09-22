namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Rendering;

public sealed class MarkdownBodyWriterTests
{
    private readonly Mock<IMermaidImageRenderer> mermaidImageRenderer = new();
    private readonly MarkdownBodyWriter writer;

    public MarkdownBodyWriterTests()
    {
        mermaidImageRenderer
            .Setup(renderer => renderer.Render(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MermaidImage { Data = TestImages.BuildMinimalPng(), Width = 10, Height = 10 });

        writer = new MarkdownBodyWriter(new HeadingNumberer(), new InlineRunWriter(), new TableConverter(), mermaidImageRenderer.Object, new DrawingElementBuilder());
    }

    [Fact]
    public async Task Write_Headings_ApplyConfiguredNumberingAndOutlineLevel()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(heading1: TestConfigurations.BuildHeading(numbering: HeadingNumberingStyle.Decimal));

        await writer.Write(mainPart, configuration, "# First\n\n# Second", CancellationToken.None);

        List<Paragraph> headingParagraphs = [.. mainPart.Document!.Body!.Elements<Paragraph>().Where(paragraph => paragraph.ParagraphProperties?.OutlineLevel is not null)];
        Assert.Equal(2, headingParagraphs.Count);
        Assert.Contains("1", headingParagraphs[0].InnerText);
        Assert.Contains("2", headingParagraphs[1].InnerText);
        Assert.Equal(0, headingParagraphs[0].ParagraphProperties!.OutlineLevel!.Val!.Value);
    }

    [Fact]
    public async Task Write_Heading_AppliesConfiguredItalicAndColor()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(heading1: TestConfigurations.BuildHeading(italic: true, color: "#ABCDEF"));

        await writer.Write(mainPart, configuration, "# Heading", CancellationToken.None);

        Run run = mainPart.Document!.Body!.Descendants<Run>().First(r => r.InnerText == "Heading");
        Assert.NotNull(run.RunProperties?.Italic);
        Assert.Equal("ABCDEF", run.RunProperties?.Color?.Val?.Value);
    }

    [Fact]
    public async Task Write_PageBreakAfterLevel_InsertsBreakBeforeNextHeadingAtThatLevel()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(heading1: TestConfigurations.BuildHeading(pageBreakAfter: true));

        await writer.Write(mainPart, configuration, "# First\n\ncontent\n\n# Second", CancellationToken.None);

        List<OpenXmlElement> children = [.. mainPart.Document!.Body!.ChildElements];
        int breakIndex = children.FindIndex(child => child.Descendants<Break>().Any(b => b.Type?.Value == BreakValues.Page));
        int secondHeadingIndex = children.FindIndex(child => child.InnerText.Contains("Second"));

        Assert.True(breakIndex >= 0 && breakIndex < secondHeadingIndex);
    }

    [Fact]
    public async Task Write_NoPageBreakConfigured_NoBreakInserted()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument();

        await writer.Write(mainPart, configuration, "# First\n\ncontent\n\n# Second", CancellationToken.None);

        Assert.DoesNotContain(mainPart.Document!.Body!.Descendants<Break>(), b => b.Type?.Value == BreakValues.Page);
    }

    [Fact]
    public async Task Write_FencedCodeBlock_UsesMonospaceFontAndGrayShading()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument();

        await writer.Write(mainPart, configuration, "```\ncode line\n```", CancellationToken.None);

        Paragraph paragraph = mainPart.Document!.Body!.Elements<Paragraph>().Single(p => p.InnerText.Contains("code line"));
        Assert.Equal("D9D9D9", paragraph.ParagraphProperties?.Shading?.Fill);
        Assert.Equal("Consolas", paragraph.Descendants<Run>().First().RunProperties?.RunFonts?.Ascii);
    }

    [Fact]
    public async Task Write_MermaidCodeBlock_EmbedsImageInsteadOfCodeText()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument();

        await writer.Write(mainPart, configuration, "```mermaid\ngraph TD\nA-->B\n```", CancellationToken.None);

        Assert.Single(mainPart.Document!.Body!.Descendants<Drawing>());
        Assert.DoesNotContain(mainPart.Document.Body.Descendants<Text>(), text => text.Text.Contains("graph TD"));
        mermaidImageRenderer.Verify(renderer => renderer.Render(It.Is<string>(diagram => diagram.Contains("graph TD")), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Write_Table_ProducesWordTable()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument();

        await writer.Write(mainPart, configuration, "| A | B |\n| --- | --- |\n| 1 | 2 |", CancellationToken.None);

        Assert.Single(mainPart.Document!.Body!.Elements<Table>());
    }

    [Fact]
    public async Task Write_List_ProducesOneParagraphPerItem()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument();

        await writer.Write(mainPart, configuration, "- First\n- Second", CancellationToken.None);

        Assert.Contains(mainPart.Document!.Body!.Elements<Paragraph>(), p => p.InnerText.Contains("First"));
        Assert.Contains(mainPart.Document.Body.Elements<Paragraph>(), p => p.InnerText.Contains("Second"));
    }
}

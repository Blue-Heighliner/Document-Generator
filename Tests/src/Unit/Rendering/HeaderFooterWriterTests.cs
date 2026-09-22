namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Rendering;

public sealed class HeaderFooterWriterTests
{
    private readonly HeaderFooterWriter writer = new();
    private readonly double printableWidth = 468;

    [Fact]
    public void WriteHeader_ModeAll_AppearsOnBothDefaultAndFirstParts()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(left: TestConfigurations.BuildLines("Every"));

        HeaderFooterPartIds ids = writer.WriteHeader(mainPart, configuration, printableWidth);

        HeaderPart defaultPart = (HeaderPart)mainPart.GetPartById(ids.DefaultId);
        HeaderPart firstPart = (HeaderPart)mainPart.GetPartById(ids.FirstId);
        Assert.Contains("Every", defaultPart.Header!.InnerText);
        Assert.Contains("Every", firstPart.Header!.InnerText);
    }

    [Fact]
    public void WriteHeader_ModeBodyAndCover_OnlyAppearOnTheirOwnPart()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(left: [
            new PageContentLine { Line = "Body line", Mode = PageContentMode.Body },
            new PageContentLine { Line = "Cover line", Mode = PageContentMode.Cover },
        ]);

        HeaderFooterPartIds ids = writer.WriteHeader(mainPart, configuration, printableWidth);

        HeaderPart defaultPart = (HeaderPart)mainPart.GetPartById(ids.DefaultId);
        HeaderPart firstPart = (HeaderPart)mainPart.GetPartById(ids.FirstId);
        Assert.Contains("Body line", defaultPart.Header!.InnerText);
        Assert.DoesNotContain("Cover line", defaultPart.Header!.InnerText);
        Assert.Contains("Cover line", firstPart.Header!.InnerText);
        Assert.DoesNotContain("Body line", firstPart.Header!.InnerText);
    }

    [Fact]
    public void WriteFooter_PageNumbers_AddsPageField()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(pageNumbers: PageNumbersMode.Center);

        HeaderFooterPartIds ids = writer.WriteFooter(mainPart, configuration, printableWidth);

        FooterPart defaultPart = (FooterPart)mainPart.GetPartById(ids.DefaultId);
        Assert.Contains(defaultPart.Footer!.Descendants<FieldCode>(), fieldCode => fieldCode.Text.Contains("PAGE"));
    }

    [Fact]
    public void WriteFooter_PageNumbersNone_HasNoPageField()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(pageNumbers: PageNumbersMode.None);

        HeaderFooterPartIds ids = writer.WriteFooter(mainPart, configuration, printableWidth);

        FooterPart defaultPart = (FooterPart)mainPart.GetPartById(ids.DefaultId);
        Assert.Empty(defaultPart.Footer!.Descendants<FieldCode>());
    }

    [Fact]
    public void WriteHeader_EmptyContent_StillProducesOneEmptyParagraph()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection();

        HeaderFooterPartIds ids = writer.WriteHeader(mainPart, configuration, printableWidth);

        HeaderPart firstPart = (HeaderPart)mainPart.GetPartById(ids.FirstId);
        Assert.Single(firstPart.Header!.Elements<Paragraph>());
    }

    [Fact]
    public void WriteHeader_LeftOnly_IsLeftJustifiedWithNoTabs()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(left: TestConfigurations.BuildLines("Left line"));

        HeaderFooterPartIds ids = writer.WriteHeader(mainPart, configuration, printableWidth);

        HeaderPart defaultPart = (HeaderPart)mainPart.GetPartById(ids.DefaultId);
        Paragraph paragraph = defaultPart.Header!.Elements<Paragraph>().Single();
        Assert.Equal(JustificationValues.Left, paragraph.ParagraphProperties!.Justification!.Val!.Value);
        Assert.Null(paragraph.ParagraphProperties.Tabs);
    }

    [Fact]
    public void WriteHeader_LeftAndRightAtSameIndex_ShareOneRowViaTabStops()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(
            left: [new PageContentLine { Line = "Left line" }],
            right: [new PageContentLine { Line = "Right line" }]);

        HeaderFooterPartIds ids = writer.WriteHeader(mainPart, configuration, printableWidth);

        HeaderPart defaultPart = (HeaderPart)mainPart.GetPartById(ids.DefaultId);
        Paragraph paragraph = defaultPart.Header!.Elements<Paragraph>().Single();
        Assert.Equal(["Left line", string.Empty, "Right line"], paragraph.Descendants<Text>().Select(text => text.Text));

        List<TabStop> tabStops = [.. paragraph.ParagraphProperties!.Tabs!.Elements<TabStop>()];
        TabStop centerStop = tabStops.Single(tab => tab.Val!.Value == TabStopValues.Center);
        TabStop rightStop = tabStops.Single(tab => tab.Val!.Value == TabStopValues.Right);
        Assert.Equal((int)(printableWidth * 20 / 2), centerStop.Position!.Value);
        Assert.Equal((int)(printableWidth * 20), rightStop.Position!.Value);
    }

    [Fact]
    public void WriteHeader_MismatchedColumnLengths_ExtraLinesGetTheirOwnRow()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(
            left: [new PageContentLine { Line = "Left 1" }, new PageContentLine { Line = "Left 2" }],
            right: [new PageContentLine { Line = "Right 1" }]);

        HeaderFooterPartIds ids = writer.WriteHeader(mainPart, configuration, printableWidth);

        HeaderPart defaultPart = (HeaderPart)mainPart.GetPartById(ids.DefaultId);
        List<Paragraph> paragraphs = [.. defaultPart.Header!.Elements<Paragraph>()];
        Assert.Equal(2, paragraphs.Count);
        Assert.Contains("Right 1", paragraphs[0].InnerText);
        Assert.Equal(JustificationValues.Left, paragraphs[1].ParagraphProperties!.Justification!.Val!.Value);
        Assert.Equal("Left 2", paragraphs[1].InnerText);
    }

    [Fact]
    public void WriteHeader_StacksTopDown_FirstLineIsTheFirstParagraph()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(left: TestConfigurations.BuildLines("First", "Second"));

        HeaderFooterPartIds ids = writer.WriteHeader(mainPart, configuration, printableWidth);

        HeaderPart defaultPart = (HeaderPart)mainPart.GetPartById(ids.DefaultId);
        List<Paragraph> paragraphs = [.. defaultPart.Header!.Elements<Paragraph>()];
        Assert.Equal("First", paragraphs[0].InnerText);
        Assert.Equal("Second", paragraphs[1].InnerText);
    }

    [Fact]
    public void WriteFooter_StacksBottomUp_FirstLineIsTheLastParagraph()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(left: TestConfigurations.BuildLines("First", "Second"));

        HeaderFooterPartIds ids = writer.WriteFooter(mainPart, configuration, printableWidth);

        FooterPart defaultPart = (FooterPart)mainPart.GetPartById(ids.DefaultId);
        List<Paragraph> paragraphs = [.. defaultPart.Footer!.Elements<Paragraph>()];
        Assert.Equal("Second", paragraphs[0].InnerText);
        Assert.Equal("First", paragraphs[1].InnerText);
    }

    [Fact]
    public void WriteFooter_PageNumbers_IsAlwaysTheFinalParagraph()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(left: TestConfigurations.BuildLines("First", "Second"), pageNumbers: PageNumbersMode.Center);

        HeaderFooterPartIds ids = writer.WriteFooter(mainPart, configuration, printableWidth);

        FooterPart defaultPart = (FooterPart)mainPart.GetPartById(ids.DefaultId);
        Paragraph lastParagraph = defaultPart.Footer!.Elements<Paragraph>().Last();
        Assert.Contains(lastParagraph.Descendants<FieldCode>(), fieldCode => fieldCode.Text.Contains("PAGE"));
    }

    [Fact]
    public void WriteHeader_PageNumbers_IsAlwaysTheFirstParagraph()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(left: TestConfigurations.BuildLines("First", "Second"), pageNumbers: PageNumbersMode.Center);

        HeaderFooterPartIds ids = writer.WriteHeader(mainPart, configuration, printableWidth);

        HeaderPart defaultPart = (HeaderPart)mainPart.GetPartById(ids.DefaultId);
        Paragraph firstParagraph = defaultPart.Header!.Elements<Paragraph>().First();
        Assert.Contains(firstParagraph.Descendants<FieldCode>(), fieldCode => fieldCode.Text.Contains("PAGE"));
    }

    [Fact]
    public void WriteFooter_PageNumbersRight_SetsThePageNumberParagraphsJustification()
    {
        MainDocumentPart mainPart = TestDocumentParts.CreateMainPart();
        PageSectionConfiguration configuration = TestConfigurations.BuildSection(pageNumbers: PageNumbersMode.Right);

        HeaderFooterPartIds ids = writer.WriteFooter(mainPart, configuration, printableWidth);

        FooterPart defaultPart = (FooterPart)mainPart.GetPartById(ids.DefaultId);
        Paragraph paragraph = defaultPart.Footer!.Elements<Paragraph>().Single(p => p.Descendants<FieldCode>().Any());
        Assert.Equal(JustificationValues.Right, paragraph.ParagraphProperties!.Justification!.Val!.Value);
    }
}

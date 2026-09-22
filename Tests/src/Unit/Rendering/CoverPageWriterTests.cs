namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Rendering;

public sealed class CoverPageWriterTests
{
    private readonly CoverPageWriter writer = new();

    [Fact]
    public void Write_AppendsOneCenteredParagraphPerTitleAndSubtitleLine()
    {
        Body body = new();
        DocumentConfiguration configuration = BuildConfiguration(title: ["Title One", "Title Two"], subtitle: ["Sub One"]);

        writer.Write(body, configuration, availableHeight: 648);

        List<Paragraph> paragraphs = [.. body.Elements<Paragraph>()];
        Assert.Contains(paragraphs, paragraph => paragraph.InnerText == "Title One");
        Assert.Contains(paragraphs, paragraph => paragraph.InnerText == "Title Two");
        Assert.Contains(paragraphs, paragraph => paragraph.InnerText == "Sub One");
        Assert.All(paragraphs, paragraph => Assert.Equal(JustificationValues.Center, paragraph.ParagraphProperties?.Justification?.Val?.Value));
    }

    [Fact]
    public void Write_TitleIsBold_SubtitleIsNot()
    {
        Body body = new();
        DocumentConfiguration configuration = BuildConfiguration(title: ["Title"], subtitle: ["Sub"]);

        writer.Write(body, configuration, availableHeight: 648);

        Run titleRun = body.Descendants<Run>().First(run => run.InnerText == "Title");
        Run subtitleRun = body.Descendants<Run>().First(run => run.InnerText == "Sub");
        Assert.NotNull(titleRun.RunProperties?.Bold);
        Assert.Null(subtitleRun.RunProperties?.Bold);
    }

    [Fact]
    public void Write_ItalicAndColorAreConfigurable()
    {
        Body body = new();
        DocumentConfiguration configuration = new()
        {
            Title = new CoverTextConfiguration { Lines = ["Title"], Italic = true, Color = "#ABCDEF" },
            Subtitle = new CoverTextConfiguration { Lines = ["Sub"] },
        };

        writer.Write(body, configuration, availableHeight: 648);

        Run titleRun = body.Descendants<Run>().First(run => run.InnerText == "Title");
        Run subtitleRun = body.Descendants<Run>().First(run => run.InnerText == "Sub");
        Assert.NotNull(titleRun.RunProperties?.Italic);
        Assert.Equal("ABCDEF", titleRun.RunProperties?.Color?.Val?.Value);
        Assert.Null(subtitleRun.RunProperties?.Italic);
    }

    [Fact]
    public void Write_EmptyTitleAndSubtitle_AppendsNothing()
    {
        Body body = new();
        DocumentConfiguration configuration = BuildConfiguration(title: [], subtitle: []);

        writer.Write(body, configuration, availableHeight: 648);

        Assert.Empty(body.Elements<Paragraph>());
    }

    [Fact]
    public void Write_MultipleLines_OnlyTheFirstLineOfEachBlockGetsSpacingBefore()
    {
        Body body = new();
        DocumentConfiguration configuration = BuildConfiguration(title: ["Title One", "Title Two"], subtitle: ["Sub One", "Sub Two"]);

        writer.Write(body, configuration, availableHeight: 648);

        List<Paragraph> paragraphs = [.. body.Elements<Paragraph>()];
        Assert.True(double.Parse(paragraphs[0].ParagraphProperties!.SpacingBetweenLines!.Before!.Value!) > 0);
        Assert.Equal("0", paragraphs[1].ParagraphProperties!.SpacingBetweenLines!.Before!.Value);
        Assert.True(double.Parse(paragraphs[2].ParagraphProperties!.SpacingBetweenLines!.Before!.Value!) > 0);
        Assert.Equal("0", paragraphs[3].ParagraphProperties!.SpacingBetweenLines!.Before!.Value);
    }

    [Fact]
    public void Write_TitleAndSubtitle_FirstSubtitleLineGetsTheConfiguredGap()
    {
        Body body = new();
        DocumentConfiguration configuration = BuildConfiguration(title: ["Title"], subtitle: ["Sub"]);

        writer.Write(body, configuration, availableHeight: 648);

        Paragraph subtitleParagraph = body.Elements<Paragraph>().Single(paragraph => paragraph.InnerText == "Sub");
        Assert.Equal("400", subtitleParagraph.ParagraphProperties!.SpacingBetweenLines!.Before!.Value);
    }

    [Fact]
    public void Write_ContentTallerThanAvailableHeight_ClampsTopSpacingToZero()
    {
        Body body = new();
        DocumentConfiguration configuration = BuildConfiguration(title: ["Title"], subtitle: ["Sub"]);

        writer.Write(body, configuration, availableHeight: 1);

        Paragraph titleParagraph = body.Elements<Paragraph>().First(paragraph => paragraph.InnerText == "Title");
        Assert.Equal("0", titleParagraph.ParagraphProperties!.SpacingBetweenLines!.Before!.Value);
    }

    [Fact]
    public void Write_EveryLine_UsesExactLineSpacing()
    {
        Body body = new();
        DocumentConfiguration configuration = BuildConfiguration(title: ["Title"], subtitle: ["Sub"]);

        writer.Write(body, configuration, availableHeight: 648);

        Assert.All(body.Elements<Paragraph>(), paragraph =>
        {
            SpacingBetweenLines spacing = paragraph.ParagraphProperties!.SpacingBetweenLines!;
            Assert.Equal(LineSpacingRuleValues.Exact, spacing.LineRule!.Value);
            Assert.True(double.Parse(spacing.Line!.Value!) > 0);
        });
    }

    private static DocumentConfiguration BuildConfiguration(IReadOnlyList<string> title, IReadOnlyList<string> subtitle) =>
        TestConfigurations.BuildDocument(title: title, subtitle: subtitle);
}

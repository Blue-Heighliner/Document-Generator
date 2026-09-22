namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Rendering;

public sealed class InlineRunWriterTests
{
    private readonly InlineRunWriter writer = new();

    [Fact]
    public void Write_NullInline_ReturnsEmpty()
    {
        Assert.Empty(writer.Write(null));
    }

    [Fact]
    public void Write_PlainText_ProducesSingleUnformattedRun()
    {
        ContainerInline inline = ParseParagraphInline("plain text");

        Run run = Assert.Single(writer.Write(inline).Cast<Run>());
        Assert.Equal("plain text", run.GetFirstChild<Text>()!.Text);
        Assert.Null(run.RunProperties);
    }

    [Fact]
    public void Write_Bold_SetsBoldProperty()
    {
        ContainerInline inline = ParseParagraphInline("**bold**");

        Run run = Assert.Single(writer.Write(inline).Cast<Run>());
        Assert.NotNull(run.RunProperties?.Bold);
        Assert.Null(run.RunProperties?.Italic);
    }

    [Fact]
    public void Write_Italic_SetsItalicProperty()
    {
        ContainerInline inline = ParseParagraphInline("*italic*");

        Run run = Assert.Single(writer.Write(inline).Cast<Run>());
        Assert.NotNull(run.RunProperties?.Italic);
        Assert.Null(run.RunProperties?.Bold);
    }

    [Fact]
    public void Write_BoldItalic_SetsBothProperties()
    {
        ContainerInline inline = ParseParagraphInline("***both***");

        Run run = Assert.Single(writer.Write(inline).Cast<Run>());
        Assert.NotNull(run.RunProperties?.Bold);
        Assert.NotNull(run.RunProperties?.Italic);
    }

    [Fact]
    public void Write_CodeSpan_UsesMonospaceFontAndShading()
    {
        ContainerInline inline = ParseParagraphInline("`code`");

        Run run = Assert.Single(writer.Write(inline).Cast<Run>());
        Assert.Equal("Consolas", run.RunProperties?.RunFonts?.Ascii);
        Assert.Equal("D9D9D9", run.RunProperties?.Shading?.Fill);
    }

    [Fact]
    public void Write_HardLineBreak_ProducesBreakRun()
    {
        ContainerInline inline = ParseParagraphInline("line one  \nline two");

        List<OpenXmlElement> elements = [.. writer.Write(inline)];
        Assert.Contains(elements, element => element is Run run && run.GetFirstChild<Break>() is not null);
    }

    private static ContainerInline ParseParagraphInline(string markdown)
    {
        MarkdownDocument document = Markdig.Markdown.Parse(markdown);
        ParagraphBlock paragraph = Assert.IsType<ParagraphBlock>(document[0]);
        return paragraph.Inline!;
    }
}

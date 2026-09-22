namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Rendering;

public sealed class TableConverterTests
{
    private readonly TableConverter converter = new();
    private readonly InlineRunWriter inlineRunWriter = new();

    [Fact]
    public void Convert_ProducesOneRowPerMarkdownRow()
    {
        Table wordTable = Convert("""
            | A | B |
            | --- | --- |
            | 1 | 2 |
            | 3 | 4 |
            """);

        Assert.Equal(3, wordTable.Elements<TableRow>().Count());
    }

    [Fact]
    public void Convert_HeaderRow_IsBold()
    {
        Table wordTable = Convert("""
            | A | B |
            | --- | --- |
            | 1 | 2 |
            """);

        TableRow headerRow = wordTable.Elements<TableRow>().First();
        Assert.All(headerRow.Descendants<Run>(), run => Assert.NotNull(run.RunProperties?.Bold));
    }

    [Fact]
    public void Convert_DataRow_IsNotBold()
    {
        Table wordTable = Convert("""
            | A | B |
            | --- | --- |
            | 1 | 2 |
            """);

        TableRow dataRow = wordTable.Elements<TableRow>().Last();
        Assert.All(dataRow.Descendants<Run>(), run => Assert.Null(run.RunProperties?.Bold));
    }

    [Fact]
    public void Convert_PreservesCellText()
    {
        Table wordTable = Convert("""
            | Name | Value |
            | --- | --- |
            | Alpha | 1 |
            """);

        Assert.Contains(wordTable.Descendants<Text>(), text => text.Text == "Alpha");
        Assert.Contains(wordTable.Descendants<Text>(), text => text.Text == "1");
    }

    private Table Convert(string markdown)
    {
        MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UsePipeTables().Build();
        MarkdownDocument document = Markdig.Markdown.Parse(markdown, pipeline);
        Markdig.Extensions.Tables.Table table = Assert.IsType<Markdig.Extensions.Tables.Table>(document[0]);
        return converter.Convert(table, inlineRunWriter);
    }
}

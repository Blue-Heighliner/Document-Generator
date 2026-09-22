namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Rendering;

public sealed class TableOfContentsWriterTests
{
    private readonly TableOfContentsWriter writer = new();

    [Fact]
    public void Write_AppendsHeadingAndTocField()
    {
        Body body = new();

        writer.Write(body, [1, 2, 3]);

        Assert.Contains(body.Descendants<Text>(), text => text.Text == "Table of Contents");
        Assert.Contains(body.Descendants<FieldCode>(), fieldCode => fieldCode.Text.Contains("TOC"));
    }

    [Fact]
    public void Write_ContiguousLevels_ProducesSingleRange()
    {
        Body body = new();

        writer.Write(body, [1, 2, 3]);

        FieldCode fieldCode = body.Descendants<FieldCode>().Single();
        Assert.Contains("\\o \"1-3\"", fieldCode.Text);
    }

    [Fact]
    public void Write_NonContiguousLevels_ProducesMultipleRanges()
    {
        Body body = new();

        writer.Write(body, [1, 3, 4]);

        FieldCode fieldCode = body.Descendants<FieldCode>().Single();
        Assert.Contains("\\o \"1-1;3-4\"", fieldCode.Text);
    }

    [Fact]
    public void Write_UnsortedDuplicateLevels_AreNormalized()
    {
        Body body = new();

        writer.Write(body, [2, 1, 2, 1]);

        FieldCode fieldCode = body.Descendants<FieldCode>().Single();
        Assert.Contains("\\o \"1-2\"", fieldCode.Text);
    }
}

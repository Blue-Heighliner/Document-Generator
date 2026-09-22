namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Markdown;

public sealed class MarkdownContentTrimmerTests
{
    private readonly MarkdownContentTrimmer trimmer = new();

    [Fact]
    public void Trim_Disabled_ReturnsInputUnchanged()
    {
        string markdown = "before\n---\ncontent\n---\nafter";
        Assert.Equal(markdown, trimmer.Trim(markdown, excludeSurroundingHorizontalBars: false));
    }

    [Fact]
    public void Trim_Enabled_KeepsOnlyContentBetweenOuterBars()
    {
        string markdown = "before\n\n---\n\n# Heading\n\ncontent\n\n---\n\nafter";
        string result = trimmer.Trim(markdown, excludeSurroundingHorizontalBars: true);

        Assert.DoesNotContain("before", result);
        Assert.DoesNotContain("after", result);
        Assert.Contains("# Heading", result);
        Assert.Contains("content", result);
    }

    [Fact]
    public void Trim_Enabled_FewerThanTwoBars_ReturnsInputUnchanged()
    {
        string markdown = "before\n---\nafter";
        Assert.Equal(markdown, trimmer.Trim(markdown, excludeSurroundingHorizontalBars: true));
    }

    [Theory]
    [InlineData("---")]
    [InlineData("***")]
    [InlineData("___")]
    [InlineData("- - -")]
    public void Trim_Enabled_RecognizesAllBarStyles(string bar)
    {
        string markdown = $"before\n{bar}\ncontent\n{bar}\nafter";
        string result = trimmer.Trim(markdown, excludeSurroundingHorizontalBars: true);

        Assert.Equal("content", result);
    }

    [Fact]
    public void Trim_Enabled_MoreThanTwoBars_UsesFirstAndLast()
    {
        string markdown = "before\n---\nfirst\n---\nsecond\n---\nafter";
        string result = trimmer.Trim(markdown, excludeSurroundingHorizontalBars: true);

        Assert.Equal("first\n---\nsecond", result);
    }
}

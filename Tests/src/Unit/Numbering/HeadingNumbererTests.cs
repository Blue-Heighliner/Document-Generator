namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Numbering;

public sealed class HeadingNumbererTests
{
    private static HeadingConfiguration BuildStyle(HeadingNumberingStyle numbering, bool numberingPrefix) =>
        TestConfigurations.BuildHeading(numbering: numbering, numberingPrefix: numberingPrefix);

    [Fact]
    public void Number_UnconfiguredLevel_ReturnsNull()
    {
        HeadingNumberer numberer = new();
        Assert.Null(numberer.Number(1, TestConfigurations.BuildDocument()));
    }

    [Fact]
    public void Number_NoneNumberingStyle_ReturnsNull()
    {
        HeadingNumberer numberer = new();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(heading1: BuildStyle(HeadingNumberingStyle.None, numberingPrefix: false));
        Assert.Null(numberer.Number(1, configuration));
    }

    [Fact]
    public void Number_SingleLevel_IncrementsSequentially()
    {
        HeadingNumberer numberer = new();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(heading1: BuildStyle(HeadingNumberingStyle.Decimal, numberingPrefix: false));

        Assert.Equal("1", numberer.Number(1, configuration));
        Assert.Equal("2", numberer.Number(1, configuration));
        Assert.Equal("3", numberer.Number(1, configuration));
    }

    [Fact]
    public void Number_WithoutNumberingPrefix_IsIndependentPerLevel()
    {
        HeadingNumberer numberer = new();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(
            heading1: BuildStyle(HeadingNumberingStyle.Decimal, numberingPrefix: false),
            heading2: BuildStyle(HeadingNumberingStyle.Decimal, numberingPrefix: false));

        Assert.Equal("1", numberer.Number(1, configuration));
        Assert.Equal("1", numberer.Number(2, configuration));
        Assert.Equal("2", numberer.Number(2, configuration));
    }

    [Fact]
    public void Number_WithNumberingPrefix_BuildsDottedPath()
    {
        HeadingNumberer numberer = new();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(
            heading1: BuildStyle(HeadingNumberingStyle.Decimal, numberingPrefix: false),
            heading2: BuildStyle(HeadingNumberingStyle.Decimal, numberingPrefix: true));

        Assert.Equal("1", numberer.Number(1, configuration));
        Assert.Equal("1.1", numberer.Number(2, configuration));
        Assert.Equal("1.2", numberer.Number(2, configuration));
    }

    [Fact]
    public void Number_NewShallowerHeading_ResetsDeeperCounters()
    {
        HeadingNumberer numberer = new();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(
            heading1: BuildStyle(HeadingNumberingStyle.Decimal, numberingPrefix: false),
            heading2: BuildStyle(HeadingNumberingStyle.Decimal, numberingPrefix: true));

        Assert.Equal("1", numberer.Number(1, configuration));
        Assert.Equal("1.1", numberer.Number(2, configuration));
        Assert.Equal("2", numberer.Number(1, configuration));
        Assert.Equal("2.1", numberer.Number(2, configuration));
    }

    [Fact]
    public void Number_PrefixSkipsAncestorLevelsWithoutNumbering()
    {
        HeadingNumberer numberer = new();
        DocumentConfiguration configuration = TestConfigurations.BuildDocument(
            heading1: BuildStyle(HeadingNumberingStyle.Decimal, numberingPrefix: false),
            heading2: BuildStyle(HeadingNumberingStyle.None, numberingPrefix: false),
            heading3: BuildStyle(HeadingNumberingStyle.Decimal, numberingPrefix: true));

        Assert.Equal("1", numberer.Number(1, configuration));
        Assert.Null(numberer.Number(2, configuration));
        Assert.Equal("1.1", numberer.Number(3, configuration));
    }
}

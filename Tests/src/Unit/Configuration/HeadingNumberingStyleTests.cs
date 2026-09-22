namespace BlueHeighliner.DocumentGenerator.Tests.Unit.Configuration;

public sealed class HeadingNumberingStyleTests
{
    [Theory]
    [InlineData((int)HeadingNumberingStyle.None, 3, "")]
    [InlineData((int)HeadingNumberingStyle.Decimal, 3, "3")]
    [InlineData((int)HeadingNumberingStyle.UpperRoman, 4, "IV")]
    [InlineData((int)HeadingNumberingStyle.LowerRoman, 4, "iv")]
    [InlineData((int)HeadingNumberingStyle.UpperLetter, 2, "B")]
    [InlineData((int)HeadingNumberingStyle.LowerLetter, 2, "b")]
    public void Format_FormatsAccordingToStyle(int style, int number, string expected)
    {
        Assert.Equal(expected, ((HeadingNumberingStyle)style).Format(number));
    }
}
